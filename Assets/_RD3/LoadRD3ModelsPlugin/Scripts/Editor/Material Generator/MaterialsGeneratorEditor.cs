using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor.Material_Generator
{
    public class MaterialsGeneratorEditor : EditorWindow
    {
        private Button _createButton;

        private const string baseTex = "Tex_Base_";
        private const string specularTex = "Tex_Spc_";
        private const string normalTex = "Tex_Nor_";
        private const string heightTex = "Tex_Hgt_";
        private const string occlusionTex = "Tex_Aoc_";
        private const string maskTex = "Tex_Mask_";
        private const string roughnessTex = "Tex_Rgh_";
        private const string emissionTex = "Tex_Ems_";
        private const string metalicTex = "Tex_Mtl_";

        private static readonly string[] texturePrefixes =
            { specularTex, normalTex, heightTex, occlusionTex, maskTex, roughnessTex, metalicTex, emissionTex };

        public static string currentFolderModelPath = string.Empty;
    
        [MenuItem("RD3/Materials Generator")]
        public static void ShowExample()
        {
            MaterialsGeneratorEditor wnd = GetWindow<MaterialsGeneratorEditor>();
            wnd.titleContent = new GUIContent("Materials Generator");
        }

        private void OnGUI()
        {
            
            GUILayout.Label("Selected Folder: " + currentFolderModelPath);

            if (GUILayout.Button("Select Folder"))
            {
                // Open the folder selection dialog
                string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");

                if (!string.IsNullOrEmpty(path))
                {
                    currentFolderModelPath = path;
                }
            }
            if (GUILayout.Button("Create Materials"))
                TryToCreateMaterials();
        }
    

        private void TryToCreateMaterials()
        {
            string path = "Assets";
            if (currentFolderModelPath != null)
            {
                path = currentFolderModelPath;
            }
            CheckForTextures(new DirectoryInfo(path));
        }
        
        
        private void CheckForTextures(DirectoryInfo directoryInfo)
        {
            var textures = new List<Texture2D>();
            _materials = new Dictionary<string, Material>();
            var files = directoryInfo.GetFiles(baseTex + "*");
            foreach (var file in files)
            {
                if (file.Name.EndsWith(".meta")) continue;
            
                var objectName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                objectName = objectName.Substring(baseTex.Length);

                if (string.IsNullOrEmpty(objectName)) continue;
                if (HasMaterial(directoryInfo, objectName)) continue;
                string fullPath = Path.Combine(directoryInfo.FullName, file.Name);
                string assetPath = "Assets" + fullPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                string folderPath = "Assets" + directoryInfo.FullName.Substring(Application.dataPath.Length).Replace("\\", "/");

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                //  var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/" + file.Name);
                textures.Add(tex);
                var maps = GetTextureMaps(directoryInfo, objectName, folderPath);

                foreach (var map in maps)
                    textures.Add(map);

                CreateMaterial(textures, folderPath, objectName);
            }

            SetTexturesToFBX(directoryInfo);
        }

        private Dictionary<string, Material> _materials;
        public void SetTexturesToFBX(DirectoryInfo directoryInfo)
        { 
            var files = directoryInfo.GetFiles("*.fbx");
            if (files.Length == 0)
            {
                Debug.LogWarning("FBX NOT FOUND");
                return;
            }
            string fullPath = Path.Combine(directoryInfo.FullName, files[0].Name);
            string assetPath = "Assets" + fullPath.Substring(Application.dataPath.Length).Replace("\\", "/");
            
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null)
            {
                Debug.LogError("ModelImporter não encontrado para o asset: " + assetPath);
                return;
            }

            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            
            modelImporter.SearchAndRemapMaterials(
                ModelImporterMaterialName.BasedOnMaterialName,
                ModelImporterMaterialSearch.Local
            );
            
            foreach (var mat in _materials)
                modelImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mat.Key), mat.Value);
            
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    
        private void CreateMaterial(List<Texture2D> textures, string path, string objectName)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            SetTextures(material, textures);
            var assetPath = Path.Combine(path, "Mat_" + objectName + ".mat").Replace("\\", "/");
            AssetDatabase.CreateAsset(material, assetPath);
            _materials.Add(material.name, material);
        }

        private List<Texture2D> GetTextureMaps(DirectoryInfo directoryInfo, string objectName, string path)
        {
            return texturePrefixes.Select(texturePrefix => GeTextureMap(directoryInfo, texturePrefix + objectName, path)).Where(texture => texture).ToList();
        }

        private Texture2D GeTextureMap(DirectoryInfo directoryInfo,string searchPattern, string path)
        {
            return (from file in directoryInfo.GetFiles(searchPattern + "*") where !file.Name.EndsWith(".meta") select AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/" + file.Name)).FirstOrDefault();
        }

        private void SetTextures(Material material, List<Texture2D> textures)
        {
            foreach (var texture in textures)
            {
                if (texture.name.Contains(baseTex))
                {
                    material.mainTexture = texture;
                    continue;
                }

                if (texture.name.Contains(specularTex))
                {
                    material.SetTexture("_SpecGlossMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(metalicTex))
                {
                    material.SetTexture("_MetallicGlossMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(normalTex))
                {
                    material.SetTexture("_BumpMap", texture);
                    material.SetTexture("_DetailNormalMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(heightTex))
                {
                    material.SetTexture("_ParallaxMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(occlusionTex))
                {
                    material.SetTexture("_OcclusionMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(maskTex))
                {
                    material.SetTexture("_DetailMask", texture);
                    continue;
                }
            
                if (texture.name.Contains(emissionTex))
                {
                    material.SetTexture("_EmissionMap", texture);
                    continue;
                }
            
                if (texture.name.Contains(roughnessTex))
                    material.SetTexture("_RoughnessMap", texture);
            }
        }

        private bool HasMaterial(DirectoryInfo directoryInfo, string objectName)
        {
            return directoryInfo.GetFiles("Mat_" + objectName + ".mat").Any(file => !file.Name.EndsWith(".meta"));
        }
    }
}