using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    private LocalPlayer _localPlayer;

    public TMP_Text networkPositionText;

    public MeshRenderer[] mesh;

    //public GameObject arlocation;

    public override void OnNetworkSpawn()
    {
        //networkPositionText.text = "are we evening spawning";
        if (IsOwner)
        {
            //mesh = GetComponent<MeshRenderer>();
            mesh = GetComponentsInChildren<MeshRenderer>();
            //mesh.enabled = false;
            foreach (MeshRenderer mr in mesh)
            {
                mr.enabled = false;
            }
            //networkPositionText.text = "Are we spawning with ownership";
            _localPlayer = FindObjectOfType<LocalPlayer>();
            //arlocation = FindObjectOfType<ARLocation>().gameObject;
            //transform.SetParent(arlocation.transform, true);

        }

        base.OnNetworkSpawn();
    }

    void Update()
    {
        //networkPositionText.text = transform.position.ToString();

        if (IsOwner)
        {
            networkPositionText.text = NetworkManager.Singleton.LocalClientId + " // " + transform.position.ToString();
            networkPositionText.gameObject.SetActive(false);

            if (_localPlayer != null)
            {
                //networkPositionText.gameObject.SetActive(true);
                //transform.SetPositionAndRotation(_localPlayer.transform.localPosition, _localPlayer.transform.localRotation);
                transform.SetPositionAndRotation(_localPlayer.transform.position, _localPlayer.transform.rotation);
            }
        } else
        {
            networkPositionText.gameObject.SetActive(true);
            //networkPositionText.text = $"this client is {NetworkManager.Singleton.LocalClientId}";
        }
    }
}
