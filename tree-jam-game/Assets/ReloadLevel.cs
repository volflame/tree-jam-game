using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class ReloadLevel : MonoBehaviour
{
    void Update()
    {
        // Check if the R key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Load the scene named "Level 1"
            SceneManager.LoadScene("Level 1");
        }
    }
}
