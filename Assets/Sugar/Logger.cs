using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public TMPro.TextMeshProUGUI consoleText; // Assign this in the Inspector
    public int maxLines = 10; // Maximum lines of logs to keep
    public int maxLogLength = 100;

    private Queue<string> logQueue = new Queue<string>();

    private void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type) {
        string logEntry = logString;

        if (logEntry.Length > maxLogLength) {
            logEntry = logEntry.Substring(0, maxLogLength) + "...";
        }

        // Add the new log to the queue
        logQueue.Enqueue(logEntry);

        // If we exceed the maximum number of lines, dequeue the oldest one
        if (logQueue.Count > maxLines) {
            logQueue.Dequeue();
        }

        // Update the TMP text
        consoleText.text = string.Join("\n", logQueue);
    }
}
