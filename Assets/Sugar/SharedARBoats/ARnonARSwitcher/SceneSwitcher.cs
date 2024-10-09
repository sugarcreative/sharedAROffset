using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    public enum SceneType {AR, NonAR}
    public SceneType sceneType;
    

    public GameObject ARScene;
    public GameObject NonARScene;

    
    private void OnValidate() {
        SwitchScene();
    }

    public void SwitchScene() {

        switch (sceneType) {
            case SceneType.AR:
                ARScene.SetActive(true);       
                NonARScene.SetActive(false);
                break;
            case SceneType.NonAR:
                NonARScene.SetActive(true);
                ARScene.SetActive(false);
                break;
            default:
                break;
        }
    }
}
