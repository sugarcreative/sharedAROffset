using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.SharedAR.Colocalization;
using System;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLogic : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _statusText, _roomNameDisplayText;

    [SerializeField]
    private Button _joinAsHostButton;

    [SerializeField]
    private Button _joinAsClientButton;

    [SerializeField]
    private SharedSpaceManager _sharedSpaceManager;

    [SerializeField]
    private ARLocation arLocation;

    private string _roomName;

    private bool _isJoined;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _roomName = "Hello";
        _joinAsClientButton.gameObject.SetActive(false);
        _joinAsHostButton.gameObject.SetActive(false);


        _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
        _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);


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
            _joinAsHostButton.gameObject.SetActive(true);
            _joinAsClientButton.gameObject.SetActive(true);
        }
    }

    private void OnJoinAsHostClicked()
    {
        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartHost();
        _joinAsHostButton.gameObject.SetActive(false);
        _joinAsClientButton.gameObject.SetActive(false);
        _isJoined = true;

    }

    private void OnJoinAsClientClicked()
    {
        
        var topts = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(arLocation);
        var ropts = SetUpRoomAndUI(topts);
        StartSharedSpace(topts, ropts);
        NetworkManager.Singleton.StartClient();
        _joinAsHostButton.gameObject.SetActive(false);
        _joinAsClientButton.gameObject.SetActive(false);
        _isJoined = true;
    }

    private void HideButtons()
    {
        _joinAsHostButton.gameObject.SetActive(false);
        _joinAsClientButton.gameObject.SetActive(false);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        _statusText.text = $"{clientId} Connected";
        //GameUIManager.PlayerJoin(Convert.ToInt32(clientId.ToString()));
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        _statusText.text = $"{clientId} Disconnected";
        //GameUIManager.PlayerLeave(Convert.ToInt32(clientId.ToString()));
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
