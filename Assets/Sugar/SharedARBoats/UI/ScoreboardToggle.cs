using UnityEngine;

public class ScoreboardToggle : MonoBehaviour
{   
    public void SetActive()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
