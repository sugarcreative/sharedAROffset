using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.SharedAR.Colocalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLogic : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _statusText, _roomNameDisplayText;

    [SerializeField] private TMP_InputField displayNameInputField, roomCodeInputField;

    [SerializeField]
    private Button _joinAsHostButton;

    [SerializeField]
    private Button _joinAsClientButton;

    [SerializeField]
    private Button _roomCodeEnterButton;

    [SerializeField]
    private SharedSpaceManager _sharedSpaceManager;

    [SerializeField]
    private ARLocation arLocation;

    [SerializeField] private GameObject hostJoinPanel, enterRoomCodePanel;

    private string _roomName;

    private bool _isJoined;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);

        //_roomName = "Hello"; // have room name become random number generation, then host only has to enter name.
        hostJoinPanel.SetActive(false);
        enterRoomCodePanel.SetActive(false);
        //roomCodeInputField.gameObject.SetActive(false);
        //_joinAsClientButton.gameObject.SetActive(false);
        //_joinAsHostButton.gameObject.SetActive(false);

        roomCodeInputField.onValueChanged.AddListener(OnRoomCodeChanged);
        displayNameInputField.onValueChanged.AddListener(OnDiplayNameChanged);

        _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
        _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);
        _joinAsClientButton.interactable = false;
        _joinAsHostButton.interactable = false;


        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;


        _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
    }



    private void OnColocalizationTrackingStateChanged(
        SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        if (args.Tracking)
        {
            if (_isJoined) return;
            //_joinAsHostButton.gameObject.SetActive(true);
            //_joinAsClientButton.gameObject.SetActive(true);
            hostJoinPanel.SetActive(true);
        }
    }

    private void OnJoinAsHostClicked()
    {
        if (displayNameInputField.text.Equals("")) return;
        //Maybe have some popup that tells them to input a name.
        _roomName = Random.Range(0, 9999).ToString();

        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartHost();
        //_joinAsHostButton.gameObject.SetActive(false);
        //_joinAsClientButton.gameObject.SetActive(false);
        hostJoinPanel.SetActive(false);
        _isJoined = true;
    }

    private void OnJoinAsClientClicked()
    {

        EnterRoomCode();
        //Maybe have some popup that tells them to input a name.

        //Press the join button, a window pops up and prompts them for a room code, once entered then continue



        //var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        //var ropts = SetUpRoomAndUI(topts);
        //StartSharedSpace(topts, ropts);
        //NetworkManager.Singleton.StartClient();


        // DO SOME CHECK HERE TO SEE IF A LOBBY WITH THE PIN EXISTS
        //_joinAsHostButton.gameObject.SetActive(false);
        //_joinAsClientButton.gameObject.SetActive(false);


        //hostJoinPanel.SetActive(false);
        //_isJoined = true;
    }

    private void EnterRoomCode()
    {
        enterRoomCodePanel.SetActive(true);
    }

    private void OnRoomCodeChanged(string text)
    {
        if(!string.IsNullOrEmpty(text))
        {
            _roomCodeEnterButton.interactable = true;
        }
        else
        {
            _roomCodeEnterButton.interactable = false;
        }
    }

    private void OnDiplayNameChanged(string displayName)
    {
        if (!string.IsNullOrEmpty(displayName))
        {
            _joinAsHostButton.interactable = true;
            _joinAsClientButton.interactable = true;
        }
        else
        {
            _joinAsHostButton.interactable = false;
            _joinAsClientButton.interactable = false;
        }
    }

    public void OnConfirmButtonClicked()
    {
        _roomName = roomCodeInputField.text;
        enterRoomCodePanel.SetActive(false);

        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartClient();


        // DO SOME CHECK HERE TO SEE IF A LOBBY WITH THE PIN EXISTS
        //_joinAsHostButton.gameObject.SetActive(false);
        //_joinAsClientButton.gameObject.SetActive(false);


        hostJoinPanel.SetActive(false);
        _isJoined = true;
    } 

    private void HideButtons()
    {
        _joinAsHostButton.gameObject.SetActive(false);
        _joinAsClientButton.gameObject.SetActive(false);
    }

    private void HideHostJoinPanel()
    {
        hostJoinPanel.SetActive(false);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        _statusText.text = $"{clientId} Connected";
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        _statusText.text = $"{clientId} Disconnected";
    }

    private ISharedSpaceRoomOptions SetUpRoomAndUI(ISharedSpaceTrackingOptions topts)
    {
        _roomNameDisplayText.text = $"Pin: {_roomName}";
        _roomNameDisplayText.gameObject.SetActive(true);

        return ISharedSpaceRoomOptions.CreateVpsRoomOptions(topts, _roomName, 10, "vps_boats");
    }

    private void StartSharedSpace(ISharedSpaceTrackingOptions topts, ISharedSpaceRoomOptions ropts)
    {
        _sharedSpaceManager.StartSharedSpace(topts, ropts);
    }
}
