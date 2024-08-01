using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    private LocalPlayer _localPlayer;

    public MeshRenderer[] mesh;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            mesh = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mesh)
            {
                mr.enabled = false;
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
        } else
        {
        }
    }
}
