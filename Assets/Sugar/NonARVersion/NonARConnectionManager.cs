using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NonARConnectionManager : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _roomNameDisplayText;

    [SerializeField] private TMP_InputField displayNameInputField, roomCodeInputField;

    [SerializeField] private FixedString64Bytes playerName;

    [SerializeField]
    private Button _joinAsHostButton;

    [SerializeField]
    private Button _joinAsClientButton;

    [SerializeField]
    private Button _roomCodeEnterButton;

    [SerializeField] private GameObject hostJoinPanel, enterRoomCodePanel;

    [SerializeField] private Button beginButton;

    [SerializeField] private GameObject CanvasDarkBG;

    private NetworkVariable<FixedString64Bytes> roomCodeNetworkVariable = new NetworkVariable<FixedString64Bytes>();

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
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnection;
    }

    public void MockTrackingEnabledButton()
    {
        beginButton.interactable = !beginButton.interactable;
    }

    public void ShowHostJoinPanel()
    {
        hostJoinPanel.GetComponent<ModalFade>().Show();
        CanvasDarkBG.GetComponent<ModalFade>().Show();
    }

    private void OnJoinAsHostClicked()
    {
        if (displayNameInputField.text.Equals("")) return;
        _roomName = Random.Range(0, 9999).ToString();
        roomCodeNetworkVariable.Value = _roomName;

        playerName = new FixedString64Bytes(displayNameInputField.text);
        NetworkManager.Singleton.StartHost();

        hostJoinPanel.SetActive(false);
        NetworkEntityManager.Instance.AddNewClientToList(NetworkManager.Singleton.LocalClientId, playerName);
        NetworkEntityManager.Instance.SetLocalColor();

        _roomNameDisplayText.text = $"Room Code: {_roomName}";
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
        if (!string.IsNullOrEmpty(text))
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

    [ServerRpc (RequireOwnership = false)]
    private void VerifyRoomCodeServerRpc(FixedString64Bytes clientRoomCode, ulong clientID)
    {
        if (clientRoomCode == roomCodeNetworkVariable.Value)
        {
            NetworkEntityManager.Instance.AddNewClientToList(clientID, playerName);
            NetworkEntityManager.Instance.SetLocalColor();
        }
        else
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
        }
    }

    public void OnConfirmButtonClicked()
    {
        _roomName = roomCodeInputField.text;
        enterRoomCodePanel.SetActive(false);
        playerName = new FixedString64Bytes(displayNameInputField.text);
        NetworkManager.Singleton.StartClient();
        hostJoinPanel.SetActive(false);
        VerifyRoomCodeServerRpc(new FixedString64Bytes(_roomName), NetworkManager.Singleton.LocalClientId);
    }

    private void OnConnection(ulong clientId)
    {
        if (IsServer) return;
        NetworkEntityManager.Instance.AddNewClientToListServerRpc(clientId, playerName);
        NetworkEntityManager.Instance.SetLocalColor();
    }

    private void OnDisconnection(ulong clientId)
    {
        if (!IsServer)
        {
            _roomNameDisplayText.text = "Failed to join the room: Incorrect room code";
        }
    }
}
