using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastPlacement : MonoBehaviour
{
    [SerializeField]
    private GameObject localAvatar;

    [SerializeField]
    private Camera _camera;

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
            SetLocalAvatarToLocation(pos, rot);
            _canPlace = false;
        }
    }

    void SetLocalAvatarToLocation(Vector3 position, Quaternion rotation)
    {
        localAvatar.transform.SetPositionAndRotation(position, rotation);
        localAvatar.gameObject.SetActive(true);
    }
}
