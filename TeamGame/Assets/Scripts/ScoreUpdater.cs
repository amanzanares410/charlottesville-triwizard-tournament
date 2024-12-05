using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText; // Displays the current score
    [SerializeField] private TextMeshProUGUI milestoneText; // Displays milestone messages
    [SerializeField] private GameCompletion completionManager;

    private int currentScore = 0;

    // Array of milestone thresholds and their corresponding messages.
    private (int threshold, string message)[] milestones = new (int, string)[]
    {
        (250, "First Year Completed!"),
        (500, "Second Year Completed!"),
        (1000, "Third Year Completed!"),
        (1500, "Fourth Year Completed!"),
    };

    // Track whether the special object has been shown

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void updateScore(int score)
    {
        currentScore = score;
        scoreText.text = score.ToString();
        UpdateMilestone(score);
    }

    private void UpdateMilestone(int score)
    {
        string milestoneMessage = "Keep Going!";

        // Check milestones
        foreach (var milestone in milestones)
        {
            if (score >= milestone.threshold)
            {
                milestoneMessage = milestone.message;
            }
        }
        // Update the milestoneText UI element
        if (milestoneText != null)
        {
            milestoneText.text = milestoneMessage;
        }
        else
        {
            Debug.LogError("Milestone TextMeshProUGUI is not assigned in the inspector.");
        }
    }

}