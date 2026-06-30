using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor
{
    public class CategoryButton
    {
        public string Name;
        public Category Category;
        public bool IsSelected;

        public CategoryButton(string name, Category category, bool isSelected)
        {
            Name = name;
            Category = category;
            IsSelected = isSelected;
        }
    }

    public enum FileType
    {
        Model,
        Textures
    }

    [Serializable]
    public class FileEntry
    {
        public string Name;
        public string DriveUrl;
        public string FileId;
        public string TextureID;
        public Texture2D Thumbnail;
        public string DownloadUrl;
        public string TexturesDownloadUrl;
        public string Tag;
        public string Project;
        public string Category;
    }

    public class GoogleSheetsWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Category _currentCategorySelected = Category.Todos;
        private float _thumbnailSize = 50f;
        private Dictionary<string, FileEntry> _fileEntries = new();

        // Pagination variables
        private int _currentPage = 0;
        private int _itemsPerPage = 50;

        // Raw data storage
        private List<IList<object>> _rawData = new();

        // Cache for loaded pages
        private Dictionary<int, List<FileEntry>> _pageCache = new();
        private string _selectedFolderPath;
        private List<FileEntry> _pageEntries = new List<FileEntry>();

        private string _searchText = "";
        private static SemaphoreSlim _thumbnailSemaphore = new(5);
        private List<CategoryButton> _categoryButtons = new List<CategoryButton>();
        
        private const string SpreedSheetId = "1e4ugRNyQgW4vUX8PoUVoYpRH3_OJBfGcTUJQr5JDL1A";
        private const string SpreedSheetName = "Respostas ao formulário 1";
        
        static void ClearPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear Editor Preferences", "Are you sure you want to delete all EditorPrefs? This action cannot be undone.", "Yes", "No"))
            {
                EditorPrefs.DeleteAll();
                Debug.Log("All EditorPrefs have been deleted.");
            }
        }
        [MenuItem("RD3/Google Sheets File List")]
        public static void ShowWindow()
        {
            GetWindow<GoogleSheetsWindow>("Sheets Files");
            GoogleLoginAuthentication.IsAuthenticated = false;
        }

        private void OnEnable()
        {
            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                if (category == Category.None) 
                    continue;
        
                var button = new CategoryButton(category.ToString(), category, category == Category.Todos);
                _categoryButtons.Add(button);
            }
        }

        private void OnGUI()
        {
            if (EditorPrefs.HasKey("spreadsheetID") && EditorPrefs.HasKey("spreadsheetName"))
            {
                GoogleLoginAuthentication.SpreadsheetId = EditorPrefs.GetString("spreadsheetID");
                GoogleLoginAuthentication.SheetName = EditorPrefs.GetString("spreadsheetName");

                if (EditorPrefs.HasKey("selectedFolderPath"))
                    _selectedFolderPath = EditorPrefs.GetString("selectedFolderPath");
            }
            else
            {
                GoogleLoginAuthentication.SpreadsheetId = SpreedSheetId;
                GoogleLoginAuthentication.SheetName = SpreedSheetName;
            }

            GUILayout.Label("Google Sheets File List", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GoogleLoginAuthentication.SpreadsheetId =
                EditorGUILayout.TextField("Spreadsheet ID:", GoogleLoginAuthentication.SpreadsheetId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GoogleLoginAuthentication.SheetName =
                EditorGUILayout.TextField("Sheet Name:", GoogleLoginAuthentication.SheetName);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Folder Selection", EditorStyles.boldLabel);

            // Display the current selection
            GUILayout.Label("Selected Folder: " + _selectedFolderPath);
            if (GUILayout.Button("Clear Editor Prefs"))
            {
                ClearPrefs();
            }
            if (GUILayout.Button("Select Folder"))
            {
                // Open the folder selection dialog
                string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");

                if (!string.IsNullOrEmpty(path))
                {
                    _selectedFolderPath = path;
                    EditorPrefs.SetString("selectedFolderPath", _selectedFolderPath);
                }
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space();

            _thumbnailSize = EditorGUILayout.Slider("Thumbnail Size", _thumbnailSize, 30f, 100f);

            EditorGUILayout.Space();

            if (!GoogleLoginAuthentication.IsAuthenticated)
            {
                if (GUILayout.Button("Authenticate with Google"))
                {
                    SaveCustomWindowValues(GoogleLoginAuthentication.SpreadsheetId,
                        GoogleLoginAuthentication.SheetName);
                    GoogleLoginAuthentication.AuthenticateAsync((async () => { CallMethod(); }));
                }
            }
            else
            {
                if (GUILayout.Button("Refresh File List"))
                {
                    CallMethod();
                }

                // Pagination controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Previous Page") && _currentPage > 0)
                {
                    _currentPage--;
                    LoadPage(_currentPage);
                }

                GUILayout.Label(
                    $"Page {_currentPage + 1} of {Mathf.CeilToInt((float)_pageEntries.Count / _itemsPerPage)}");
                if (GUILayout.Button("Next Page") && (_currentPage + 1) * _itemsPerPage < _rawData.Count)
                {
                    _currentPage++;
                    LoadPage(_currentPage);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _searchText = EditorGUILayout.TextField("Search:", _searchText);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_searchText == "")
                    {
                        Refresh();
                        SearchCategory();
                    }
                }

                if (GUILayout.Button("Search"))
                {
                    /*if (_currentCategorySelected != Category.Todos)
                        LoadAllEntries();*/
                    if (_searchText != "")
                        SearchText();
                }

                EditorGUILayout.EndHorizontal();

                // Category filter
         
                int columns = 5;
                EditorGUILayout.BeginHorizontal();
                
                for (int i = 0; i < _categoryButtons.Count; i++)
                {
                    var category = _categoryButtons[i].Category;
                    bool isSelected = _categoryButtons[i].IsSelected;
                    if(category == Category.None) continue;
                    bool pressed = GUILayout.Toggle(isSelected, category.ToString(), GUI.skin.button);
                    
                    if (pressed != isSelected)
                    {
                        if (pressed)
                        {
                            if (category == Category.Todos)
                            {
                                _currentCategorySelected = Category.Todos;
                                foreach (var t in _categoryButtons)
                                    t.IsSelected = t.Category == Category.Todos;
                            }
                            else
                            {
                                if(_currentCategorySelected == Category.Todos)
                                    _currentCategorySelected &= ~Category.Todos;
                            
                                _currentCategorySelected |= category;
                                _categoryButtons[i].IsSelected = true;

                                foreach (var t in _categoryButtons.Where(t => t.Category == Category.Todos))
                                    t.IsSelected = false;
                            }
                        }
                        else
                        {
                            _currentCategorySelected &= ~category;
                            _categoryButtons[i].IsSelected = false;

                            if (_currentCategorySelected == 0)
                            {
                                _currentCategorySelected = Category.Todos;
                                foreach (var t in _categoryButtons)
                                    t.IsSelected = t.Category == Category.Todos;
                            }

                            Refresh();
                        }
                        
                        Debug.Log($"Categorias selecionadas: {_currentCategorySelected}");
                        SearchCategory();
                    }

          
                    if ((i + 1) % columns == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
                
                EditorGUILayout.EndHorizontal();


                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                // Display only the items for the current page
                if (_pageCache.TryGetValue(_currentPage, out var value))
                {
                    foreach (var entry in value)
                    {
                        EditorGUILayout.BeginHorizontal(GUI.skin.box);

                        if (entry.Thumbnail != null)
                            GUILayout.Label(entry.Thumbnail, GUILayout.Width(_thumbnailSize),
                                GUILayout.Height(_thumbnailSize));
                        else
                            GUILayout.Label("Loading...", GUILayout.Width(_thumbnailSize),
                                GUILayout.Height(_thumbnailSize));


                        GUILayout.BeginVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(entry.Name);
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Download"))
                        {
                            ModelDownload.DownloadFileAsync(entry, FileType.Model, _selectedFolderPath);
                            ModelDownload.DownloadFileAsync(entry, FileType.Textures, _selectedFolderPath);
                        }

                        GUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void Refresh()
        {
            LoadAllEntries();
            LoadFirstPage();
        }

        private void SearchCategory()
        {
            var filteredEntries =
                SearchTool.SearchByCategory(_pageEntries.ToList(), _currentCategorySelected);
  
            _pageEntries = filteredEntries;
            LoadFirstPage();
        }

        private void SearchText()
        {
            var filteredEntries = SearchTool.SearchBy(_pageEntries.ToList(), _searchText);
            _pageEntries = filteredEntries;
            LoadFirstPage();
        }

        private void LoadFirstPage()
        {
            _pageCache.Clear();
            _currentPage = 0;
            LoadPage(_currentPage);
        }

            
        private async void CallMethod()
        {
            await GoogleLoginAuthentication.FetchFileListAsync((response) =>
            {
                // Store raw data
                _rawData = (List<IList<object>>)response.Values;
                LoadAllEntries();
                _pageCache.Clear();
                // Load the first page
                LoadPage(0);
            });
        }

        private void LoadPage(int page)
        {
            if (_pageCache.ContainsKey(page))
                return;

            List<FileEntry> pageList = new List<FileEntry>();

            int startIndex = page * _itemsPerPage;
            int endIndex = Mathf.Min(startIndex + _itemsPerPage, _pageEntries.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var entry = _pageEntries[i];

                // Load thumbnail async (only once)
                if (entry.Thumbnail == null && !string.IsNullOrEmpty(entry.FileId))
                {
                    LoadThumbnailAsync(entry);
                }

                pageList.Add(entry);
            }

            _pageCache[page] = pageList;

            Repaint();
        }

        private void LoadAllEntries()
        {
            _pageEntries = new List<FileEntry>();

            for (int i = 0; i < _rawData.Count; i++)
            {
                var row = _rawData[i];

                if (i == 0) continue;

                if (row.Count >= 6 && row[0] != null && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    var entry = new FileEntry
                    {
                        Name = row[1].ToString(),
                        DriveUrl = row[4]?.ToString(),
                        FileId = ModelDownload.ExtractFileIdFromUrl(row[4]?.ToString()),
                        TextureID = ModelDownload.ExtractFileIdFromUrl(row[6]?.ToString()),
                        DownloadUrl = row[5]?.ToString(),
                        TexturesDownloadUrl = row[6]?.ToString(),
                        Category = row[0]?.ToString(),
                        Tag = row[3]?.ToString(),
                        Project = row[8]?.ToString()
                    };

                    _pageEntries.Add(entry);
                }
            }
        }


        private async void LoadThumbnailAsync(FileEntry entry)
        {
            try
            {
                await _thumbnailSemaphore.WaitAsync();

                var fileRequest = GoogleLoginAuthentication.DriveService.Files.Get(entry.FileId);
                fileRequest.Fields = "id, mimeType";
                var file = await fileRequest.ExecuteAsync();

                if (!file.MimeType.StartsWith("image/"))
                {
                    Debug.LogError($"O arquivo não é uma imagem: {file.MimeType}");
                    return;
                }

                string imageUrl = $"https://drive.google.com/thumbnail?id={entry.FileId}";

                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    string accessToken =
                        ((Google.Apis.Auth.OAuth2.ITokenAccess)GoogleLoginAuthentication.DriveService
                            .HttpClientInitializer).GetAccessTokenForRequestAsync().Result;
                    webRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");

                    var tcs = new TaskCompletionSource<bool>();

                    webRequest.SendWebRequest().completed += (asyncOperation) =>
                    {
                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                            entry.Thumbnail = texture;
                            Repaint();
                            tcs.SetResult(true);
                        }
                        else
                        {
                            Debug.LogError($"Erro ao baixar thumbnail: {webRequest.error}");
                            tcs.SetException(new Exception(webRequest.error));
                        }
                    };

                    await tcs.Task;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load thumbnail for {entry.Name}: {e.Message}");
            }
            finally
            {
                _thumbnailSemaphore.Release();
            }
        }


        private void SaveCustomWindowValues(string spreadsheetID, string spreadsheetName)
        {
            EditorPrefs.SetString("spreadsheetID", spreadsheetID);
            EditorPrefs.SetString("spreadsheetName", spreadsheetName);
        }

        private void OnDestroy()
        {
            foreach (var entry in _fileEntries.Values.Where(entry => entry.Thumbnail != null))
                DestroyImmediate(entry.Thumbnail);
        }
    }
}