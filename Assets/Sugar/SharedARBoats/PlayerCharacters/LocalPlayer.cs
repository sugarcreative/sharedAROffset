using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{

    public SkinnedMeshRenderer[] skinnedMeshRenderer;
    public MeshRenderer[] meshRenderer;

    void Awake()
    {
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        meshRenderer = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(FixedString64Bytes colorArg)
    {

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
}
