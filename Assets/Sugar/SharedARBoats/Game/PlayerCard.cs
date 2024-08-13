using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, killsText, deathsText;

    [SerializeField] private Image colorIcon;

    [SerializeField] private GameObject[] lobbyItems;

    [SerializeField] private GameObject[] scoreboardItems;

    private void Start()
    {
        ModeLobby();
    }

    public void Initialize(FixedString64Bytes name)
    {
        nameText.text = name.ToString();
        killsText.text = "0";
        deathsText.text = "0";
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

    public void SetColor(FixedString64Bytes colorArg)
    {
        Color newCol;
        if (ColorUtility.TryParseHtmlString(colorArg.ToString(), out newCol))
        {
            colorIcon.color = newCol;
        }
    }

    public void SetScore(int Score)
    {
        killsText.text = Score.ToString();
    }

    public void SetDeaths(int deaths) { 
        deathsText.text = deaths.ToString();
    }
}
