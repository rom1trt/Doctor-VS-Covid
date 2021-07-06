using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomescreenButtons : MonoBehaviour
{
   
   public void PlayGame() // Loads the scene "Game" when button "Play" is clicked
   {
      SceneManager.LoadScene("Demo");
   }

   public void Quit() // Closes the application when the button "Quit" is clicked
   {
      Application.Quit();
   }
}
  