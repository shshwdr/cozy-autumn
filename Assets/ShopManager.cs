using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ShopInfo {
    public string type;
    public Dictionary<string,int> cost;
        public string description;

}

public class ShopManager : Singleton<ShopManager>
{
    public Transform shopItems;
    List<ShopInfo> shopInfos;
    Dictionary<string,ShopInfo> shopDict = new Dictionary<string, ShopInfo>();
    public List<string> unPurchasedShopInfos = new List<string>();

    public List<string> purchasedShopInfos = new List<string>();
    // Start is called before the first frame update
    void Start()
    {

        shopInfos = CsvUtil.LoadObjects<ShopInfo>("shop");
        foreach (var info in shopInfos)
        {
            shopDict[info.type] = info;
            unPurchasedShopInfos.Add(info.type);
        }
    }

    public ShopInfo getInfo(string type)
    {

        if (!shopDict.ContainsKey(type))
        {
            Debug.LogError("no key for " + type);
        }
        return shopDict[type];
    }

    public bool canAffort(string type)
    {
        foreach(var pair in getInfo(type).cost)
        {
            if (!ResourceManager.Instance.hasEnoughAmount(pair.Key, pair.Value))
            {
                return false;
            }
        }
        return true;
    }

    public void purchase(string type)
    {
        foreach (var pair in getInfo(type).cost)
        {
            ResourceManager.Instance.consumeResource(pair.Key, pair.Value);
        }
        unPurchasedShopInfos.Remove(type);
        purchasedShopInfos.Add(type);

        shopItems.Find(type).gameObject.SetActive(true);

    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
