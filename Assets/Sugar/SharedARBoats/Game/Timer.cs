using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Instance;

    [SerializeField] private float time;

    [SerializeField] private TMP_Text timerText;

    public bool timerActive = false;

    public static event Action onGameEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerActive) return;
        tickTimer();
    }

    public void StartTimer(float timeToSet)
    {
        time = timeToSet;
        timerActive = true;
        UpdateTimerText();
    }

    private void tickTimer()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            time = Mathf.Max(time, 0);
            UpdateTimerText();
        }
        else
        {
            timerActive = false;
            EndGame();
        }
    }

    private void UpdateTimerText()
    {
        //timerText.text = time.ToString("F2");
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }

    private void EndGame()
    {
        onGameEnd?.Invoke();
    }
}
