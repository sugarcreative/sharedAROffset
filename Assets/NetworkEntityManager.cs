using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class NetworkEntityManager : NetworkBehaviour
{

    #region Class Fields

    public static NetworkEntityManager Instance;

    private NetworkList<PlayerData> allPlayerData;

    [SerializeField] private GameObject scoreboardPanel;

    [SerializeField] private ScoreboardLogic scoreboardLogic;

    private const int MAXHEALTH = 1;

    private const int STARTSCORE = 0;

    private const int DAMAGE = 1;

    [SerializeField] private GameObject localPlayer;

    [SerializeField] private TMP_Text combatLog;

    [SerializeField] private TMP_Text localHealth;

    [SerializeField] private TMP_Text localScore;

    [SerializeField] private PlayerCard playerCardPrefab;

    [SerializeField] Transform playerCardParent;

    private Dictionary<ulong, PlayerCard> playerCards = new Dictionary<ulong, PlayerCard>();

    public bool shootingDebug = false;

    public FixedString64Bytes[] colorList = new FixedString64Bytes[] { "#AD3232", "#A59622", "#285D27", "#28A5A7", "#29447D", "#A5579E", "#00FF6E" };

    private bool isReadyLocal = false;

    private bool gameStarted;

    [SerializeField] private Button readyButton;

    private bool gettingReady;

    private ulong latestClientId;

    [SerializeField] private GameObject[] playStateObjects;

    [SerializeField] private Transform spawnAroundObject;

    private bool isFirstGo = true;

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private PlayerAvatar[] networkedGameObjects;

    [SerializeField] private GameObject CanvasDarkBG;

    [SerializeField] private Sprite[] readyButtonImages;

    [SerializeField] private GameObject TopBar;

    private int readyButtonImageIndex = 0;

    [SerializeField] private Button[] shootingButtons;

    private bool gameEnd = false;

    [SerializeField] private GameObject wheel;

    [SerializeField] private GameObject[] uiPlayStateObjects;

    [SerializeField] private GameObject roomCodeText;

    [SerializeField] private GameObject playerFinder;

    [SerializeField] private GameObject[] ribbons;

    private const float GAMETIME = 30f;

    #endregion

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
        Timer.onGameEnd += EndGame;
        gameStarted = false;
        allPlayerData = new NetworkList<PlayerData>();
        allPlayerData.OnListChanged += OnPlayerListChanged;
        localPlayer.GetComponent<MovementAndSteering>()._pauseUpdate = true;
    }

    #endregion

    #region Ready

    public void SetReady()
    {
        switch (readyButtonImageIndex)
        {
            case 0:
                readyButtonImageIndex = 1;
                SwapReadyButtonImage();
                break;
            case 2:
                if (IsServer)
                {
                    //gameStarted = true;
                    ServerStartMatch();
                }
                return;
            case 3:
                readyButtonImageIndex = 1;
                SwapReadyButtonImage();
                break;
        }

        if (!isReadyLocal)
        {
            isReadyLocal = true;
            SendReady(isReadyLocal);
        }
    }

    private void SwapReadyButtonImage()
    {
        readyButton.image.sprite = readyButtonImages[readyButtonImageIndex];
    }

    public void SendReady(bool value)
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
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientId == clientId)
            {
                allPlayerData[i] = new PlayerData()
                {
                    clientId = allPlayerData[i].clientId,
                    name = allPlayerData[i].name,
                    score = allPlayerData[i].score,
                    health = allPlayerData[i].health,
                    deaths = allPlayerData[i].deaths,
                    isDead = allPlayerData[i].isDead,
                    color = allPlayerData[i].color,
                    isReady = value
                };
                UpdateScoreboardReadyClientRpc(allPlayerData[i].clientId, allPlayerData[i].isReady);
            }
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerData> e)
    {

        if (IsServer)
        {

            if (gameStarted)
            {
                foreach (PlayerData playerData in allPlayerData)
                {
                    UpdateScoreboardClientRpc(playerData);
                }
            }
            else
            {
                int count = 0;
                foreach (PlayerData playerData in allPlayerData)
                {
                    if (playerData.isReady)
                    {
                        count++;
                    }
                }

                if (isReadyLocal)
                {
                    if (count == allPlayerData.Count)
                    {
                        readyButtonImageIndex = 2;
                    }
                    else
                    {
                        readyButtonImageIndex = 1;
                    }
                    SwapReadyButtonImage();
                }
            }
        }
    }

    private void ServerStartMatch()
    {
        if (isFirstGo)
        {
            isFirstGo = false;
        }
        else
        {
            PositionPlayers();
        }

        gameStarted = true;

        for (int i = 0; i < allPlayerData.Count; i++)
        {

            allPlayerData[i] = new PlayerData()
            {
                clientId = allPlayerData[i].clientId,
                name = allPlayerData[i].name,
                score = STARTSCORE,
                health = MAXHEALTH,
                deaths = 0,
                isDead = false,
                color = allPlayerData[i].color,
                isReady = false
            };
            UpdateScoreboardScoreClientRpc(i.ConvertTo<UInt64>(), allPlayerData[i].score);
            UpdateScoreboardDeathsClientRpc(i.ConvertTo<UInt64>(), allPlayerData[i].deaths);
            UpdateLocalScoreClientRpc(i.ConvertTo<UInt64>(), allPlayerData[i].score);
            UpdateLocalDeathsClientRpc(i.ConvertTo<UInt64>(), allPlayerData[i].deaths);
        }
        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        gameStarted = true;
        wheel.GetComponent<WheelSwitcher>().SwitchWheel(0);
        roomCodeText.GetComponent<ModalFade>().Hide();
        isReadyLocal = false;
        SetUpScene();
        CanvasDarkBG.GetComponent<ModalFade>().Hide();
        scoreboardPanel.GetComponent<ModalFade>().Hide();
        StartCoroutine(WaitToExecute(0.27f, new Action[] { scoreboardLogic.ModeScoreboard, SetScoreBoardModeScore }));
        playerFinder.GetComponent<ModalFade>().Show();
        Timer.Instance.StartTimer(GAMETIME);
        localPlayer.GetComponent<MovementAndSteering>()._pauseUpdate = false;
        gameEnd = false;
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
        //roomCodeText.GetComponent<ModalFade>().Hide();
        foreach (GameObject g in playStateObjects)
        {
            g.SetActive(true);
        }

        foreach (GameObject g in uiPlayStateObjects)
        {
            g.GetComponent<ModalFade>().Show();
        }
        localPlayer.GetComponent<LocalPlayer>().ShowCanvas();
        //TopBar.GetComponent<ModalFade>().Show();
        ToggleNetworkPlayerVisibilty(true);
    }

    private void DestroyScene()
    {
        localPlayer.SetActive(false);
        foreach (GameObject g in playStateObjects)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in uiPlayStateObjects)
        {
            g.GetComponent<ModalFade>().Hide();
        }
        localPlayer.GetComponent<LocalPlayer>().HideCanvas();
        //TopBar.GetComponent<ModalFade>().Hide();
        ToggleNetworkPlayerVisibilty(false);
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
                        playerAvatar.gameObject.SetActive(true);
                        playerAvatar.gameObject.GetComponentInChildren<ShipEffectController>(true).IsRespawn();
                    }
                }
                break;
            case false:
                foreach (PlayerAvatar player in networkedGameObjects)
                {
                    if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
                    {
                        player.gameObject.SetActive(false);
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
        localPlayer.GetComponent<MovementAndSteering>()._pauseUpdate = true;
        gameEnd = true;
        DestroyScene();
        gameStarted = false;
        readyButtonImageIndex = 3;
        SwapReadyButtonImage();
        playerFinder.GetComponent<ModalFade>().Hide();
        scoreboardLogic.ModeGameEnd();
        scoreboardPanel.GetComponent<ModalFade>().Show();
        CanvasDarkBG.GetComponent<ModalFade>().Show();
    }

    private void ShowLobby(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        scoreboardLogic.Initialize();
        scoreboardPanel.GetComponent<ModalFade>().Show();
    }

    [ClientRpc]
    private void ShowLobbyClientRpc(ulong clientId)
    {
        ShowLobby(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddNewClientToListServerRpc(ulong clientId, FixedString64Bytes name)
    {
        AddNewClientToList(clientId, name);
    }

    public void AddNewClientToList(ulong clientId, FixedString64Bytes name)
    {
        if (!IsServer) return;

        GetNetworkedObjectsClientRpc();
        foreach (var playerData in allPlayerData)
        {
            if (playerData.clientId == clientId) return;
        }

        PlayerData newPlayerData = new()
        {
            clientId = clientId,
            name = name,
            score = STARTSCORE,
            health = MAXHEALTH,
            deaths = 0,
            isDead = false,
            color = colorList[allPlayerData.Count],
            isReady = false,
        };


        //UpdateLocalHealthClientRpc(clientId, MAXHEALTH);
        allPlayerData.Add(newPlayerData);

        foreach (var playerData in allPlayerData)
        {
            //SetScoreboardClientRpc(playerData.clientId, playerData.name, playerData.score, playerData.deaths, playerData.color);
            NewSetScoreboardClientRpc(playerData);
        }

        SetSailColorClientRpc();
        PositionPlayerStartClientRpc(clientId);
        ShowLobbyClientRpc(clientId);
        SetUpSceneClientRpc(clientId);

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
        if (!IsServer) return;

        //Reduce the health of target
        if (shootingDebug)
        {
            for (int i = 0; i < allPlayerData.Count; i++)
            {
                ShooterDebugClientRpc(shooterId);
                TargetDebugClientRpc(target);
            }
            return;
        }

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientId == target)
            {
                if (allPlayerData[i].isDead) return;

                int newHealth = allPlayerData[i].health - DAMAGE;

                WheelDamageClientRpc(target, newHealth);

                if (newHealth <= 0)
                {
                    int newDeaths = allPlayerData[i].deaths + 1;
                    newHealth = 0;

                    allPlayerData[i] = new PlayerData
                    {
                        clientId = allPlayerData[i].clientId,
                        name = allPlayerData[i].name,
                        score = allPlayerData[i].score,
                        health = newHealth,
                        deaths = newDeaths,
                        isDead = true,
                        color = allPlayerData[i].color,
                        isReady = allPlayerData[i].isReady,
                    };

                    UpdateLocalDeathsClientRpc(target, allPlayerData[i].deaths);

                    OnBoatDeathClientRpc(target);

                    for (int j = 0; j < allPlayerData.Count; j++)
                    {
                        if ((allPlayerData[j].clientId == shooterId))
                        {
                            int newScore = allPlayerData[j].score + 1;

                            UpdateLocalScoreClientRpc(shooterId, newScore);

                            allPlayerData[j] = new PlayerData
                            {
                                clientId = allPlayerData[j].clientId,
                                name = allPlayerData[j].name,
                                score = newScore,
                                health = allPlayerData[j].health,
                                deaths = allPlayerData[j].deaths,
                                isDead = allPlayerData[j].isDead,
                                color = allPlayerData[j].color,
                                isReady = allPlayerData[j].isReady,
                            };
                        }
                    }
                    break;
                }

                if (!allPlayerData[i].isDead)
                {
                    UpdateLocalTextClientRpc(shooterId, target);

                    allPlayerData[i] = new PlayerData
                    {
                        clientId = allPlayerData[i].clientId,
                        name = allPlayerData[i].name,
                        score = allPlayerData[i].score,
                        health = newHealth,
                        deaths = allPlayerData[i].deaths,
                        isDead = allPlayerData[i].isDead,
                        color = allPlayerData[i].color,
                        isReady = allPlayerData[i].isReady,
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
            wheel.GetComponent<WheelSwitcher>().SwitchWheel(0);
        }
        else if (health > 3)
        {
            wheel.GetComponent<WheelSwitcher>().SwitchWheel(1);
        }
        else
        {
            wheel.GetComponent<WheelSwitcher>().SwitchWheel(2);
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
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            localPlayer.GetComponent<MovementAndSteering>()._pauseUpdate = true;
            foreach (Button button in shootingButtons)
            {
                button.interactable = false;
            }
            localPlayer.GetComponentInChildren<ShipEffectController>(true).IsSunk();
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
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientId == clientId)
            {
                allPlayerData[i] = new PlayerData
                {
                    clientId = allPlayerData[i].clientId,
                    name = allPlayerData[i].name,
                    score = allPlayerData[i].score,
                    health = MAXHEALTH,
                    deaths = allPlayerData[i].deaths,
                    isDead = false,
                    color = allPlayerData[i].color,
                    isReady = allPlayerData[i].isReady,
                };
                break;
            }
        }
    }

    public void RespawnTest()
    {
        localPlayer.GetComponentInChildren<ShipEffectController>().IsRespawn();
    }

    private IEnumerator SinkIntoRespawn(GameObject player, ulong clientId)
    {
        yield return new WaitForSeconds(6f);
        wheel.GetComponent<WheelSwitcher>().SwitchWheel(0);
        player.GetComponentInChildren<ShipEffectController>().IsRespawn();
        player.GetComponent<MovementAndSteering>()._pauseUpdate = false;
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
        playerCards[clientId].SetScore(newScore);
    }

    [ClientRpc]
    public void UpdateScoreboardDeathsClientRpc(ulong clientId, int newDeaths)
    {
        playerCards[clientId].SetDeaths(newDeaths);
    }

    [ClientRpc]
    public void UpdateScoreBoardColorClientRpc(ulong clientId, FixedString64Bytes newCol)
    {
        playerCards[clientId].SetColor(newCol);
    }

    [ClientRpc]
    public void UpdateScoreboardReadyClientRpc(ulong clientId, bool newReady)
    {
        playerCards[clientId].SetReady(newReady);
    }

    [ClientRpc]
    public void UpdateScoreboardClientRpc(PlayerData playerdata)
    {
        playerCards[playerdata.clientId].SetPlayerData(playerdata);
    }

    [ClientRpc]
    public void SetScoreboardClientRpc(ulong clientId, FixedString64Bytes name, int score, int deaths, FixedString64Bytes colorArg)
    {
        if (playerCards.ContainsKey(clientId)) return;

        CreatePlayerCard(clientId, name, score, deaths, colorArg);
    }

    [ClientRpc]
    private void NewSetScoreboardClientRpc(PlayerData playerData)
    {
        if (playerCards.ContainsKey(playerData.clientId)) return;

        NewCreatePlayerCard(playerData);
    }

    private void SetScoreBoardModeScore()
    {
        foreach (KeyValuePair<ulong, PlayerCard> card in playerCards)
        {
            card.Value.ModeScoreboard();
        }
    }

    private void SetScoreboardModeLobby()
    {
        foreach (KeyValuePair<ulong, PlayerCard> card in playerCards)
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
        playerCards.Add(clientId, newCard);
        newCard.Initialize(name);
    }

    private void NewCreatePlayerCard(PlayerData playerData)
    {
        PlayerCard newCard = Instantiate(playerCardPrefab, playerCardParent);
        playerCards.Add(playerData.clientId, newCard);
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
        //combatLog.text = "you have been hit";
    }

    [ClientRpc]
    private void ShooterDebugClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        //combatLog.text = "you have shot someone";
    }


    [ClientRpc]
    void UpdateLocalTextClientRpc(ulong shooterId, ulong targetId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetId) return;
        //combatLog.text = $"You have been shot by {shooterId}";
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
        localHealth.text = health.ToString();
    }

    [ClientRpc]
    void ConveyDeathClientRpc(ulong shooterId, ulong targetId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetId)
        {
            //combatLog.text = $"{shooterId} has sunk your boat!";
        }
    }

    #endregion


    private void RibbonColorSet(FixedString64Bytes colorArg)
    {
        foreach(GameObject ribbon in ribbons)
        {
            Color newCol;
            if (UnityEngine.ColorUtility.TryParseHtmlString(colorArg.ToString(), out newCol))
            {
                ribbon.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Tint", newCol);
            }
        }
    }
}
