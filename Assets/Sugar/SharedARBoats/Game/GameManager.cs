using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager Instance;


    #region Timer
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private float time;
    #endregion

    //[SerializeField] private bool hasGameStarted = false;

    //[SerializeField] private bool hasGameEnded = false;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        //scoreboardPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        StartMatchTimerLocal();
    }

    void StartGame()
    {
        if (!IsServer) return;

        //StartGameLocalClientRpc();
    }

    void StartMatchTimerLocal()
    {

        //if (!hasGameStarted) return;

        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else if (time <= 0)
        {
            time = 0;
            //hasGameEnded = true;
        }

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ShowScoreboard()
    {
        //scoreboardPanel.SetActive(true);
    }

    void HideScoreboard()
    {
        //scoreboardPanel.SetActive(false);
    }

    [ClientRpc]
    void StartGameLocalClientRpc()
    {
        //hasGameStarted = true;
    }

    [ClientRpc]
    void ShowScoreboardClientRpc()
    {
        ShowScoreboard();
    }

    [ClientRpc]
    void HideScoreboardClientRpc()
    {
        HideScoreboard();
    }
}
