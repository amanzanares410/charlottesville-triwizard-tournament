using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] private CharacterSelectionData selectionData;
    //[SerializeField] private Sprite[] characterSprites;
    [SerializeField] private string[] characterNames;

    public void SelectCharacter(int characterIndex)
    {
        selectionData.selectedCharacterName = characterNames[characterIndex];
        //selectionData.selectedCharacterSprite = characterSprites[characterIndex];
        SceneManager.LoadScene("SampleScene");
    }
}

