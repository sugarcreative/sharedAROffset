using TMPro;
using Unity.Netcode;

public class NetworkEntityManager : NetworkBehaviour
{
    public static NetworkEntityManager Instance;

    private NetworkList<PlayerData> allPlayerData;

    private const int MAXHEALTH = 3;

    private const int DAMAGE = 1;

    private TMP_Text text;

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
        text = FindObjectOfType<CombatLog>().GetComponent<TMP_Text>();
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
            score = 0,
            health = MAXHEALTH
        };

        allPlayerData.Add(newPlayerData);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ReduceHealthServerRpc(ulong shooterId, ulong target)
    {
        if (!IsServer) return;

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
                    ConveyDeathClientRpc(shooterId, target);
                    return;
                }

                UpdateLocalTextClientRpc(shooterId, target);

                allPlayerData[i] = new PlayerData
                {
                    clientId = allPlayerData[i].clientId,
                    score = allPlayerData[i].score,
                    health = newHealth
                };

                break;
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
        text.text = $"You have been shot by {shooterId}";
    }

    [ClientRpc]
    void ConveyDeathClientRpc(ulong shooterId, ulong targetId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetId)
        {
            text.text = $"{shooterId} has sunk your boat!";
        }
    }
}
