using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetworkEntityManager : NetworkBehaviour
{
    public static NetworkEntityManager Instance;

    private NetworkList<PlayerData> allPlayerData;

    [SerializeField] private GameObject scoreboardPanel;

    [SerializeField] private ScoreboardLogic scoreboardLogic;

    private const int MAXHEALTH = 10;

    private const int STARTSCORE = 0;

    private const int DAMAGE = 1;

    [SerializeField] private GameObject localPlayer;

    [SerializeField] private TMP_Text combatLog;

    [SerializeField] private TMP_Text localHealth;

    [SerializeField] private TMP_Text localScore;

    [SerializeField] private PlayerCard playerCardPrefab;

    [SerializeField] Transform playerCardParent;

    private List<ulong> clientIds = new List<ulong>();

    private Dictionary<ulong, PlayerCard> playerCards = new Dictionary<ulong, PlayerCard>();

    public bool shootingDebug = false;

    public FixedString64Bytes[] colorList = new FixedString64Bytes[] { "#AD3232", "#A59622", "#285D27", "#28A5A7", "#29447D", "#A5579E", "#00FF6E" };

    private bool isReadyLocal;

    private bool gameStarted;

    [SerializeField] private GameObject arScenery;

    [SerializeField] private Button readyButton;

    private bool gettingReady;

    private ulong latestClientId;

    [SerializeField] private GameObject[] playStateObjects;

    [SerializeField] private Transform spawnAroundObject;

    private float radius = 0.6f;

    private bool isFirstGo = true;

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private PlayerAvatar[] networkedGameObjects;

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
    }

    #region Ready

    public void SetReady()
    {
        gameStarted = true;

        if (IsServer)
        {
            if (isFirstGo)
            {
                isFirstGo = false;
            }
            else
            {
                PositionPlayers();
            }

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
                UpdateLocalHealthClientRpc(i.ConvertTo<UInt64>(), allPlayerData[i].deaths);
            }
            StartGameClientRpc();
            readyButton.interactable = false;
        }
        else
        {
            isReadyLocal = !isReadyLocal;
            SendReady(isReadyLocal);
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
                    TestClientRpc(playerData);
                }
            }
            else
            {
                GenericTestClientRpc($"{latestClientId} is {gettingReady}");

                int numOfPlayers = allPlayerData.Count - 1;
                int count = 0;
                foreach (PlayerData playerData in allPlayerData)
                {
                    if (playerData.isReady)
                    {
                        count++;
                    }
                }

                if (count == numOfPlayers)
                {
                    readyButton.interactable = true;
                }
                else
                {
                    readyButton.interactable = false;
                }
            }
        }
    }


    [ClientRpc]
    private void TestClientRpc(PlayerData playerdata)
    {
        playerCards[playerdata.clientId].SetPlayerData(playerdata);
    }

    public void SendReady(bool value)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        SendReadyToServerRpc(clientId, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendReadyToServerRpc(ulong clientId, bool value)
    {
        SendReadyInner(clientId, value);

        foreach (PlayerData player in allPlayerData)
        {
            if (player.clientId == clientId)
            {
                gettingReady = player.isReady;
                latestClientId = player.clientId;
            }
        }
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
            }
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        networkedGameObjects = FindObjectsOfType<PlayerAvatar>(true);

        combatLog.text = $"there are {networkedGameObjects.Count()} networkObjects in scene";
        isReadyLocal = false;
        scoreboardLogic.ModeScoreboard();
        scoreboardPanel.SetActive(false);
        SetUpScene();
        SetScoreBoardModeScore();
        Timer.Instance.StartTimer(30f);
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
        foreach (GameObject g in playStateObjects)
        {
            g.SetActive(true);
        }
    }

    private void DestroyScene()
    {
        foreach (GameObject g in playStateObjects)
        {
            g?.SetActive(false);
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
        DestroyScene();
        gameStarted = false;
        //scoreboardPanel.GetComponent<ScoreboardLogic>()?.ModeGameEnd();
        scoreboardLogic.ModeGameEnd();
        scoreboardPanel?.SetActive(true);
    }

    private void ShowLobby(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        //scoreboardPanel.GetComponent<ScoreboardLogic>().Initialize();
        scoreboardLogic.Initialize();
        scoreboardPanel.SetActive(true);
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


        UpdateLocalHealthClientRpc(clientId, MAXHEALTH);
        allPlayerData.Add(newPlayerData);

        foreach (var playerData in allPlayerData)
        {
            SetScoreboardClientRpc(playerData.clientId, playerData.name, playerData.score, playerData.deaths, playerData.color);
        }

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
                    UpdateLocalHealthClientRpc(target, newHealth);
                    //UpdateScoreboardDeathsClientRpc(target, newDeaths);
                    //ConveyDeathClientRpc(shooterId, target);
                    OnBoatDeathClientRpc(target);
                }

                if (!allPlayerData[i].isDead)
                {
                    UpdateLocalTextClientRpc(shooterId, target);
                    UpdateLocalHealthClientRpc(target, newHealth);

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

        //Increment the Score of the shooter 

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if ((allPlayerData[i].clientId == shooterId))
            {

                int newScore = allPlayerData[i].score + 1;

                UpdateLocalScoreClientRpc(shooterId, newScore);
                //UpdateScoreboardScoreClientRpc(shooterId, newScore);

                allPlayerData[i] = new PlayerData
                {
                    clientId = allPlayerData[i].clientId,
                    name = allPlayerData[i].name,
                    score = newScore,
                    health = allPlayerData[i].health,
                    deaths = allPlayerData[i].deaths,
                    isDead = allPlayerData[i].isDead,
                    color = allPlayerData[i].color,
                    isReady = allPlayerData[i].isReady,
                };
            }
        }
    }

    [ClientRpc]
    public void OnBoatDeathClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            localPlayer.GetComponentInChildren<ShipEffectController>(true).IsSunk();
            SinkIntoRespawnFunction(localPlayer);
        }

        foreach (PlayerAvatar player in networkedGameObjects)
        {
            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.gameObject.GetComponentInChildren<ShipEffectController>().IsSunk();
                SinkIntoRespawnFunction(player);
            }
        }
    }

    public void ReduceHealth(ulong shooterId, ulong targetId)
    {
        if (IsOwner)
        {
            ReduceHealthServerRpc(shooterId, targetId);
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

    private void SinkIntoRespawnFunction(PlayerAvatar player)
    {
        StartCoroutine(SinkIntoRespawn(player));
    }

    private IEnumerator SinkIntoRespawn(PlayerAvatar player)
    {
        yield return new WaitForSeconds(6);
        player.gameObject.GetComponentInChildren<ShipEffectController>().IsRespawn();
    }

    private void SinkIntoRespawnFunction(GameObject player)
    {
        StartCoroutine(SinkIntoRespawn(player));
    }

    private IEnumerator SinkIntoRespawn(GameObject player)
    {
        yield return new WaitForSeconds(6);
        player.GetComponentInChildren<ShipEffectController>().IsRespawn();
    }

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
    public void SetScoreboardClientRpc(ulong clientId, FixedString64Bytes name, int score, int deaths, FixedString64Bytes colorArg)
    {
        if (playerCards.ContainsKey(clientId)) return;

        CreatePlayerCard(clientId, name, score, deaths, colorArg);
    }




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
    void UpdateLocalHealthClientRpc(ulong clientId, int health)
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
}
