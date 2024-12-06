using UnityEngine;

public class ShirtChanger : MonoBehaviour
{
    public CharacterSelectionData characterSelectionData; 
    public SkinnedMeshRenderer skinnedMeshRenderer; 
    public SkinnedMeshRenderer pantsRenderer; 
    public Material engMaterial; 
    public Material commMaterial;
    public Material nurseMaterial;
    public Material artsMaterial;

    void Start()
    {
        UpdateCharacterMaterial();
    }

    void UpdateCharacterMaterial()
    {
        if (characterSelectionData != null && skinnedMeshRenderer != null)
        {
            switch (characterSelectionData.selectedCharacterName)
            {
                case "Engineering":
                    skinnedMeshRenderer.material = engMaterial;
                    break;
                case "Commerce":
                    skinnedMeshRenderer.material = commMaterial;
                    break;
                case "Nursing":
                    skinnedMeshRenderer.material = nurseMaterial;
                    pantsRenderer.material = nurseMaterial;
                    break;
                case "Arts and Sciences":
                    skinnedMeshRenderer.material = artsMaterial;
                    break;
                default:
                    Debug.LogWarning("Unknown character selected. Defaulting to original material.");
                    break;
            }
        }
        else
        {
            Debug.LogError("CharacterSelectionData or SkinnedMeshRenderer is not assigned!");
        }
    }
}
