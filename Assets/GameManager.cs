using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    bool isGameOver = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
