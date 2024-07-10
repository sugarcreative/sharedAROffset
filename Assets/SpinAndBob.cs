using TMPro;
using UnityEngine;

public class SpinAndBob : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;

    private Vector3 startPosition;

    public TMP_Text localPositionText;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        localPositionText.text = transform.position.ToString();

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
