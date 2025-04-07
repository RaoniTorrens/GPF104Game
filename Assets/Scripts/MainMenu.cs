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
    public string gameSceneName = "Level1"; // Name of the game scene to load

    void Start()
    {
        // Ensure buttons are assigned in the Inspector and add listeners
        if (startButton == null)
        {
            Debug.LogError("Start Button not assigned in the Inspector!");
        }
        else
        {
            // Add a listener to the start button to call StartGame() when clicked
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton == null)
        {
            Debug.LogError("Quit Button not assigned in the Inspector!");
        }
        else
        {
            // Add a listener to the quit button to call QuitGame() when clicked
            quitButton.onClick.AddListener(QuitGame);
        }

        if (titleText == null)
        {
            Debug.LogError("Title Text not assigned in the Inspector!");
        }
        else
        {
            // Set the title text
            titleText.text = "Goblin Rescue";
        }
    }

    // Loads the game scene when the Start button is clicked.
    public void StartGame()
    {
        Debug.Log("Start Game button clicked!");
        SceneManager.LoadScene(gameSceneName); // Load the game scene
    }

    
    // Quits the game when the Quit button is clicked.
    
    public void QuitGame()
    {
        Debug.Log("Quit Game button clicked!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Quit in the Unity Editor
        #else
            Application.Quit(); // Quit the application in a build
        #endif
    }
}
