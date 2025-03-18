using UnityEngine;

public class ExitController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (GameManager.instance.FoundGoblins.Count > 0) 
        {
            GameManager.instance.Win();
        }
    }
}
