using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace _RD3.Adressables
{
    public class AssetBundleLoader : MonoBehaviour
    {
        private string _prefabUrl =
            "https://www.dropbox.com/s/6lu9mwishga8a8z/new%20prefab?st=yvxe9907&dl=1";

        private string _prefabName = "new prefab";

        private void Start()
        {
            StartCoroutine(LoadPrefabFromURL());
        }

        private IEnumerator LoadPrefabFromURL()
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(_prefabUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load");
                yield break;
            }

            var bundle = DownloadHandlerAssetBundle.GetContent(request);
            var prefab = bundle.LoadAsset<GameObject>(_prefabName);
            Instantiate(prefab);
            bundle.Unload(false);
            yield return null;
        }
    }
}