using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPositionManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<Vector3> P1_Position = new NetworkVariable<Vector3>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
    );

    [HideInInspector]
    public NetworkVariable<Quaternion> P1_Rotation = new NetworkVariable<Quaternion>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
    );

    [HideInInspector]
    public NetworkVariable<Vector3> P2_Position = new NetworkVariable<Vector3>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
    );

    [HideInInspector]
    public NetworkVariable<Quaternion> P2_Rotation = new NetworkVariable<Quaternion>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
    );

    public GameObject ReferenceObject;

    //Update player 1 position
    public void UpdateP1PositionAndRotation(Vector3 _Position, Quaternion _Rotation) {
        P1_Position.Value = _Position;
        P1_Rotation.Value = _Rotation;
    }

    //Update player 2 position
    public void UpdateP2PositionAndRotation(Vector3 _Position, Quaternion _Rotation) {
        P2_Position.Value = _Position;
        P2_Rotation.Value = _Rotation;
    }
}
