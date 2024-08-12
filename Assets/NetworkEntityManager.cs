using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkEntityManager : NetworkBehaviour
{
    public static NetworkEntityManager Instance;

    private NetworkList<PlayerData> allPlayerData;

    private const int MAXHEALTH = 10;

    private const int STARTSCORE = 0;

    private const int DAMAGE = 1;

    private TMP_Text combatLog;

    private TMP_Text localHealth;

    private TMP_Text localScore;

    [SerializeField] private PlayerCard playerCardPrefab;

    [SerializeField] Transform playerCardParent;

    private List<ulong> clientIds = new List<ulong>();

    private Dictionary<ulong, PlayerCard> playerCards = new Dictionary<ulong, PlayerCard>(); 


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
        allPlayerData = new NetworkList<PlayerData>();
        combatLog = FindObjectOfType<CombatLog>().GetComponent<TMP_Text>();
        localHealth = FindObjectOfType<LocalHealth>().GetComponent<TMP_Text>();
        localScore = FindObjectOfType<LocalScore>().GetComponent<TMP_Text>();

    }

    [ServerRpc (RequireOwnership = false)]
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

        PlayerData newPlayerData = new PlayerData
        {
            clientId = clientId,
            name = name,
            score = STARTSCORE,
            health = MAXHEALTH,
            deaths = 0,
            isDead = false
        };


        UpdateLocalHealthClientRpc(clientId, MAXHEALTH);
        allPlayerData.Add(newPlayerData);

        foreach (var playerData in allPlayerData)
        {
            SetScoreboardClientRpc(playerData.clientId, playerData.name, playerData.score, playerData.deaths);
        }


    }


    [ServerRpc(RequireOwnership = false)]
    public void ReduceHealthServerRpc(ulong shooterId, ulong target)
    {
        if (!IsServer) return;

        //Reduce the health of target

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientId == target)
            {

                if (allPlayerData[i].isDead) return;

                int newDeaths = allPlayerData[i].deaths + 1;
                int newHealth = allPlayerData[i].health - DAMAGE;

                if (newHealth <= 0)
                {
                    newHealth = 0;
                    
                    allPlayerData[i] = new PlayerData
                    {
                        clientId = allPlayerData[i].clientId,
                        name = allPlayerData[i].name,
                        score = allPlayerData[i].score,
                        health = newHealth,
                        deaths = newDeaths,
                        isDead = true
                    };
                    UpdateLocalHealthClientRpc(target, newHealth);
                    ConveyDeathClientRpc(shooterId, target);
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
                        isDead = allPlayerData[i].isDead
                    };
                }
                break;
            }
        }

        //Increment the Score of the shooter 

        for (int i = 0;i < allPlayerData.Count; i++)
        {
            if ((allPlayerData[i].clientId == shooterId))
            {

                int newScore = allPlayerData[i].score + 1;

                UpdateLocalScoreClientRpc(shooterId, newScore);
                UpdateScoreboardScoreClientRpc(shooterId, newScore);

                allPlayerData[i] = new PlayerData
                {
                    clientId = allPlayerData[i].clientId,
                    name = allPlayerData[i].name,
                    score = newScore,
                    health = allPlayerData[i].health,
                    deaths = allPlayerData[i].deaths,
                    isDead = allPlayerData[i].isDead
                };

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

    public void CreatePlayerCard(ulong clientId, FixedString64Bytes name, int kills, int deaths)
    {
        PlayerCard newCard = Instantiate(playerCardPrefab, playerCardParent);
        playerCards.Add(clientId, newCard);
        newCard.Initialize(name);
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
    public void SetScoreboardClientRpc(ulong clientId, FixedString64Bytes name, int score, int deaths)
    {
        if (playerCards.ContainsKey(clientId)) return;

        CreatePlayerCard(clientId, name, score, deaths);
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
            combatLog.text = $"{shooterId} has sunk your boat!";
        }
    }
}
