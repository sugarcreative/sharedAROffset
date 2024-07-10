using Niantic.Lightship.AR.LocationAR;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    private LocalPlayer _localPlayer;

    public TMP_Text networkPositionText;

    public ulong localID;
    private PlayerPositionManager PosManager;
    private GameObject ReferenceObject;

    //public GameObject arlocation;

    public override void OnNetworkSpawn()
    {
        //networkPositionText.text = "are we evening spawning";
        if (IsOwner)
        {
            networkPositionText.text = "Are we spawning with ownership";
            _localPlayer = FindObjectOfType<LocalPlayer>();
            //arlocation = FindObjectOfType<ARLocation>().gameObject;
            //transform.SetParent(arlocation.transform, true);
            localID = NetworkManager.Singleton.LocalClientId;
            
        }

        PosManager = FindObjectOfType<PlayerPositionManager>();
        ReferenceObject = PosManager.ReferenceObject;

        base.OnNetworkSpawn();
    }

    void Update()
    {
        //networkPositionText.text = transform.position.ToString();

        if (IsOwner)
        {
            networkPositionText.text = transform.position.ToString();

            if (_localPlayer != null)
            {

                Vector3 posOffset = Vector3.zero;
                Quaternion rotOffset = Quaternion.identity;

                if(localID == 0) {
                    posOffset = PosManager.P1_Position.Value;
                    rotOffset = PosManager.P1_Rotation.Value;
                } else {
                    posOffset = PosManager.P2_Position.Value;
                    rotOffset = PosManager.P2_Rotation.Value;
                }

                transform.SetPositionAndRotation(ReferenceObject.transform.position + posOffset, ReferenceObject.transform.rotation * rotOffset);

                //transform.SetPositionAndRotation(_localPlayer.transform.localPosition, _localPlayer.transform.localRotation);
                //transform.SetPositionAndRotation(_localPlayer.transform.position, _localPlayer.transform.rotation);
            }
        } else
        {
            networkPositionText.gameObject.SetActive(false);
            //networkPositionText.text = $"this client is {NetworkManager.Singleton.LocalClientId}";
        }
    }
}
