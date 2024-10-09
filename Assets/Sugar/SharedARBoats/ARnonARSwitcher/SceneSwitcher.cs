using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    public enum SceneType {AR, NonAR, HallwayAR}
    public SceneType sceneType;
    

    public GameObject ARScene;
    public GameObject NonARScene;
    public GameObject HallwayARScene;
    
    private void OnValidate() {
        SwitchScene();
    }

    void SwitchScene() {

        switch (sceneType) {
            case SceneType.AR:
                ARScene.SetActive(true);       
                NonARScene.SetActive(false);
                HallwayARScene.SetActive(false);
                break;
            case SceneType.NonAR:
                NonARScene.SetActive(true);
                ARScene.SetActive(false);
                HallwayARScene.SetActive(false);
                break;
            case SceneType.HallwayAR:
                HallwayARScene.SetActive(true);
                ARScene.SetActive(false);
                NonARScene.SetActive(false);
                break;
            default:
                break;
        }

    }
}
