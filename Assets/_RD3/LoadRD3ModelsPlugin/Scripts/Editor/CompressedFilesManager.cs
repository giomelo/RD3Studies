using System.IO;
using System.Linq;
using _RD3.LoadRD3ModelsPlugin.Scripts.Editor.Material_Generator;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using UnityEditor;
using UnityEngine;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor
{
    public class CompressedFilesManager : UnityEditor.Editor
    {
        public enum FileType
        {
            zip,
            rar
        }


        public static void UncompressFile(FileType fileType, string filePath, string folderPath)
        {
            Debug.Log("Uncompressing File..." + fileType);
            if (fileType == FileType.rar)
            {
                using (var archive = RarArchive.Open(filePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(folderPath, new ExtractionOptions()
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                    }
                }
            }
            else
            {
                using (var archive = ZipArchive.Open(filePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(folderPath, new ExtractionOptions()
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                    }
                }

            }

            File.Delete(filePath);
            AssetDatabase.Refresh();
            Debug.Log("Textures saved in:" + folderPath);
            MaterialsGeneratorEditor.currentFolderModelPath = folderPath;
            MaterialsGeneratorEditor.ShowExample();
        }
   
    }
}
