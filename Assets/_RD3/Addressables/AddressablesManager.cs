using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _RD3
{
    public class AddressablesManager : MonoBehaviour
    {
        [SerializeField]private AssetReference _assetReference;
        [SerializeField]private AssetLabelReference _assetLabelReference;
        
        private void Start()
        {
           
            Addressables.LoadAssetAsync<GameObject>(_assetLabelReference).Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    Instantiate(op.Result);
                }
                else
                    Debug.LogError("Failed to load asset");

                //Addressables.Release(op);
            };
        }
    }
}

