using UnityEngine;

public class ScoreboardToggle : MonoBehaviour
{

    [SerializeField] GameObject scoreBoard;
    public void SetActive()
    {
        if (scoreBoard.activeSelf)
        {
            scoreBoard.SetActive(false);
        } else
        {
            scoreBoard.SetActive(true);
        }
    }
}
