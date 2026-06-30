using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor
{
    public abstract class GoogleLoginAuthentication
    {
        public static string SpreadsheetId;
        public static string SheetName;
        public static SheetsService SheetsService;
        public static DriveService DriveService;
        private static UserCredential _credential;
        public static bool IsAuthenticated = false;
        public static async void AuthenticateAsync(Action callback)
        {
            try
            {
                string credPath = "Assets/_RD3/LoadRD3ModelsPlugin/credentials.json";

                if (!File.Exists(credPath))
                {
                    Debug.LogError("credentials.json not found in Assets/Editor folder!");
                    return;
                }

                using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
                {
                    string credentialsPath = Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                        ".credentials/sheets.googleapis.com-dotnet-quickstart.json"
                    );

                    _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                        new[] {
                            SheetsService.Scope.SpreadsheetsReadonly,
                            DriveService.Scope.DriveReadonly
                        },
                        "user",
                        CancellationToken.None,
                        new Google.Apis.Util.Store.FileDataStore(credentialsPath, true)
                    );
                }

                SheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = "Unity Sheets Integration"
                });

                DriveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = "Unity Sheets Integration"
                });

                IsAuthenticated = true;
                Debug.Log("Authentication successful!");

                if (!string.IsNullOrEmpty(SpreadsheetId))
                {
                    callback?.Invoke();
                   // await FetchFileListAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Authentication failed: {e.Message}\nStack trace: {e.StackTrace}");
                IsAuthenticated = false;
            }
        }
        
        public static async Task FetchFileListAsync(Action<ValueRange> callBack)
        {
            if (string.IsNullOrEmpty(GoogleLoginAuthentication.SpreadsheetId))
            {
                Debug.LogError("Spreadsheet ID is empty!");
                return;
            }

            try
            {
                var spreadsheet = await GoogleLoginAuthentication.SheetsService.Spreadsheets.Get(GoogleLoginAuthentication.SpreadsheetId).ExecuteAsync();
                Debug.Log($"Successfully accessed spreadsheet: {spreadsheet.Properties.Title}");

                string range = $"'{GoogleLoginAuthentication.SheetName}'!B:J";
                Debug.Log($"Requesting range: {range}");

                var request = GoogleLoginAuthentication.SheetsService.Spreadsheets.Values.Get(GoogleLoginAuthentication.SpreadsheetId, range);
                var response = await request.ExecuteAsync();
                Debug.Log($"Received {response.Values?.Count ?? 0} rows from spreadsheet");

                callBack?.Invoke(response);
            }
            catch (Google.GoogleApiException e)
            {
                Debug.LogError($"Google API Error: {e.Message}\n" +
                               $"HTTP Status: {e.HttpStatusCode}\n" +
                               $"Error Reason: {e.Error?.Message}\n" +
                               $"Stack Trace: {e.StackTrace}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to fetch file list: {e.Message}\n" +
                               $"Stack Trace: {e.StackTrace}");
            }
        }
    }
}