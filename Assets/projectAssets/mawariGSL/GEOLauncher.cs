using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GEOLauncher : MonoBehaviour
{
    // bundle id of the geosmartlauncher. should be the same as in AndroidManifest.xml
    public string application = "com.DefaultCompany.MawariVPSnew";


    ///  >> test on the editor side
    int escCounter = 0;
    float timer = 0f;

    private void Update() {
        timer -= Time.deltaTime;
        if(timer <= 0) {
            escCounter = 0;
            timer = 0f;
        }
        if(Input.GetKeyDown(KeyCode.Escape)) {
            timer = 1f;
            escCounter++;
            Debug.Log(escCounter);
            if(escCounter == 2) {
                LaunchApplication();
            }
        }
    }
    ///  << test on the editor side

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("On Application pause");
        if (pauseStatus)
        {
            LaunchApplication();
        }
    }

    public void LaunchApplication()
    {
        bool fail = false;

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaObject launchIntent = null;

        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", application); 
        }
        catch
        {
            fail = true;
        }

        if (!fail)
        {
            currentActivity.Call("startActivity", launchIntent); //launch intent from bundle ID
        }

        unityPlayer.Dispose();
        currentActivity.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }
}
