// Loads scenes and exits on Esc

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    private int curScene = 0;
    private string[] scenes = new string[]{
        "Tutorial (Very Easy)",
        "Tutorial (Easy)",
        "Tutorial (Involved)",
        "Level1",
        "Level2",
        "Level3",
        "Level4",
        "Level5",
        "Level6",
        "End Screen"
    };

    // Loads the scene with the curScene number
    public void LoadNextScene() {
        if (curScene >= scenes.Length) {
            return;
        }
        
        SceneManager.LoadScene(scenes[curScene]);
        curScene++;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) == true) {
            Quit();
        }
    }

    public void Quit() {
        Application.Quit();
    }
}
