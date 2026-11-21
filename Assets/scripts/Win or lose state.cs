using UnityEngine;
using UnityEngine.SceneManagement;

public class Winorlosestate : MonoBehaviour
{
    public GameObject win;
    public GameObject lose;
    public GameObject gameWin;
  
    public void ShowWin(bool show)
    { 
        if(show == true)
        {
            win.SetActive(show);
        }
    }
    public void ShowLose(bool show)
    {
        if (show == true)
        {
            gameWin.SetActive(show);
        }
    }
    public void ShowGameWin(bool show)
    {
        if (show == true)
        {
            lose.SetActive(show);
        }
    }
    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; 
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("WINNER");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
           
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("REload");
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    public void FirstLevel()
    {
        SceneManager.LoadScene("forest");
    }
}
