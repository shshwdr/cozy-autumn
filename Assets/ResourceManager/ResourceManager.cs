using Pool;
using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceInfo {
    public string type;
    public int originalValue;
}


public class ResourceManager : Singleton<ResourceManager>
{
    public List<ResourceInfo> resourceInfos = new List<ResourceInfo>();
    Dictionary<string, int> resouceAmount = new Dictionary<string, int>();
    void Start()
    {
        resourceInfos = CsvUtil.LoadObjects<ResourceInfo>("resource");
        foreach (var resource in resourceInfos)
        {
            resouceAmount[resource.type] = resource.originalValue;
        }

        EventPool.Trigger("updateResource");
    }


    public void consumeResource(string type, int value)
    {
        changeAmount(type, -value);
        if (!hasEnoughAmount(type,0))
        {
            GameManager.Instance.gameover();
        }
    }
    public void addResource(string type, int value)
    {
        changeAmount(type, value);
    }
    void changeAmount(string type, int value)
    {

        if (!resouceAmount.ContainsKey(type))
        {
            Debug.LogError("no key in resource " + type);
            return ;
        }
        resouceAmount[type] += value;
        EventPool.Trigger("updateResource");

    }

    public int getAmount(string type)
    {
        if (!resouceAmount.ContainsKey(type))
        {
            Debug.LogError("no key in resource " + type);
            return 0;
        }
        return resouceAmount[type];
    }

    public bool hasEnoughAmount(string type, int amount)
    {
        var hasAmount = getAmount(type);
        return hasAmount >= amount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
