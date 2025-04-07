using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Set singleton - make sure only one instance of GameManager exists
    public static GameManager instance { get; private set; }

    
    public AudioManager audioManager; 
    public List<GameObject> FoundGoblins { get; private set; } 
    private TMP_Text WinLoseTextElement { get; set; } // Text to display "VICTORY" or "DEFEAT"

    //References to ui objects
    public GameObject mainUI; // Reference to the main UI GameObject
    public GameObject gameOverUI; // Reference to the game over UI GameObject

    [Header("Scene to Load")]
    public string mainMenuSceneName = "MainMenu"; // Name of the main menu scene to load


    void Awake()
    {
        // Singleton pattern implementation
        if (instance != null && instance != this)
            Destroy(this); // Destroy duplicate instances
        else
            instance = this;    // Set this instance as the singleton
    }

    void Start()
    {
        FoundGoblins = new List<GameObject>(); // Initialise the list of found goblins
        WinLoseTextElement = GetComponentInChildren<TMP_Text>(); 
        WinLoseTextElement.alpha = 0; // Initially hide Win/Lose text
        
        // Check if a Canvas component exists
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas component found on this GameObject.");
            return;
        }
        
        Debug.Log($"Canvas Render Mode: {canvas.renderMode}"); // Log the canvas render mode
        Debug.Log($"Assigned Camera: {(canvas.worldCamera != null ? canvas.worldCamera.name : "None")}"); // Log the assigned camera
    }

    void Update()
    {
        // Check for the Escape key to quit the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    
    // Adds a rescued goblin to the FoundGoblins list.
    public void Rescue(GameObject goblin)
    {
        Debug.Log("RESCUE");
        FoundGoblins.Add(goblin);
    }

    
    // Starts the win sequence, displaying "VICTORY" and returning to the main menu after a delay.
        public void Win()
    {
        StartCoroutine(ShowVictoryAndLoadMainMenu(5f));
    }

    
    // Starts the lose sequence, displaying "DEFEAT" and returning to the main menu after a delay.
         public void Lose()
    {
        StartCoroutine(ShowDefeatAndLoadMainMenu(5f));
    }

    
    
    // Loads the main scene of the game.
        private void LoadMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }

    
    /// Quit the game.
        void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Quit in the Unity Editor
        #else
            Application.Quit(); // Quit the application in a build
        #endif
    }
    
    
    // Displays "DEFEAT" for a specified time and then loads the main menu.
        private IEnumerator ShowDefeatAndLoadMainMenu(float delay)
    {
        WinLoseTextElement.text = "DEFEAT"; // Set the text to "DEFEAT"
        WinLoseTextElement.alpha = 1; // Change to 1 for visibility 
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        SceneManager.LoadScene(mainMenuSceneName); // Load the main menu scene
    }

    
    // Displays "VICTORY" for a specified time and then loads the main menu.
       private IEnumerator ShowVictoryAndLoadMainMenu(float delay)
    {
        Debug.Log("WIN");
        WinLoseTextElement.text = "VICTORY"; // Set the text to "VICTORY"
        WinLoseTextElement.alpha = 1; // Change to 1 for visibility 
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        SceneManager.LoadScene(mainMenuSceneName); // Load the main menu scene
    }
}
