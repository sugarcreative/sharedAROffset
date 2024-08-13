using Unity.Netcode;
using UnityEngine;

public class ScoreboardLogic : NetworkBehaviour
{
    public GameObject[] scoreboardItems;

    public GameObject[] lobbyItems;

    private void Start()
    {
        ModeLobby();
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
}
