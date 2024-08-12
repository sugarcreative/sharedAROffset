using UnityEngine;

public class ScoreboardToggle : MonoBehaviour
{

    [SerializeField] GameObject scoreBoard;
    public void SetActive()
    {
        if (scoreBoard.activeInHierarchy)
        {
            scoreBoard.SetActive(false);
        } else
        {
            scoreBoard.SetActive(true);
        }
    }
}
