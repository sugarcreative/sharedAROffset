using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    private LocalPlayer _localPlayer;

    public MeshRenderer[] mesh;

    public SkinnedMeshRenderer[] skinnedMeshRenderer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            mesh = GetComponentsInChildren<MeshRenderer>();
            skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (MeshRenderer mr in mesh)
            {
                mr.enabled = false;
            }

            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderer)
            {
                smr.enabled=false;
            }

            _localPlayer = FindObjectOfType<LocalPlayer>();
        }

        base.OnNetworkSpawn();
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
