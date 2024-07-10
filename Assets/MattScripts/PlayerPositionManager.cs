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
    public GameObject RemotePlayer;

    private ulong localID;
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        localID = NetworkManager.Singleton.LocalClientId;

        if(localID == 0) {
            //IS PLAYER ONE, WANTS TO UPDATE P2 VALUES
            P2_Position.OnValueChanged += UpdateLocalPosition;
            P2_Rotation.OnValueChanged += UpdateLocalRotation;
        } else {
            //IS PLAYER TWO, WANTS TO UPDATE P1 VALUES
            P1_Position.OnValueChanged += UpdateLocalPosition;
            P1_Rotation.OnValueChanged += UpdateLocalRotation;
            UpdateLocalPosition(Vector3.zero, P1_Position.Value);
            UpdateLocalRotation(Quaternion.identity, P1_Rotation.Value);
        }
    }

    private void UpdateLocalRotation(Quaternion previousValue, Quaternion newValue) {     
        Quaternion playerRotation = ReferenceObject.transform.rotation * newValue;
        RemotePlayer.transform.rotation = playerRotation;
        RemotePlayer.SetActive(true);
    }

    private void UpdateLocalPosition(Vector3 previousValue, Vector3 newValue) {
        Vector3 playerPosition = ReferenceObject.transform.position + newValue;
        RemotePlayer.transform.position = playerPosition;
        RemotePlayer.SetActive(true);
    }

    //Update player 1 position
    public void UpdateP1PositionAndRotation(Vector3 _Position, Quaternion _Rotation) {
        UpdateP1ServerRpc(_Position, _Rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateP1ServerRpc(Vector3 pos, Quaternion rot) {
        P1_Position.Value = pos;
        P1_Rotation.Value = rot;
    }

    //Update player 2 position
    public void UpdateP2PositionAndRotation(Vector3 _Position, Quaternion _Rotation) {
        UpdateP2ServerRpc(_Position, _Rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateP2ServerRpc(Vector3 pos, Quaternion rot) {
        P2_Position.Value = pos;
        P2_Rotation.Value = rot;
    }
}
