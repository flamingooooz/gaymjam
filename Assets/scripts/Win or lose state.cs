using UnityEngine;
using UnityEngine.SceneManagement;

public class Winorlosestate : MonoBehaviour
{
    public GameObject win;
    public GameObject lose;
    public GameObject gameWin;
    public float elapsed =0;
    public int show = 222;

    public void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= show)
        {
            ShowWin(true);
        }
    }

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
            lose.SetActive(show);
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
}
