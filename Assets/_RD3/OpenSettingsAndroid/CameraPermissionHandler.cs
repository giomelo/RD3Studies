using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraPermissionHandler : MonoBehaviour
{
    public void OpenSettings()
    {

#if UNITY_IOS
        string url = MyNativeBindings.GetSettingsURL();
        Debug.Log("the settings url is:" + url);
        Application.OpenURL(url);
#elif UNITY_ANDROID
        try
        {
            using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            string packageName = currentActivityObject.Call<string>("getPackageName");
            using var uriClass = new AndroidJavaClass("android.net.Uri");
            using AndroidJavaObject uriObject =
                uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
            using var intentObject = new AndroidJavaObject("android.content.Intent",
                "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject);
            intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
            intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
            currentActivityObject.Call("startActivity", intentObject);
            Application.Quit();
        }
        catch
        {
            Application.Quit();
        }
#endif
    }
}