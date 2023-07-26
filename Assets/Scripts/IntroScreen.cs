using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("MoveToMainMenu", 3);
    }

    private void MoveToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Intro");
    }
}
