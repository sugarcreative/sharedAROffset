using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardLogic : NetworkBehaviour
{
    public GameObject[] scoreboardItems;

    public GameObject[] lobbyItems;

    public Button readyButton;

    public void Initialize()
    {
        ModeLobby();

        if (IsServer)
        {
            readyButton.interactable = false;
        }
    }

    public void ModeLobby()
    {
        foreach (GameObject item in scoreboardItems)
        {
            item.SetActive(false);
        }

        foreach (GameObject item in lobbyItems)
        {
            
            item.SetActive(true);
        }
    }

    public void ModeScoreboard()
    {
        foreach (GameObject item in lobbyItems)
        {
            item.SetActive(false);
        }
        foreach (GameObject item in scoreboardItems)
        {
            item.SetActive(true);
        }
    }

    public void ModeGameEnd()
    {
        readyButton.gameObject.SetActive(true);
    }
}
