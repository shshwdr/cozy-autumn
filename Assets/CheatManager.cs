using Doozy.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatManager : Singleton<CheatManager>
{
    public bool _wontDie = false;


    public bool isOn = true;

    public bool wontDie
    {
        get
        {

            return isOn && _wontDie;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ResourceManager.Instance.addResource("nut", 10);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                ResourceManager.Instance.addResource("stick", 10);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                ResourceManager.Instance.addResource("stone", 10);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                _wontDie = true;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                RuleManager.Instance.unlockAll();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                AchievementManager.Instance.unlockAll();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                ShopManager.Instance.purchaseAll();
            }


            if (Input.GetKeyDown(KeyCode.B))
            {
                GameManager.Instance.gameover();
            }

        }
    }
}
