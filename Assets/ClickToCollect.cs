using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToCollect : MonoBehaviour
{
    bool isClicked = false;
    //public DropboxType dropboxType;
    //public HelperPlant parentPlant;
    // public string dropboxType;
    public List<PairInfo<int>> resource;
    //public HelperPlantType unlockPlant;
    public float speed = 1f;
    public float amplitude = 1f;
    public bool needClick;
    public GameObject fullGameobject;

    // Start is called before the first frame update
    void Start()
    {
    }
    static public GameObject createClickToCollectItem(List<PairInfo<int>> r, Vector3 pos, bool needClick = false)
    {
        GameObject collectInBattlePrefab = Resources.Load<GameObject>("prefabs/clickToCollect");
        var go = Instantiate(collectInBattlePrefab, pos, Quaternion.identity, CollectionManager.Instance.transform);
        var box = go.GetComponentInChildren<ClickToCollect>();
        //box.dropboxType = DropboxType.resource;
        box.resource = r;
        box.UpdateImage();
        box.needClick = needClick;
        return go;
    }
    public void UpdateImage()
    {

        string maxP = "";
        int maxV = 0;
        foreach (var pair in resource)
        {
            if (pair.Value > maxV)
            {
                maxV = pair.Value;
                maxP = pair.Key;
            }
        }
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("resourceRoom/" + maxP); //JsonManager.Instance.getItemInfo(maxP).sprite;//  HUD.Instance.propertyImage[(int)(maxP));
    }

    private void OnMouseDown()
    {
        collect();
    }

    private void OnMouseOver()
    {

        if (!needClick)
        {
            collect();


        }
    }

    void collect()
    {
        //if (parentPlant)
        //{
        //    parentPlant.resourceCollect();
        //}
        if (!isClicked)
        {
            isClicked = true;
        }
        //if (dropboxType == DropboxType.unlock)
        {
            //PlantsManager.Instance.UnlockPlant(unlockPlant);
            //BirdManager.Instance.needToUnlock[unlockPlant] = false;
            //TutorialManager.Instance.firstSeeSomething("unlock");
            //CollectionManager.Instance.AddCoins(transform.position, resource);
        }
        //else
        {
            //if (GameManager.Instance.isInBattle)
            {
                //Inventory.Instance.addItem(resource);
            }
            //else
            {

                CollectionManager.Instance.AddCoins(transform.position, resource);
            }
            // ResourceManager.Instance.changeAmount(resource[0].Key, resource[0].Value);
        }
        Destroy(fullGameobject);

        transform.parent.gameObject.SendMessage("finishCollect");
    }

    void Update()
    {
        var verticalMove = Mathf.Sin(Time.realtimeSinceStartup * speed) * amplitude * Vector3.up * Time.deltaTime;
        transform.position += verticalMove;
    }
}
