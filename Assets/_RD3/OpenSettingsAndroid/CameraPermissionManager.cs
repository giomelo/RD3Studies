using System;
using UnityEngine;
using UnityEngine.Android;

public class CameraPermissionManager : MonoBehaviour
{
    private const string CameraPermission =
        Permission.Camera;
    
    [SerializeField]private GameObject _error;
    [SerializeField]private GameObject _sucssessMessage;

    private void Start()
    {
        CheckCameraPermission();
    }

    public void CheckCameraPermission()
    {
        if (Permission.HasUserAuthorizedPermission(CameraPermission))
        {
            Debug.Log("Permissão da câmera concedida!");
            _sucssessMessage.SetActive(true);
         //   return;
        }
        _error.SetActive(true);
        Debug.LogWarning(
            "Permissão da câmera não concedida. Abrindo configurações do app."
        );

        OpenAppSettings();
    }

    private void OpenAppSettings()
    {
        using (AndroidJavaClass unityPlayer =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject activity =
                   unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject intent =
                       new AndroidJavaObject(
                           "android.content.Intent",
                           "android.settings.APPLICATION_DETAILS_SETTINGS"))
                {
                    using (AndroidJavaClass uriClass =
                           new AndroidJavaClass("android.net.Uri"))
                    {
                        AndroidJavaObject uri =
                            uriClass.CallStatic<AndroidJavaObject>(
                                "fromParts",
                                "package",
                                activity.Call<string>("getPackageName"),
                                null
                            );

                        intent.Call<AndroidJavaObject>("setData", uri);

                        activity.Call("startActivity", intent);

                        uri.Dispose();
                    }
                }
            }
        }
    }
}