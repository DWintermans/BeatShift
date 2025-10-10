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
            Debug.LogError("Current scene not in array");
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex < scenes.Length)
        {
            Debug.Log("Loading: " + scenes[nextIndex]);
            SceneManager.LoadScene(scenes[nextIndex]);
        }
        else
        {
            Debug.Log("No more levels, returning to Main Menu");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
