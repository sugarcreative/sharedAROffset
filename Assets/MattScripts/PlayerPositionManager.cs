using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPositionManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<Vector3> P1_Position = new NetworkVariable<Vector3>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [HideInInspector]
    public NetworkVariable<Quaternion> P1_Rotation = new NetworkVariable<Quaternion>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [HideInInspector]
    public NetworkVariable<Vector3> P2_Position = new NetworkVariable<Vector3>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [HideInInspector]
    public NetworkVariable<Quaternion> P2_Rotation = new NetworkVariable<Quaternion>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    public GameObject ReferenceObject;
    public GameObject LocalPlayer;
    public GameObject RemotePlayer;

    public TMPro.TextMeshProUGUI LocalPlayerText, RemotePlayerText;

    private bool hasSpawned = false;

    private ulong localID;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        localID = NetworkManager.Singleton.LocalClientId;

        if(localID == 0) {
            //IS PLAYER ONE, LISTEN FOR CHANGES TO P2
            P2_Position.OnValueChanged += UpdateRemotePosition;
            P2_Rotation.OnValueChanged += UpdateRemoteRotation;
        } else {
            //IS PLAYER TWO, LISTEN FOR CHANGES TO P1
            P1_Position.OnValueChanged += UpdateRemotePosition;
            P1_Rotation.OnValueChanged += UpdateRemoteRotation;
        }

        hasSpawned = true;
    }

    private void Update() {

        if(hasSpawned == false) {
            return;
        }

        // Continuously update the position and rotation of the local player
        if (localID == 0) {
            UpdateP1PositionAndRotation();
        }
        else {
            UpdateP2PositionAndRotation();
        }
    }

    private void UpdateRemoteRotation(Quaternion previousValue, Quaternion newValue) {
        //Quaternion playerRotation = ReferenceObject.transform.rotation * newValue;
        //RemotePlayer.transform.rotation = playerRotation;
        RemotePlayer.transform.localRotation = newValue;
        RemotePlayer.SetActive(true);
    }

    private void UpdateRemotePosition(Vector3 previousValue, Vector3 newValue) {
        //Vector3 playerPosition = ReferenceObject.transform.position + newValue;
        //RemotePlayer.transform.position = playerPosition;
        RemotePlayer.SetActive(true);
        RemotePlayerText.text = newValue.ToString();
        RemotePlayer.transform.localPosition = newValue;
    }

    //Update player 1 position
    public void UpdateP1PositionAndRotation() {
        //var relativePositionAndRotation = GetRelativePositionAndRotation(ReferenceObject.transform, LocalPlayer.transform);
        //UpdateP1ServerRpc(relativePositionAndRotation.Item1, relativePositionAndRotation.Item2);
        LocalPlayerText.text = LocalPlayer.transform.localPosition.ToString();
        UpdateP1ServerRpc(LocalPlayer.transform.localPosition, LocalPlayer.transform.localRotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateP1ServerRpc(Vector3 pos, Quaternion rot) {
        P1_Position.Value = pos;
        P1_Rotation.Value = rot;
    }

    //Update player 2 position
    public void UpdateP2PositionAndRotation() {
        //var relativePositionAndRotation = GetRelativePositionAndRotation(ReferenceObject.transform, LocalPlayer.transform);
        //UpdateP2ServerRpc(relativePositionAndRotation.Item1, relativePositionAndRotation.Item2);
        LocalPlayerText.text = LocalPlayer.transform.localPosition.ToString();
        UpdateP2ServerRpc(LocalPlayer.transform.localPosition, LocalPlayer.transform.localRotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateP2ServerRpc(Vector3 pos, Quaternion rot) {
        P2_Position.Value = pos;
        P2_Rotation.Value = rot;
    }

    public (Vector3, Quaternion) GetRelativePositionAndRotation(Transform reference, Transform target) {
        // Calculate relative position
        Vector3 relativePosition = reference.InverseTransformPoint(target.position);

        // Calculate relative rotation
        Quaternion relativeRotation = Quaternion.Inverse(reference.rotation) * target.rotation;

        return (relativePosition, relativeRotation);
    }
}
