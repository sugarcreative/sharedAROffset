using UnityEngine;
using UnityEngine.Events;

public class CircularGestureDetection : MonoBehaviour
{
    public static CircularGestureDetection Instance;

    private Vector2 startTouchPosition;
    private Vector2 previousTouchPosition;
    private bool isTouching = false;
    private float totalAngle = 0f;
    private float angleDirection = 0f;
    private bool isCircularGesture = false;
    private bool previousGesture = false;
    private bool isClockwise = false;

    public float timeBetweenGestures = .5f;
    public float timeSinceLastGesture = 0f;

    private float timeSinceLastUpdate;

    [HideInInspector]
    public UnityEventBool GestureChanged;
    [HideInInspector]
    public UnityEventBool GestureDirection;
    [HideInInspector]
    public UnityEventBool IsTouching;

    private float _rotationSpeed;
    private const float MAX_SPEED = 100f;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;
        timeSinceLastGesture = timeBetweenGestures + 1f;
    }

    void Update()
    {

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartTouch(touch.position);
                    break;
                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        UpdateTouch(touch.position);
                    }
                    break;
                case TouchPhase.Ended:
                    EndTouch();
                    break;
            }
        }

        timeSinceLastGesture += Time.deltaTime;


        if (timeSinceLastGesture <= timeBetweenGestures)
        {
            isCircularGesture = true;
        }
        else
        {
            isCircularGesture = false;
        }

        if (previousGesture != isCircularGesture)
        {
            GestureChanged.Invoke(isCircularGesture);
        }

        previousGesture = isCircularGesture;
        //Debug.Log(_rotationSpeed);
    }

    private void StartTouch(Vector2 position)
    {
        startTouchPosition = position;
        previousTouchPosition = position;
        isTouching = true;
        totalAngle = 0f;
        angleDirection = 0f;
        isCircularGesture = false;
        IsTouching?.Invoke(isTouching);
    }

    private void UpdateTouch(Vector2 currentPosition)
    {
        if (previousTouchPosition != currentPosition)
        {
            Vector2 from = previousTouchPosition - startTouchPosition;
            Vector2 to = currentPosition - startTouchPosition;
            float angle = Vector2.SignedAngle(from, to);

            totalAngle += Mathf.Abs(angle);
            angleDirection += angle;
            //Debug.Log(angleDirection);

            //NEW CODE
            timeSinceLastUpdate += Time.deltaTime;

            if (Mathf.Abs(angleDirection) > 90f)
            {
                if (angleDirection > 0)
                {
                    // Clockwise gesture detected
                    //Debug.Log("Clockwise gesture detected");
                    isClockwise = false;
                }
                else
                {
                    // Anti-clockwise gesture detected
                    //Debug.Log("Anti-clockwise gesture detected");
                    isClockwise = true;
                }
                GestureDirection.Invoke(isClockwise);
            }

            if (totalAngle > 135) //180 != half a cirle for whatever reason, no idea
            {
                _rotationSpeed = totalAngle / timeSinceLastUpdate;
                _rotationSpeed = Mathf.Clamp(_rotationSpeed, 0f, MAX_SPEED);

                timeSinceLastGesture = 0f;
                timeSinceLastUpdate = 0f;
                angleDirection = 0f;

                if (Input.touchCount > 0)
                {
                    StartTouch(Input.GetTouch(0).position);
                }
            }
            previousTouchPosition = currentPosition;
        }
    }

    private void EndTouch()
    {
        isTouching = false;
        isCircularGesture = false;
        GestureChanged.Invoke(isCircularGesture);
        //OnCircleGesture?.Invoke()
        IsTouching?.Invoke(isTouching);
    }
}

[System.Serializable]
public class UnityEventBool : UnityEvent<bool>
{

}

[System.Serializable]
public class UnityEventBoolBool : UnityEvent<bool, bool>
{

}

[System.Serializable]
public class UnityEventInt : UnityEvent<int>
{

}

[System.Serializable]
public class UnityEventString : UnityEvent<string>
{

}
