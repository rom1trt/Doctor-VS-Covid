using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SelectedPlayer
{
    None,
    Doctor,
    Virus
}

public class CharacterSelection : MonoBehaviour
{
    public SelectedPlayer selection = SelectedPlayer.None; // current client character selection
    // buttons
    public Button doctorSelect;
    public Button virusSelect;

    void Update()
    {
        // disable buttons for character that has already been selected
        doctorSelect.interactable = GameManager.instance.doctorAvailable;
        virusSelect.interactable = GameManager.instance.virusAvailable;
    }

    // button method to select the character on the selection menu
    public void SelectCharacterClient(int id)
    {
        switch (id)
        {
            case 0:
                if (GameManager.instance.doctorAvailable && selection == SelectedPlayer.None)
                {
                    selection = SelectedPlayer.Doctor;
                    GameManager.instance.SetSelectionState(id, false);
                }
                break;
            case 1:
                if (GameManager.instance.virusAvailable && selection == SelectedPlayer.None)
                {
                    selection = SelectedPlayer.Virus;
                    GameManager.instance.SetSelectionState(id, false);
                }
                break;
        }
    }

    // reset selection state
    public void ResetState()
    {
        selection = SelectedPlayer.None;
    }
}
