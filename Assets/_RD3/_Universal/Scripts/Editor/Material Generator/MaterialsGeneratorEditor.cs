using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MaterialsGeneratorEditor : EditorWindow
{
    private Button _createButton;

    private const string _baseTex = "Tex_Base_";
    private const string _specularTex = "Tex_Spc_";
    private const string _normalTex = "Tex_Nor_";
    private const string _heightTex = "Tex_Hgt_";
    private const string _occlusionTex = "Tex_Aoc_";
    private const string _maskTex = "Tex_Mask_";
    private const string _roughnessTex = "Tex_Rgh_";
    private const string _emissionTex = "Tex_Ems_";
    private const string _metalicTex = "Tex_Mtl_";

    private static readonly string[] _texturePrefixes =
        { _specularTex, _normalTex, _heightTex, _occlusionTex, _maskTex, _roughnessTex, _metalicTex, _emissionTex };

    [MenuItem("RD3/Materials Generator")]
    public static void ShowExample()
    {
        MaterialsGeneratorEditor wnd = GetWindow<MaterialsGeneratorEditor>();
        wnd.titleContent = new GUIContent("Materials Generator");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_RD3/_Universal/Scripts/Editor/Material Generator/MaterialsGeneratorEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_RD3/_Universal/Scripts/Editor/Material Generator/MaterialsGeneratorEditor.uss");
        root.styleSheets.Add(styleSheet);
        
        _createButton = rootVisualElement.Query<Button>("CreateMaterialsBtn");
        _createButton.clicked += TryToCreateMaterials;
    }

    private void TryToCreateMaterials()
    {
        var folders = AssetDatabase.GetSubFolders("Assets/_RD3");
        
        foreach (var folder in folders)
            Recursive(folder);
    }
    
    private void Recursive(string folder)
    {
        CheckForTextures(new DirectoryInfo(folder), folder);

        var folders = AssetDatabase.GetSubFolders(folder);
        
        foreach (var fld in folders)
            Recursive(fld);
    }

    private void CheckForTextures(DirectoryInfo directoryInfo, string path)
    {
        var textures = new List<Texture2D>();

        foreach (var file in directoryInfo.GetFiles(_baseTex + "*"))
        {
            if (file.Name.EndsWith(".meta")) continue;
            
            var objectName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            objectName = objectName.Substring(_baseTex.Length);

            if (string.IsNullOrEmpty(objectName)) continue;
            if (HasMaterial(directoryInfo, objectName)) continue;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/" + file.Name);
            textures.Add(tex);
            
            var maps = GetTextureMaps(directoryInfo, objectName, path);

            foreach (var map in maps)
                textures.Add(map);

            CreateMaterial(textures, path, objectName);
        }
    }

    private void CreateMaterial(List<Texture2D> textures, string path, string objectName)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        SetTextures(material, textures);
        AssetDatabase.CreateAsset(material, path + "/" + "Mat_" + objectName + ".mat");
    }

    private List<Texture2D> GetTextureMaps(DirectoryInfo directoryInfo, string objectName, string path)
    {
        var textures = new List<Texture2D>();
        
        foreach (var texturePrefix in _texturePrefixes)
        {
            var texture = GeTextureMap(directoryInfo, texturePrefix + objectName, path);
            
            if(!texture) continue;

            textures.Add(texture);
        }

        return textures;
    }

    private Texture2D GeTextureMap(DirectoryInfo directoryInfo,string searchPattern, string path)
    {
        foreach (var file in directoryInfo.GetFiles(searchPattern + "*"))
        {
            if (file.Name.EndsWith(".meta")) continue;
            
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/" + file.Name);
        }
        
        return null;
    }

    private void SetTextures(Material material, List<Texture2D> textures)
    {
        foreach (var texture in textures)
        {
            if (texture.name.Contains(_baseTex))
            {
                material.mainTexture = texture;
                continue;
            }

            if (texture.name.Contains(_specularTex))
            {
                material.SetTexture("_SpecGlossMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_metalicTex))
            {
                material.SetTexture("_MetallicGlossMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_normalTex))
            {
                material.SetTexture("_BumpMap", texture);
                material.SetTexture("_DetailNormalMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_heightTex))
            {
                material.SetTexture("_ParallaxMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_occlusionTex))
            {
                material.SetTexture("_OcclusionMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_maskTex))
            {
                material.SetTexture("_DetailMask", texture);
                continue;
            }
            
            if (texture.name.Contains(_emissionTex))
            {
                material.SetTexture("_EmissionMap", texture);
                continue;
            }
            
            if (texture.name.Contains(_roughnessTex))
                material.SetTexture("_RoughnessMap", texture);
        }
    }

    private bool HasMaterial(DirectoryInfo directoryInfo, string objectName)
    {
        foreach (var file in directoryInfo.GetFiles("Mat_" + objectName + ".mat"))
        {
            if (file.Name.EndsWith(".meta")) continue;

            return true;
        }
        
        return false;
    }
}