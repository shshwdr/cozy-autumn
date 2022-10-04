using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{

    AudioSource audioSource;
    public void play(string name)
    {
        var clip = Resources.Load<AudioClip>("sfx/" + name);
        if (!clip)
        {
            Debug.Log("no clip");
            return;
        }
        audioSource.PlayOneShot(clip);
    }
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
