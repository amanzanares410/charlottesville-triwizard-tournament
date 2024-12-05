using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TempleRun.Player;
using UnityEngine;

public class GameCompletion : MonoBehaviour
{
    [SerializeField] private GameObject completionScreen;
    [SerializeField] private ScoreUpdater scoreUpdater;
    [SerializeField] private PlayerController playerController;

    private bool gameCompleted = false;
    // Start is called before the first frame update
    void Start()
    {
        if (completionScreen != null)
        {
            completionScreen.SetActive(false);
        }
    }

    public void CheckScore()
    {
        int score = scoreUpdater.GetCurrentScore();
        if (score >= 1500 && !gameCompleted)
        {
            gameCompleted = true;
            CompleteGame();
        }
    }

    private void CompleteGame()
    {
        if (completionScreen != null)
        {
            completionScreen.SetActive(true);
        }

        if (playerController != null) 
        {
            playerController.StopPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckScore();
    }
}
