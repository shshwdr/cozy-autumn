using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    bool isGameOver = false;
    // Start is called before the first frame update
    void Start()
    {
        
        foreach (var re in FindObjectsOfType<MonoBehaviour>().OfType<CanReset>())
        {
            re.Reset();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            restartGame();
        }
    }

    public void restartGame()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void gameover()
    {
        if (isGameOver)
        {
            return;
        }
        isGameOver = true;

        PopupManager.Instance.showEvent("Game Over", () => { restartGame(); }, "Restart");
    }
}
