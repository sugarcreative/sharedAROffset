using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.SharedAR.Colocalization;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLogic : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _statusText, _roomNameDisplayText;

    [SerializeField] private TMP_InputField displayNameInputField, roomCodeInputField;

    [SerializeField] private FixedString64Bytes playerName;

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

    [SerializeField] private Button beginButton;

    //[SerializeField] private Sprite[] buttonImages;

    private string _roomName;

    private bool _isJoined;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);

        hostJoinPanel.SetActive(false);
        enterRoomCodePanel.SetActive(false);
        beginButton.interactable = false;

        roomCodeInputField.onValueChanged.AddListener(OnRoomCodeChanged);
        displayNameInputField.onValueChanged.AddListener(OnDiplayNameChanged);

        _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
        _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);
        _joinAsClientButton.interactable = false;
        _joinAsHostButton.interactable = false;

        NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;

        _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
    }



    private void OnColocalizationTrackingStateChanged(
        SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        if (args.Tracking)
        {
            if (_isJoined) return;
            //hostJoinPanel.SetActive(true);
            beginButton.interactable = true;
        }
        else
        {
            beginButton.interactable = false;
        }
    }

    public void ShowHostJoinPanel()
    {
        //hostJoinPanel.GetComponent<ModalFade>().Show();
        hostJoinPanel.SetActive(true);
    }

    private void OnJoinAsHostClicked()
    {
        if (displayNameInputField.text.Equals("")) return;
        _roomName = Random.Range(0, 9999).ToString();
        playerName = new FixedString64Bytes(displayNameInputField.text);
        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartHost();
        hostJoinPanel.SetActive(false);
        _isJoined = true;
        NetworkEntityManager.Instance.AddNewClientToList(NetworkManager.Singleton.LocalClientId, playerName);
        NetworkEntityManager.Instance.SetLocalColor();
    }

    private void OnJoinAsClientClicked()
    {
        EnterRoomCode();
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
        playerName = new FixedString64Bytes(displayNameInputField.text);

        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartClient();
        hostJoinPanel.SetActive(false);

        _isJoined = true;
    }

    private void OnConnection(ulong clientId)
    {
        if (IsServer) return;
        NetworkEntityManager.Instance.AddNewClientToListServerRpc(clientId, playerName);
        NetworkEntityManager.Instance.SetLocalColor();

    }

    //private void OnClientConnectedCallback(ulong clientId)
    //{
    //    _statusText.text = $"{clientId} Connected";
    //}

    //private void OnClientDisconnectedCallback(ulong clientId)
    //{
    //    _statusText.text = $"{clientId} Disconnected";
    //}

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
