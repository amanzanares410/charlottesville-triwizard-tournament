using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private CharacterSelectionTracker selectionTracker;
    [SerializeField] private CharacterData[] characters;

    public void StartGame(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // Mark the selected character in the tracker
        selectionTracker.selectedCharacter = characters[characterIndex];
        SceneManager.LoadScene("SampleScene");
    }
}
