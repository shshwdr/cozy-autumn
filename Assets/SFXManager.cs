using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{

    AudioSource audioSource;
    public void play(string name)
    {
        var clip = Resources.Load<AudioClip>("sfx/" + name);
        audioSource.PlayOneShot(clip);
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
