using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // This is a singleton
    public static GameManager instance { get; private set; }

    // Editable on Editor
    [SerializeField]
    protected List<GameObject> FoundGoblins;
    private TMP_Text WinLoseTextElement { get; set; }

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;    
    }

    void Start()
    {
        FoundGoblins = new List<GameObject>();
        WinLoseTextElement = GetComponentInChildren<TMP_Text>();
        WinLoseTextElement.alpha = 0;
    }

    public void Rescue(GameObject goblin)
    {
        Debug.Log("RESCUE");
        FoundGoblins.Add(goblin);
    }

    public void Win()
    {
        Debug.Log("WIN");
        WinLoseTextElement.text = "VICTORY";
        WinLoseTextElement.alpha = 100;
        
        // Quit Scene
    }

    public void Lose()
    {
        WinLoseTextElement.text = "DEFEAT";
        WinLoseTextElement.alpha = 100;
        
        // Quit Scene
    }
}
