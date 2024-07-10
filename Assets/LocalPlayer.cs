using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LocalPlayer : MonoBehaviour
{
    public bool doSync = false;
    
    public PlayerPositionManager positionManager;
    public GameObject ReferenceObject;

    private void Start() {
        positionManager = FindObjectOfType<PlayerPositionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(doSync == false) {
            return;
        }

        Vector3 posOffset = transform.position - ReferenceObject.transform.position;
        Quaternion RotationDifference = Quaternion.Inverse(ReferenceObject.transform.rotation) * transform.rotation;

        if (NetworkManager.Singleton.LocalClientId == 0) {
            positionManager.UpdateP1PositionAndRotation(posOffset, RotationDifference);
        } else {
            positionManager.UpdateP2PositionAndRotation(posOffset, RotationDifference);
        }
    }
}
