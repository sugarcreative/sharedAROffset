using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class LocalPlayer : MonoBehaviour
{  
    // Update is called once per frame
    void Update()
    {
        DoRaycast();
    }

    private void DoRaycast() {
        if (Input.touchCount > 0 && Input.touchCount < 2 && Input.GetTouch(0).phase == TouchPhase.Began) {
            Touch touch = Input.GetTouch(0);

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0) {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
                return;
            }

            TouchToRay(touch.position);
        }
    }

    private void TouchToRay(Vector3 touch) {

        Ray ray = Camera.main.ScreenPointToRay(touch);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.transform.CompareTag("Floor")) {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 pos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
