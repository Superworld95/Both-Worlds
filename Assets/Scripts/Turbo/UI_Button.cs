using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UI_Button : MonoBehaviour
{
    public void StartButtons()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitButton()
    {
        Application.Quit();
        print("exit");
    }
}
