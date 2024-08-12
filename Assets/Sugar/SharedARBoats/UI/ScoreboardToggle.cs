using UnityEngine;

public class ScoreboardToggle : MonoBehaviour
{

    [SerializeField] GameObject scoreBoard;
    public void SetActive()
    {
        bool boolToSet = scoreBoard.gameObject.activeInHierarchy ? false: true;
        scoreBoard.SetActive(boolToSet);
    }
}
