using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int score = 0;
    public void StopGame(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangeCharacter()
    {
        SceneManager.LoadScene("CharacterSelect");
    }
    
}