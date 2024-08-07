using TMPro;
using Unity.Netcode;

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
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
        combatLog = FindObjectOfType<CombatLog>().GetComponent<TMP_Text>();
        localHealth = FindObjectOfType<LocalHealth>().GetComponent<TMP_Text>();
        localScore = FindObjectOfType<LocalScore>().GetComponent<TMP_Text>();

    }

    private void AddNewClientToList(ulong clientId)
    {
        if (!IsServer) return;

        foreach (var playerData in allPlayerData)
        {
            if (playerData.clientId == clientId) return;
        }

        PlayerData newPlayerData = new PlayerData
        {
            clientId = clientId,
            score = STARTSCORE,
            health = MAXHEALTH,
            isDead = false
        };

        UpdateLocalHealthClientRpc(clientId, MAXHEALTH);
        allPlayerData.Add(newPlayerData);
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

                int newHealth = allPlayerData[i].health - DAMAGE;

                if (newHealth <= 0)
                {
                    newHealth = 0;
                    allPlayerData[i] = new PlayerData
                    {
                        clientId = allPlayerData[i].clientId,
                        score = allPlayerData[i].score,
                        health = newHealth,
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
                        score = allPlayerData[i].score,
                        health = newHealth,
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

                allPlayerData[i] = new PlayerData
                {
                    clientId = allPlayerData[i].clientId,
                    score = newScore,
                    health = allPlayerData[i].health,
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
