using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public Button quitButton;
    public TMP_Text titleText;

    [Header("Scene to Load")]
    public string gameSceneName = "Level1"; // Set the name of your game scene here

    void Start()
    {
        // Ensure buttons are assigned in the Inspector
        if (startButton == null)
        {
            Debug.LogError("Start Button not assigned in the Inspector!");
        }
        else
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton == null)
        {
            Debug.LogError("Quit Button not assigned in the Inspector!");
        }
        else
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (titleText == null)
        {
            Debug.LogError("Title Text not assigned in the Inspector!");
        }
        else
        {
            titleText.text = "Goblin Rescue";
        }
    }

    public void StartGame()
    {
        Debug.Log("Start Game button clicked!");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button clicked!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
