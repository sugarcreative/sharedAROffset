using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NONARNETWORK : NetworkBehaviour
{
    [SerializeField] private TMP_InputField displayNameInputField;

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

    [SerializeField] private GameObject topBar;

    private bool _isJoined;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);

        beginButton.interactable = true;

        displayNameInputField.onValueChanged.AddListener(OnDiplayNameChanged);

        _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
        _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);
        _joinAsClientButton.interactable = false;
        _joinAsHostButton.interactable = false;

        //NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
    }
    public void ShowHostJoinPanel()
    {
        hostJoinPanel.GetComponent<ModalFade>().Show();
        CanvasDarkBG.GetComponent<ModalFade>().Show();
    }

    private void OnJoinAsHostClicked()
    {
        if (displayNameInputField.text.Equals("")) return;
      
        playerName = new FixedString64Bytes(displayNameInputField.text);

        NetworkManager.Singleton.StartHost();
        hostJoinPanel.GetComponent<ModalFade>().Hide();
        topBar.GetComponent<ModalFade>().Show();
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
        enterRoomCodePanel.GetComponent<ModalFade>().Show();
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
        playerName = new FixedString64Bytes(displayNameInputField.text);
        NetworkManager.Singleton.StartClient();
        hostJoinPanel.GetComponent<ModalFade>().Hide();
        topBar.GetComponent<ModalFade>().Show();
        _isJoined = true;
    }

    private void OnConnection(ulong clientId)
    {
        if (IsServer) return;
        NetworkEntityManager.Instance.AddNewClientToListServerRpc(clientId, playerName);
        NetworkEntityManager.Instance.SetLocalColor();

    }
}
