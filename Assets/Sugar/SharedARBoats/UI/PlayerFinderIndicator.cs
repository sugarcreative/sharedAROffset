using UnityEngine;
using UnityEngine.UI;

public class PlayerFinderIndicator : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject indicatorImage;

    [SerializeField] private Camera camera;


    private void Update()
    {

        Vector3 screenPos = camera.WorldToScreenPoint(playerTransform.position);

        bool isBehindCamera = screenPos.z < 0;

        bool isOffScreen = isBehindCamera || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

        if (isOffScreen)
        {
            indicatorImage.GetComponent<ModalFade>().Show();

            if (isBehindCamera)
            {
                screenPos.x = Screen.width - screenPos.x;
                screenPos.y = Screen.height - screenPos.y;
                screenPos.z = Mathf.Abs(screenPos.z);
            }


            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            Vector2 directionFromCenter = new Vector2(screenPos.x - screenCenter.x, screenPos.y - screenCenter.y).normalized;


            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;


            indicatorImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, angle - 90);
        }
        else
        {
            indicatorImage.GetComponent<ModalFade>().Hide();
        }
    }
}