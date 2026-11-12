using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace RD3_Utilities
{
    /// <summary>
    /// Utility to request device permissions.
    /// </summary>
    public class RequestPermission : MonoBehaviour
    {
        [SerializeField] bool keepScreenOn = true;
        [SerializeField] bool cameraPermission = true;
        [SerializeField] bool microphone = false;
        [SerializeField] bool writeToGallery = false;
        [SerializeField] bool fineLocation = false;

        void Start()
        {
            Application.targetFrameRate = 30;
            if (keepScreenOn) Screen.sleepTimeout = SleepTimeout.NeverSleep;

            StartCoroutine(RequestPermissions());
        }

        /// <summary>
        /// Routine to fire all the permission requests signalized in the booleans.
        /// </summary>
        /// 
        IEnumerator RequestPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (cameraPermission && !Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Camera));
        }
        if (microphone && !Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Microphone));
        }
        if(writeToGallery && !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite));
        }
        if (fineLocation && !Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
        }
#elif UNITY_IOS && !UNITY_EDITOR

        if(cameraPermission && !Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            yield return new WaitUntil(() => Application.HasUserAuthorization(UserAuthorization.WebCam));
        }
        if (microphone && !Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
            yield return new WaitUntil(() => Application.HasUserAuthorization(UserAuthorization.Microphone));
        }
#endif

          //  yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
          yield return null;
        }
    }
}

