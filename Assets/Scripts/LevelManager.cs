using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public string[] scenes;

    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(scenes, currentScene);

        if (currentIndex == -1)
        {
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex < scenes.Length)
        {   
            var cutsceneController = FindFirstObjectByType<CutsceneController>();
            if (cutsceneController != null)
                cutsceneController.PlayCutScene(CutsceneAction.ShowBlackPanel);

            SceneManager.LoadScene(scenes[nextIndex]);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
