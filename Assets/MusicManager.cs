using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainMenu;
    public AudioClip game;

    // Start is called before the first frame update
    void Start()
    {
        if(StageManager.Instance.currentStage == "")
        {
            GetComponent<AudioSource>().clip = mainMenu;
        }
        else
        {
            GetComponent<AudioSource>().clip = game;
        }
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
