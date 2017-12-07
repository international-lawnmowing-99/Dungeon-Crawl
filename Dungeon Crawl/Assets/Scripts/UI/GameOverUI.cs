using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour {

    public void GoToMain()
    {
        SceneManager.LoadScene("Menu Scene");
    }
    public void GoToStart()
    {
        SceneManager.LoadScene("Game Scene");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
