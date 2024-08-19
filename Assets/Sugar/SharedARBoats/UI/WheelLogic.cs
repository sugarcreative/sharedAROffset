using UnityEngine;

public class WheelLogic : MonoBehaviour
{
    [SerializeField] private Camera wheelCam;
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(wheelCam.transform.position);
    }
}
