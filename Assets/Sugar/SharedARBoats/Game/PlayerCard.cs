using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, killsText, deathsText;

    public void Initialize(string name)
    {
        nameText.text = name;
        killsText.text = "0";
        deathsText.text = "0";
    }

    public void SetScore(int Score)
    {
        killsText.text = Score.ToString();
    }

    public void SetDeaths(int deaths) { 
        deathsText.text = deaths.ToString();
    }
}
