using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor
{
    public abstract class ModelDownload
    {
        public static string ExtractFileIdFromUrl(string url)
        {
            var patterns = new[]
            {
                @"drive\.google\.com/file/d/(.*?)/",
                @"drive\.google\.com/open\?id=(.*?)(?:$|\&)",
                @"drive\.google\.com/uc\?id=(.*?)(?:$|\&)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(url, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return url;
        }

        public static async void DownloadFileAsync(FileEntry entry, FileType type, string path)
        {
            string tempDownloadURL;
            string extensionName;
            if (type == FileType.Model)
            {
                tempDownloadURL = entry.DownloadUrl;
                extensionName = "fbx";
            }
            else 
            {
                tempDownloadURL = entry.TexturesDownloadUrl;
                extensionName = "rar";
            }
       
            if (string.IsNullOrEmpty(tempDownloadURL))
            {
                Debug.LogError("Download URL is empty!");
                return;
            }

            try
            {
                string fileId = ExtractFileIdFromUrl(tempDownloadURL);
                Debug.Log("Download URL:" + tempDownloadURL);

                var fileRequest = GoogleLoginAuthentication.DriveService.Files.Get(fileId);
                fileRequest.Fields = "id, mimeType";
                var file = await fileRequest.ExecuteAsync();

                if (file.MimeType != "application/octet-stream" && !file.MimeType.EndsWith("fbx") && !file.MimeType.EndsWith("rar") && !file.MimeType.EndsWith("compressed"))
                {
                    Debug.LogError($"O arquivo não é um modelo 3D (.fbx) nem um arquivo de texturas (.rar): {file.MimeType}");
                    return;
                }

                if (file.MimeType.EndsWith("compressed"))
                    extensionName = "zip";

                using (var stream = new MemoryStream())
                {
                    await fileRequest.DownloadAsync(stream);
                    path = Path.Combine(path, entry.Name);
                    Directory.CreateDirectory(path);
                    string filePath = Path.Combine(path, $"{entry.Name}."+extensionName);
                    File.WriteAllBytes(filePath, stream.ToArray());
                    Debug.Log($"File saved to: {filePath}");
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download file: {e.Message}\n" +
                               $"Stack Trace: {e.StackTrace}");
            }

            if (type != FileType.Textures) return;
            Enum.TryParse(extensionName, out CompressedFilesManager.FileType fileType);
            CompressedFilesManager.UncompressFile(fileType, Path.Combine(path, $"{entry.Name}." + extensionName), path);
        }
    }
}