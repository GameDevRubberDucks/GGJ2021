using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_SceneLoader : MonoBehaviour
{
    //--- Methods ---//
    public void LoadNewScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
