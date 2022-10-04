using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void init(string type) { }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual IEnumerator onNextStep() {
        yield return null;
    }

    public virtual void getKilled() { }
}
