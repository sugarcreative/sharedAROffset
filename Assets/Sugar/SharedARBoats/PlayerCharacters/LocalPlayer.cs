using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{

    public SkinnedMeshRenderer[] skinnedMeshRenderer;
    public MeshRenderer[] meshRenderer;

    public GameObject canvas;

    void Awake()
    {
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        meshRenderer = GetComponentsInChildren<MeshRenderer>(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        GetComponentInChildren<ShipEffectController>().IsRespawn();
    }

    public void SetColor(FixedString64Bytes colorArg)
    {
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        meshRenderer = GetComponentsInChildren<MeshRenderer>(true);
        Color newCol;
        
        if (ColorUtility.TryParseHtmlString(colorArg.ToString(), out newCol))
        {
            foreach (SkinnedMeshRenderer mr in skinnedMeshRenderer)
            {
                mr.material.SetColor("_Tint", newCol);
            }
            foreach (MeshRenderer mr in meshRenderer)
            {
                mr.material.SetColor("_Tint", newCol);
            }
        }
    }


    public void ShowCanvas()
    {
        canvas.GetComponent<ModalFade>().Show();
    }

    public void HideCanvas()
    {
        canvas.GetComponent<ModalFade>().Hide();
    }
}
