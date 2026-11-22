using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    public GameObject startGame;
    public GameObject exitGame;
   public void OnStartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Audiomanager.PlaySound(SoundType.Button);
    }
    public void OnExitClicked() 
    {
        Audiomanager.PlaySound(SoundType.Button);
        Application.Quit();
        Debug.Log("I AM EXCITING CHILLL!!!");
    }

}
