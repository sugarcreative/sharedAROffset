using Niantic.Lightship.AR.LocationAR;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ARLocationPlacement : MonoBehaviour
{
    [SerializeField]
    private GameObject localAvatar;

    [SerializeField]
    private GameObject networkAvatar;

    [SerializeField]
    private ARLocation ArLocation;

    [SerializeField]
    private Camera _camera;

    [SerializeField] private TMP_Text text;

    [SerializeField]
    private bool _canPlace = true;

    private void Update()
    {
        if (!_canPlace) return;

        if (Input.touchCount > 0 && Input.touchCount < 2 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                return;
            }


            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }
            TouchToRay(touch.position);
        }
    }

    void TouchToRay(Vector3 touch)
    {

        Ray ray = _camera.ScreenPointToRay(touch);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.transform.CompareTag("Floor"))
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 pos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            //SpawnObjectFromHitTest(pos, rot);
            SetLocalAvatarToLocation(pos, rot);
            //_canPlace = false;
        }
    }

    void SetLocalAvatarToLocation(Vector3 position, Quaternion rotation)
    {
        localAvatar.transform.SetPositionAndRotation(position, rotation);
        localAvatar.gameObject.SetActive(true);
    }

    public void SpawnObjectFromHitTest(Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate(localAvatar, position, rotation);
        go.transform.SetParent(ArLocation.transform, true);

        SpawnNetworkPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        //GameObject netGo = Instantiate(networkAvatar);
        //PlayerAvatar playerAvatar = netGo.GetComponent<PlayerAvatar>();
        //playerAvatar.Initialize(go);
        //NetworkObject networkObject = networkAvatar.GetComponent<NetworkObject>();
        //networkObject.transform.SetParent(ArLocation.transform, true);
        //networkObject.Spawn();
    }

    [ServerRpc]
    private void SpawnNetworkPlayerServerRpc(ulong clientId)
    {
        GameObject netGo = Instantiate(networkAvatar);
        //PlayerAvatar playerAvatar = netGo.GetComponent<PlayerAvatar>();
        //playerAvatar.Initialize();
        text.text = clientId.ToString();
        //NetworkObject networkObject = networkAvatar.GetComponent<NetworkObject>();
        //networkObject.transform.SetParent(ArLocation.transform, true);
        //networkObject.SpawnWithOwnership(clientId);
    }
}
