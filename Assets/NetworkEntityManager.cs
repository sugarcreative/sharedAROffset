using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NetworkEntityManager : NetworkBehaviour
{
    
    public static NetworkEntityManager Instance;
    
#region Server Variables
        
        private NetworkList<PlayerData> _allPlayerData;

#endregion

#region Player Variables
    
    [Space (10)]
    [SerializeField] private GameObject localPlayer;
    private MovementAndSteering _movementAndSteering;
    private ShipEffectController _shipEffectController;
    [SerializeField] private GameObject ribbon;
    [SerializeField] private Image coloredRing;
    
#endregion
    
#region Network Variables
    
    [SerializeField] private PlayerAvatar[] networkedGameObjects;
    
#endregion
    
#region UI Elements
    
    [Header("UI Elements")]
    
    [SerializeField] private TMP_Text combatLog; //This is only used for debugging
    [FormerlySerializedAs("localHealth")] [SerializeField] private TMP_Text localDeathCount;
    [SerializeField] private TMP_Text localScore;

    [SerializeField] private GameObject wheel;
    private WheelSwitcher _wheelSwitcher;
    
    [SerializeField] private GameObject scoreboardPanel;
    private ScoreboardLogic _scoreboardLogic;
    private ModalFade _scoreboardPanelModalFade;
    
    private ModalFade _playerFinderModalFade;
    
    [SerializeField] private Transform playerCardParent;
    [SerializeField] private PlayerCard playerCardPrefab;
    private Dictionary<ulong, PlayerCard> _playerCards = new Dictionary<ulong, PlayerCard>();
    
    [SerializeField] private Button readyButton;
    [SerializeField] private Button[] shootingButtons;
    
    [SerializeField] private GameObject canvasDarkBG;
    [SerializeField] private GameObject topUIBar;
    [SerializeField] private GameObject roomCodeText;
    [SerializeField] private GameObject[] uiPlayStateObjects;
    [SerializeField] private GameObject[] wheelAndWheelCam;
    [SerializeField] private GameObject playerFinder;
    
    [SerializeField] private Sprite[] readyButtonImages;
    
    private int _readyButtonImageIndex;
    
#endregion
   
#region Game Settings
    
    [Header ("Game Settings")]
    
    [SerializeField] private  int playerMaxHealth = 10;

    [SerializeField] private  int startingScore = 0;

    [SerializeField] private  int playerDamage = 1;
    
    [SerializeField] private float gameTime = 300f;

#endregion

#region Game Runtime Variables
    
    private bool _isReadyLocal;
    private bool _gameStarted;
    private bool _gettingReady;
    private bool _isFirstGo = true;
    private bool _gameEnd;
    
#endregion


    

    public bool shootingDebug = false;

    public FixedString64Bytes[] colorList = { "#AD3232", "#A59622", "#285D27", "#28A5A7", "#29447D", "#A5579E", "#00FF6E" };
    


    [SerializeField] private Transform spawnAroundObject;
    
    [SerializeField] private Transform[] spawnPoints;







    


    

#region Default Functions

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        //Get variables related to the player object
        _movementAndSteering = localPlayer.GetComponent<MovementAndSteering>();
        _movementAndSteering._pauseUpdate = true;
        _shipEffectController = localPlayer.GetComponentInChildren<ShipEffectController>();
        
        //Get object to switch between different wheel damage states
        _wheelSwitcher = wheel.GetComponent<WheelSwitcher>();
        
        _scoreboardPanelModalFade = scoreboardPanel.GetComponent<ModalFade>();
        _scoreboardLogic = scoreboardPanel.GetComponent<ScoreboardLogic>();
        
        _playerFinderModalFade = playerFinder.GetComponent<ModalFade>();
        
        Timer.onGameEnd += EndGame;
        
        
        _allPlayerData = new NetworkList<PlayerData>();
        _allPlayerData.OnListChanged += OnPlayerListChanged;
        
    }

    private void Update()
    {
        if (_gameEnd)
        {
            StopAllCoroutines();
        }
    }

#endregion

#region Ready

    private void SwapReadyButtons(int index) {
        switch (index)
                {
                    case 0:
                        _readyButtonImageIndex = 1;
                        SwapReadyButtonImage();
                        break;
                    case 2:
                        if (IsServer)
                        {
                            ServerStartMatch();
                        }
                        return;
                    case 3:
                        _readyButtonImageIndex = 1;
                        SwapReadyButtonImage();
                        break;
                }
    }

    public void SetReady()
    {
        SwapReadyButtons(_readyButtonImageIndex);

        if (_isReadyLocal) return;
            
        _isReadyLocal = true;
        SendReady(_isReadyLocal);
    }

    private void SwapReadyButtonImage()
    {
        readyButton.image.sprite = readyButtonImages[_readyButtonImageIndex];
    }

    private void SendReady(bool value)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        if (IsServer)
        {
            SendReadyInner(clientId, value);
        }
        else
        {
            SendReadyToServerRpc(clientId, value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendReadyToServerRpc(ulong clientId, bool value)
    {
        SendReadyInner(clientId, value);
    }

    private void SendReadyInner(ulong clientId, bool value)
    {
        for (int i = 0; i < _allPlayerData.Count; i++)
        {
            if (_allPlayerData[i].clientId == clientId)
            {
                _allPlayerData[i] = new PlayerData()
                {
                    clientId = _allPlayerData[i].clientId,
                    name = _allPlayerData[i].name,
                    score = _allPlayerData[i].score,
                    health = _allPlayerData[i].health,
                    deaths = _allPlayerData[i].deaths,
                    isDead = _allPlayerData[i].isDead,
                    color = _allPlayerData[i].color,
                    isReady = value
                };
                UpdateScoreboardReadyClientRpc(_allPlayerData[i].clientId, _allPlayerData[i].isReady);
            }
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerData> e)
    {

        if (IsServer)
        {

            if (_gameStarted)
            {
                foreach (PlayerData playerData in _allPlayerData)
                {
                    UpdateScoreboardClientRpc(playerData);
                }
            }
            else
            {
                int count = 0;
                foreach (PlayerData playerData in _allPlayerData)
                {
                    if (playerData.isReady)
                    {
                        count++;
                    }
                }

                if (_isReadyLocal)
                {
                    if (count == _allPlayerData.Count)
                    {
                        _readyButtonImageIndex = 2;
                    }
                    else
                    {
                        _readyButtonImageIndex = 1;
                    }
                    SwapReadyButtonImage();
                }
            }
        }
    }

    private void ServerStartMatch()
    {
        if (_isFirstGo)
        {
            _isFirstGo = false;
        }
        else
        {
            PositionPlayers();
        }

        _gameStarted = true;

        for (int i = 0; i < _allPlayerData.Count; i++)
        {

            _allPlayerData[i] = new PlayerData()
            {
                clientId = _allPlayerData[i].clientId,
                name = _allPlayerData[i].name,
                score = startingScore,
                health = playerMaxHealth,
                deaths = 0,
                isDead = false,
                color = _allPlayerData[i].color,
                isReady = false
            };
            UpdateScoreboardScoreClientRpc(i.ConvertTo<UInt64>(), _allPlayerData[i].score);
            UpdateScoreboardDeathsClientRpc(i.ConvertTo<UInt64>(), _allPlayerData[i].deaths);
            UpdateLocalScoreClientRpc(i.ConvertTo<UInt64>(), _allPlayerData[i].score);
            UpdateLocalDeathsClientRpc(i.ConvertTo<UInt64>(), _allPlayerData[i].deaths);
        }
        StartGameClientRpc();
    }


    [ClientRpc]
    private void StartGameClientRpc()
    {
        _gameStarted = true;
        _movementAndSteering.ResetWheel();
        _wheelSwitcher.SwitchWheel(0);
        roomCodeText.GetComponent<ModalFade>().Hide();
        _isReadyLocal = false;
        SetUpScene();
        localPlayer.GetComponent<LocalPlayer>().ShowCanvas();
        _shipEffectController.IsRespawn();
        ToggleNetworkPlayerVisibilty(true);
        canvasDarkBG.GetComponent<ModalFade>().Hide();
        _scoreboardPanelModalFade.Hide();
        StartCoroutine(WaitToExecute(0.27f, new Action[] { _scoreboardLogic.ModeScoreboard, SetScoreBoardModeScore }));
        _playerFinderModalFade.Show();
        Timer.Instance.StartTimer(gameTime);
        _movementAndSteering._pauseUpdate = false;
        _gameEnd = false;
        if (_isFirstGo) _isFirstGo = false;
        EnablePlayerControls(localPlayer);
        EnsureNetworkColliders();
        _movementAndSteering.ForceAllowGestureDetection();
    }



    [ClientRpc]
    private void SetSailColorClientRpc()
    {
        SetSailColor();
    }

    private void SetSailColor()
    {
        foreach (PlayerAvatar player in networkedGameObjects)
        {
            player.SetColor(colorList[player.gameObject.GetComponent<NetworkObject>().OwnerClientId]);
        }
    }

    public void SetLocalColor()
    {
        localPlayer.GetComponent<LocalPlayer>().SetColor(colorList[NetworkManager.Singleton.LocalClientId]);
        RibbonColorSet(colorList[NetworkManager.Singleton.LocalClientId]);

    }

    [ClientRpc]
    private void GetNetworkedObjectsClientRpc()
    {
        networkedGameObjects = FindObjectsOfType<PlayerAvatar>(true);
        foreach (var networkedGameObject in networkedGameObjects) {
            if (networkedGameObject.IsLocalPlayer) {
                networkedGameObject.gameObject.layer = LayerMask.NameToLayer("NoInteractions");
            }
        }
    }

    IEnumerator WaitToExecute(float time, Action[] function)
    {
        yield return new WaitForSeconds(time);
        foreach (Action action in function)
        {
            action();
        }
    }

#endregion

    [ClientRpc]
    private void SetUpSceneClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        SetUpScene();
    }

    private void SetUpScene()
    {
        foreach (GameObject g in uiPlayStateObjects)
        {
            g.GetComponent<ModalFade>().Show();
        }

        if (!_isFirstGo)
        {
            _shipEffectController.IsRespawn();
        }
        
        _wheelSwitcher.NewWheel();


        ToggleNetworkPlayerVisibilty(true);
    }

    private void DestroyScene()
    {
        _wheelSwitcher.WheelDeath();
        foreach (GameObject g in uiPlayStateObjects)
        {
            g.GetComponent<ModalFade>().Hide();
        }
        _shipEffectController.SetInvis();
        localPlayer.GetComponent<LocalPlayer>().HideCanvas();
        ToggleNetworkPlayerVisibilty(false);
    }

    private void EnsureNetworkColliders()
    {
        foreach (PlayerAvatar avatar in networkedGameObjects)
        {
            avatar.GetComponent<Collider>().enabled = true;
        }
    }

    private void ToggleNetworkPlayerVisibilty(bool active)
    {
        if (networkedGameObjects.Length == 0) return;

        switch (active)
        {
            case true:
                foreach (PlayerAvatar playerAvatar in networkedGameObjects)
                {
                    if (playerAvatar.gameObject.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
                    {
                        playerAvatar.gameObject.GetComponentInChildren<ShipEffectController>(true).IsRespawn();
                        playerAvatar.gameObject.GetComponent<Collider>().enabled = true;
                    }
                }
                break;
            case false:
                foreach (PlayerAvatar player in networkedGameObjects)
                {
                    if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
                    {
                        player.gameObject.GetComponentInChildren<ShipEffectController>(true).SetInvis();
                        player.gameObject.GetComponent<Collider>().enabled = false;
                    }
                }
                break;
        }
    }

    private void EndGame()
    {
        if (!IsServer) return;
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        _movementAndSteering._pauseUpdate = true;
        _gameEnd = true;
        DestroyScene();
        _gameStarted = false;
        _readyButtonImageIndex = 3;
        SwapReadyButtonImage();
        _playerFinderModalFade.Hide();
        _scoreboardLogic.ModeGameEnd();
        _scoreboardPanelModalFade.Show();
        canvasDarkBG.GetComponent<ModalFade>().Show();
    }

    private void ShowLobby(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        _scoreboardLogic.Initialize();
        _scoreboardPanelModalFade.Show();
    }

    [ClientRpc]
    private void ShowLobbyClientRpc(ulong clientId)
    {
        ShowLobby(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddNewClientToListServerRpc(ulong clientId, FixedString64Bytes clientName)
    {
        AddNewClientToList(clientId, clientName);
    }

    public void AddNewClientToList(ulong clientId, FixedString64Bytes clientName)
    {
        if (!IsServer) return;

        GetNetworkedObjectsClientRpc();
        foreach (var playerData in _allPlayerData)
        {
            if (playerData.clientId == clientId) return;
        }

        PlayerData newPlayerData = new()
        {
            clientId = clientId,
            name = clientName,
            score = startingScore,
            health = playerMaxHealth,
            deaths = 0,
            isDead = false,
            color = colorList[_allPlayerData.Count],
            isReady = false,
        };


        _allPlayerData.Add(newPlayerData);

        foreach (var playerData in _allPlayerData)
        {
            NewSetScoreboardClientRpc(playerData);
        }

        SetSailColorClientRpc();
        PositionPlayerStartClientRpc(clientId);
        ShowLobbyClientRpc(clientId);

    }

    private void PositionPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PositionPlayerStartClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void PositionPlayerStartClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        localPlayer.transform.position = spawnPoints[clientId].transform.position;
        Vector3 directionAwayFromCenter = (localPlayer.transform.position - spawnAroundObject.transform.position).normalized;
        directionAwayFromCenter.y = 0f;
        localPlayer.transform.rotation = Quaternion.LookRotation(directionAwayFromCenter);
        SetSailColor();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReduceHealthServerRpc(ulong shooterId, ulong target)
    {
        if (!IsServer || _gameEnd) return;

        //Reduce the health of target
        if (shootingDebug)
        {
            for (int i = 0; i < _allPlayerData.Count; i++)
            {
                ShooterDebugClientRpc(shooterId);
                TargetDebugClientRpc(target);
            }
            return;
        }

        for (int i = 0; i < _allPlayerData.Count; i++)
        {
            if (_allPlayerData[i].clientId == target)
            {
                if (_allPlayerData[i].isDead) return;

                int newHealth = _allPlayerData[i].health - playerDamage;

                WheelDamageClientRpc(target, newHealth);

                if (newHealth <= 0)
                {
                    int newDeaths = _allPlayerData[i].deaths + 1;
                    newHealth = 0;

                    _allPlayerData[i] = new PlayerData
                    {
                        clientId = _allPlayerData[i].clientId,
                        name = _allPlayerData[i].name,
                        score = _allPlayerData[i].score,
                        health = newHealth,
                        deaths = newDeaths,
                        isDead = true,
                        color = _allPlayerData[i].color,
                        isReady = _allPlayerData[i].isReady,
                    };

                    UpdateLocalDeathsClientRpc(target, _allPlayerData[i].deaths);

                    OnBoatDeathClientRpc(target);

                    for (int j = 0; j < _allPlayerData.Count; j++)
                    {
                        if ((_allPlayerData[j].clientId == shooterId))
                        {
                            int newScore = _allPlayerData[j].score + 1;

                            UpdateLocalScoreClientRpc(shooterId, newScore);

                            _allPlayerData[j] = new PlayerData
                            {
                                clientId = _allPlayerData[j].clientId,
                                name = _allPlayerData[j].name,
                                score = newScore,
                                health = _allPlayerData[j].health,
                                deaths = _allPlayerData[j].deaths,
                                isDead = _allPlayerData[j].isDead,
                                color = _allPlayerData[j].color,
                                isReady = _allPlayerData[j].isReady,
                            };
                        }
                    }
                    break;
                }

                if (!_allPlayerData[i].isDead)
                {
                    UpdateLocalTextClientRpc(shooterId, target);

                    _allPlayerData[i] = new PlayerData
                    {
                        clientId = _allPlayerData[i].clientId,
                        name = _allPlayerData[i].name,
                        score = _allPlayerData[i].score,
                        health = newHealth,
                        deaths = _allPlayerData[i].deaths,
                        isDead = _allPlayerData[i].isDead,
                        color = _allPlayerData[i].color,
                        isReady = _allPlayerData[i].isReady,
                    };
                }
                break;
            }
        }
    }

    [ClientRpc]
    private void WheelDamageClientRpc(ulong clientId, int health)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        if (health > 6)
        {
            _wheelSwitcher.SwitchWheel(0);
        }
        else if (health > 3)
        {
            _wheelSwitcher.SwitchWheel(1);
        }
        else
        {
            _wheelSwitcher.SwitchWheel(2);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnShootServerRpc(ulong shooterId, ulong objectId)
    {

        OnShootClientRpc(shooterId, objectId);
        if (NetworkManager.Singleton.LocalClientId == shooterId) return;
        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == shooterId)
            {
                if (objectId == 0)
                {
                    player.gameObject.GetComponentInChildren<NetworkCannonsStarboard>().ShootCannons();
                    player.gameObject.GetComponentInChildren<ShipEffectController>().FireRight();
                }
                else if (objectId == 1)
                {
                    player.gameObject.GetComponentInChildren<NetworkCannonsPort>().ShootCannons();
                    player.gameObject.GetComponentInChildren<ShipEffectController>().FireLeft();
                }
                break;
            }
        }
    }

    [ClientRpc]
    public void OnShootClientRpc(ulong shooterId, ulong objectId)
    {
        if (IsServer || shooterId == NetworkManager.Singleton.LocalClientId) return;
        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == shooterId)
            {
                if (objectId == 0)
                {
                    player.gameObject.GetComponentInChildren<NetworkCannonsStarboard>().ShootCannons();
                    player.gameObject.GetComponentInChildren<ShipEffectController>().FireRight();
                }
                else
                {
                    player.gameObject.GetComponentInChildren<NetworkCannonsPort>().ShootCannons();
                    player.gameObject.GetComponentInChildren<ShipEffectController>().FireLeft();
                }
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnSailServerRpc(ulong clientId, float value)
    {
        OnSailClientRpc(clientId, value);
        if (NetworkManager.Singleton.LocalClientId == clientId) return;
        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.GetComponentInChildren<ShipEffectController>().SetFurl(value);
            }
        }

    }

    [ClientRpc]
    private void OnSailClientRpc(ulong clientId, float value)
    {
        if (IsServer || clientId == NetworkManager.Singleton.LocalClientId) return;
        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.GetComponentInChildren<ShipEffectController>().SetFurl(value);
            }
        }

    }


    #region Death and Respawn

    [ClientRpc]
    public void OnBoatDeathClientRpc(ulong clientId)
    {

        AudioManager.Instance.PlaySound("woodenShipBreak");
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            _movementAndSteering._pauseUpdate = true;
            foreach (var button in shootingButtons)
            {
                button.interactable = false;
            }
            _shipEffectController.IsSunk();
            SinkIntoRespawnFunction(localPlayer, clientId);

        }

        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.gameObject.GetComponentInChildren<ShipEffectController>().IsSunk();
                player.gameObject.GetComponent<Collider>().enabled = false;
                SinkIntoRespawnFunction(player);
            }
        }
    }

    private void SinkIntoRespawnFunction(GameObject player, ulong clientId)
    {
        StartCoroutine(SinkIntoRespawn(player, clientId));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNotDeadServerRpc(ulong clientId)
    {
        for (int i = 0; i < _allPlayerData.Count; i++)
        {
            if (_allPlayerData[i].clientId == clientId)
            {
                _allPlayerData[i] = new PlayerData
                {
                    clientId = _allPlayerData[i].clientId,
                    name = _allPlayerData[i].name,
                    score = _allPlayerData[i].score,
                    health = playerMaxHealth,
                    deaths = _allPlayerData[i].deaths,
                    isDead = false,
                    color = _allPlayerData[i].color,
                    isReady = _allPlayerData[i].isReady,
                };
                break;
            }
        }
    }

    private void EnablePlayerControls(GameObject player)
    {
        _movementAndSteering._pauseUpdate = false;
        _movementAndSteering.CanTurnWheel = true;
        foreach (Button button in shootingButtons)
        {
            button.interactable = true;
        }
    }

    private IEnumerator SinkIntoRespawn(GameObject player, ulong clientId)
    {

        yield return new WaitForSeconds(6f);
        _wheelSwitcher.NewWheel();
        _shipEffectController.IsRespawn();
        _movementAndSteering._pauseUpdate = false;
        _movementAndSteering.ResetWheel();
        foreach (Button button in shootingButtons)
        {
            button.interactable = true;
        }
        SetNotDeadServerRpc(clientId);
    }

    #region Death and Respawn for NetworkObjects
    private void SinkIntoRespawnFunction(PlayerAvatar player)
    {
        StartCoroutine(SinkIntoRespawn(player));
    }

    private IEnumerator SinkIntoRespawn(PlayerAvatar player)
    {
        yield return new WaitForSeconds(6);
        player.gameObject.GetComponentInChildren<ShipEffectController>().IsRespawn();
        player.gameObject.GetComponent<Collider>().enabled = true;
    }

    #endregion
    #endregion


    #region Scoreboard

    [ClientRpc]
    public void UpdateScoreboardScoreClientRpc(ulong clientId, int newScore)
    {
        _playerCards[clientId].SetScore(newScore);
    }

    [ClientRpc]
    public void UpdateScoreboardDeathsClientRpc(ulong clientId, int newDeaths)
    {
        _playerCards[clientId].SetDeaths(newDeaths);
    }

    [ClientRpc]
    public void UpdateScoreBoardColorClientRpc(ulong clientId, FixedString64Bytes newCol)
    {
        _playerCards[clientId].SetColor(newCol);
    }

    [ClientRpc]
    public void UpdateScoreboardReadyClientRpc(ulong clientId, bool newReady)
    {
        _playerCards[clientId].SetReady(newReady);
    }

    [ClientRpc]
    public void UpdateScoreboardClientRpc(PlayerData playerdata)
    {
        _playerCards[playerdata.clientId].SetPlayerData(playerdata);
    }

    [ClientRpc]
    public void SetScoreboardClientRpc(ulong clientId, FixedString64Bytes name, int score, int deaths, FixedString64Bytes colorArg)
    {
        if (_playerCards.ContainsKey(clientId)) return;

        CreatePlayerCard(clientId, name, score, deaths, colorArg);
    }

    [ClientRpc]
    private void NewSetScoreboardClientRpc(PlayerData playerData)
    {
        if (_playerCards.ContainsKey(playerData.clientId)) return;

        NewCreatePlayerCard(playerData);
    }

    private void SetScoreBoardModeScore()
    {
        foreach (KeyValuePair<ulong, PlayerCard> card in _playerCards)
        {
            card.Value.ModeScoreboard();
        }
    }

    private void SetScoreboardModeLobby()
    {
        foreach (KeyValuePair<ulong, PlayerCard> card in _playerCards)
        {
            card.Value.ModeLobby();
        }
    }

    public void CreatePlayerCard(ulong clientId, FixedString64Bytes name, int kills, int deaths, FixedString64Bytes colorArg)
    {
        PlayerCard newCard = Instantiate(playerCardPrefab, playerCardParent);
        newCard.SetScore(kills);
        newCard.SetDeaths(deaths);
        newCard.SetColor(colorArg);
        _playerCards.Add(clientId, newCard);
        newCard.Initialize(name);
    }

    private void NewCreatePlayerCard(PlayerData playerData)
    {
        PlayerCard newCard = Instantiate(playerCardPrefab, playerCardParent);
        _playerCards.Add(playerData.clientId, newCard);
        newCard.SetPlayerData(playerData);
    }


    #endregion


    #region Debugging texts
    [ClientRpc]
    private void GenericTestClientRpc(string somethingToPrint)
    {
        //combatLog.text = somethingToPrint;
    }


    [ClientRpc]
    private void TargetDebugClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        combatLog.text = "you have been hit";
    }

    [ClientRpc]
    private void ShooterDebugClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        combatLog.text = "you have shot someone";
    }


    [ClientRpc]
    void UpdateLocalTextClientRpc(ulong shooterId, ulong targetId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetId) return;
        combatLog.text = $"You have been shot by {shooterId}";
    }

    [ClientRpc]
    void UpdateLocalScoreClientRpc(ulong clientId, int score)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        localScore.text = score.ToString();
    }

    [ClientRpc]
    void UpdateLocalDeathsClientRpc(ulong clientId, int health)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        localDeathCount.text = health.ToString();
    }

    [ClientRpc]
    void ConveyDeathClientRpc(ulong shooterId, ulong targetId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetId)
        {
            combatLog.text = $"{shooterId} has sunk your boat!";
        }
    }

    #endregion


    private void RibbonColorSet(FixedString64Bytes colorArg)
    {
        Color newCol;
        if (UnityEngine.ColorUtility.TryParseHtmlString(colorArg.ToString(), out newCol))
        {
            ribbon.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Tint", newCol);
            //coloredRing.color = newCol;
        }
    }
}
