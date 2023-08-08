using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Move to main menu after 3 seconds
        Invoke("MoveToMainMenu", 3);
    }

    // Load main menu and unload intro
    private void MoveToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Intro");
    }
}
