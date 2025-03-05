using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _RD3
{
    public class AddressablesRemote : MonoBehaviour
    {
        [SerializeField] private string remoteAssetAddress;

        private void Start()
        {
            LoadRemoteAsset(remoteAssetAddress);
        }

        private void LoadRemoteAsset(string address)
        {
            Addressables.LoadAssetAsync<GameObject>(address).Completed += OnAssetLoaded;
        }

        private void OnAssetLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset carregado com sucesso
                GameObject asset = handle.Result;
                Instantiate(asset); // Instancia o asset na cena
                Debug.Log("Asset carregado com sucesso!");
            }
            else
            {
                // Falha ao carregar
                Debug.LogError("Erro ao carregar o asset: " + handle.OperationException.Message);
            }
        }
    }
}
