using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{

    private static GameUIManager instance;

    //[SerializeField] private TMP_Text healthText;

    [SerializeField] private PlayerCard playerCardPrefab;

    [SerializeField] private Transform scoreboard;

    private Dictionary<int, PlayerCard> playerCards = new Dictionary<int, PlayerCard>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public static void PlayerJoin(int clientId)
    {
        PlayerCard newCard = Instantiate(instance.playerCardPrefab, instance.scoreboard);
        instance.playerCards.Add(clientId, newCard);
        newCard.Initialize(clientId.ToString());

    }

    public static void PlayerLeave(int clientId)
    {
        if (instance.playerCards.TryGetValue(clientId, out PlayerCard playerCard))
        {
            Destroy(playerCard.gameObject);
            instance.playerCards.Remove(clientId);
        }
    }
}
