using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private LocalPlayer _localPlayer;

    public MeshRenderer[] mesh;

    public SkinnedMeshRenderer[] skinnedMeshRenderer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            mesh = GetComponentsInChildren<MeshRenderer>(true);
            skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (MeshRenderer mr in mesh)
            {
                mr.enabled = false;
            }

            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderer)
            {
                smr.enabled=false;
            }

            _localPlayer = FindObjectOfType<LocalPlayer>(true);
        }

        base.OnNetworkSpawn();
    }

    public void SetColor(FixedString64Bytes colorArg)
    {
        mesh = GetComponentsInChildren<MeshRenderer>(true);
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Color newCol;
        if (ColorUtility.TryParseHtmlString(colorArg.ToString(), out newCol))
        {
            foreach (SkinnedMeshRenderer mr in skinnedMeshRenderer)
            {
                mr.material.SetColor("_Tint", newCol);
            }

            foreach (MeshRenderer mr in mesh)
            {
                mr.material.SetColor("_Tint", newCol);
            }
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (_localPlayer != null)
            {
                transform.SetPositionAndRotation(_localPlayer.transform.position, _localPlayer.transform.rotation);
            }
        }
    }
}
