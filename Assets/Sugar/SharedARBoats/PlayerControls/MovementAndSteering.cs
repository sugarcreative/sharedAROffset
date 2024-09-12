using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MovementAndSteering : MonoBehaviour
{

    #region GestureDetection
    private bool _isClockwise;
    private bool _isGestureDetected;
    private bool _isSliderInteracted = false;
    public bool CanTurnWheel;
    private string _hoveredGameObjectName;
    private string _sliderName = "handle";
    private string _readyButtonName = "ReadyButton";
    //private string _shootButtonName = "button";
    //private string _wheelName = "wheel";
    #endregion

    #region Steering
    [SerializeField] private float _wheelRotation = 0f;
    private Vector2 _wheelRotationClamp = new Vector2(-170f, 170f);
    [SerializeField] private float _steeringMultiplier = 0.1f;
    [SerializeField] private float _rotationSpeed = 80f;
    [SerializeField] private float _counterRotationSpeed = 70f;
    #endregion

    #region Sailing
    [SerializeField] private float _minSlider = 0.1f;
    [SerializeField] private float _maxSlider = 1f;
    [SerializeField] private float _boatSpeed;
    [SerializeField] private float _boatSpeedMult;
    [SerializeField] private float _timeSinceLastSliderChange;

    private float _currentSliderValue;
    #endregion

    private Rigidbody _rb;
    public Slider _slider;

    #region debugging
    public bool _pauseUpdate = false;
    [SerializeField] private bool _pauseDebug = true;

    [SerializeField] CircularGestureDetection _circularGestureDetection;
    #endregion

    public bool ignoreWheelLock = false;

    [SerializeField] private bool _gestureWasLastInput;

    //public Image _theWheel;

    public GameObject _wheelAsset;

    #region DEFAULT FUNCTIONS
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        FindObjectOfType<CircularGestureDetection>().GestureChanged.AddListener(GestureDetected);
        FindObjectOfType<CircularGestureDetection>().GestureDirection.AddListener(GestureAngle);

        _slider.minValue = _minSlider;
        _slider.maxValue = _maxSlider;
        _slider.onValueChanged.AddListener(SliderChanged);

    }

    private void OnEnable()
    {
        ResetWheel();
    }

    public void ResetWheel()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _wheelRotation = 0f;
        _wheelAsset.transform.rotation = Quaternion.Euler(_wheelAsset.transform.rotation.eulerAngles.x, _wheelAsset.transform.rotation.eulerAngles.y, -_wheelRotation);
        _wheelRotation = 0;
        _slider.value = 0;
    }

    //private void FixedUpdate()
    //{
    //    if (_pauseUpdate) return;
    //    Sailing();
    //}

    //private void Update()
    //{


    //    _hoveredGameObjectName = GetUIElement();

    //    if (_pauseUpdate) return;



    //    if (!_pauseDebug)
    //    {
    //        //Debug.Log(_currentSliderValue);
    //    }



    //    if (Input.touchCount == 0) // maybe change this to Input.GetMouseButton(0)?
    //    {
    //        Debug.Log("Setting wheel lock false");
    //        CanTurnWheel = false;
    //    }
    //    else
    //    {
    //        CanTurnWheel = true;
    //    }

    //    if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
    //    {
    //        // make it so that first finger on slider means check for other fingers and allow them to rotate the wheel

    //        if ((_hoveredGameObjectName == _sliderName) || _isSliderInteracted)
    //        {
    //            Debug.Log("randomSpace");
    //            CanTurnWheel = false;

    //        }
    //        else
    //        {
    //            Debug.Log("Setting wheel lock true");
    //            CanTurnWheel = true;
    //        }
    //        //Debug.Log(_hoveredGameObjectName + " - hovered Object. " + _isSliderInteracted + " - sliderInteracted");
    //    }


    //    _timeSinceLastSliderChange += Time.deltaTime;

    //    Steering();
    //}

    private void Update()
    {

        AllowGestureDetection(_isSliderInteracted);

        if (_pauseUpdate) return;

        if (Input.GetMouseButton(0))
        {
            if (_isGestureDetected)
            {
                _slider.interactable = false;
                _gestureWasLastInput = true;
                //shooting buttons not interactable
            }
            else
            {
                _slider.interactable = true;
                //shooting buttons interactable
            }
        }
        else
        {
            _slider.interactable = true;
            _gestureWasLastInput = false;
        }

        UpdatedSteering(Input.GetMouseButton(0));
        Sailing();
    }

    #endregion

    public void SliderChanged(float value)
    {
        _currentSliderValue = value;

        _timeSinceLastSliderChange = 0;

        _isSliderInteracted = true;
    }

    public void OnPointerDown()
    {
        _isSliderInteracted = true;
    }

    public void OnPointerUp()
    {
        _isSliderInteracted = false;
    }

    private void Sailing()
    {
        if (!_pauseDebug)
        {
            //Debug.Log(transform.forward);
            //Debug.Log(_currentSliderValue);
        }

        float targetSpeed = (_currentSliderValue * _boatSpeedMult);
        _boatSpeed = Mathf.Lerp(_boatSpeed, targetSpeed, 100 * Time.fixedDeltaTime);

        Vector3 force = transform.forward * _boatSpeed;
        _rb.AddForce(force, ForceMode.VelocityChange);

    }

    private void AllowGestureDetection(bool sliderInteracted)
    {
        if (sliderInteracted)
        {
            _circularGestureDetection.enabled = false;
        }
        else
        {
            _circularGestureDetection.enabled = true;
        }
    }

    private void UpdatedSteering(bool input)
    {
        if (input)
        {
            if (_isGestureDetected)
            {
                _wheelRotation += _isClockwise ? -_rotationSpeed * Time.deltaTime : +_rotationSpeed * Time.deltaTime;

            }
            else
            {
                if (_gestureWasLastInput)
                {
                    
                }
                else
                {
                    _wheelRotation = Mathf.MoveTowards(_wheelRotation, 0f, _counterRotationSpeed * Time.deltaTime);
                }
            }
        } else
        {
            _wheelRotation = Mathf.MoveTowards(_wheelRotation, 0f, _counterRotationSpeed * Time.deltaTime);
        }

        _wheelRotation = Mathf.Clamp(_wheelRotation, _wheelRotationClamp.x, _wheelRotationClamp.y);
        _wheelAsset.transform.rotation = Quaternion.Euler(_wheelAsset.transform.rotation.eulerAngles.x, _wheelAsset.transform.rotation.eulerAngles.y, -_wheelRotation);


        transform.Rotate(0f, -_wheelRotation * _steeringMultiplier * Time.deltaTime, 0f);
    }

    //private void Steering()
    //{
    //    //Debug.Log("We are doing steering");
    //    if (CanTurnWheel)
    //    {
    //        if (_isGestureDetected)
    //        {
    //            //Debug.Log("We are getting the gesture but wheel is locked");
    //            if (_hoveredGameObjectName == _sliderName || _isSliderInteracted || _timeSinceLastSliderChange <= 0.2f) return;

    //            _wheelRotation += _isClockwise ? -_rotationSpeed * Time.deltaTime : +_rotationSpeed * Time.deltaTime;
    //        }
    //    }
    //    else
    //    {
    //        //Debug.Log("We are getting the gesture and the wheel is NOT locked");
    //        _wheelRotation = Mathf.MoveTowards(_wheelRotation, 0f, _counterRotationSpeed * Time.deltaTime);
    //    }

    //    _wheelRotation = Mathf.Clamp(_wheelRotation, _wheelRotationClamp.x, _wheelRotationClamp.y);
    //    _wheelAsset.transform.rotation = Quaternion.Euler(_wheelAsset.transform.rotation.eulerAngles.x, _wheelAsset.transform.rotation.eulerAngles.y, -_wheelRotation);
    //    //_theWheel.transform.rotation = Quaternion.Euler(0f, 0f, _wheelRotation);


    //    transform.Rotate(0f, -_wheelRotation * _steeringMultiplier * Time.deltaTime, 0f);
    //}

    private string GetUIElement()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            if (results.Count > 0)
            {
                GameObject hoveredGameObject = results[0].gameObject;
                return hoveredGameObject.name;
            }
        }
        return null;
    }

    #region UNITY EVENT FUNCTIONS
    private void GestureAngle(bool isClockwise)
    {
        _isClockwise = isClockwise;
    }

    private void GestureDetected(bool isGestureDetected)
    {
        _isGestureDetected = isGestureDetected;
        //Debug.Log("Gesture is detected in bool" + isGestureDetected);
    }
    #endregion

}
