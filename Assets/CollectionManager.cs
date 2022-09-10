using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
[Serializable]
public class PairInfo<T>
{
	public string Key;
	public T Value;
	public PairInfo(string k, T v)
	{
		Key = k;
		Value = v;
	}
}
public class CollectionManager : Singleton<CollectionManager>
{
	//References
	[Header("UI references")]
	[SerializeField] TMP_Text coinUIText;
	[SerializeField] GameObject animatedCoinPrefab;

	[Space]
	[Header("Available coins : (coins to pool)")]
	[SerializeField] int maxCoins;
	Queue<GameObject> coinsQueue = new Queue<GameObject>();


	[Space]
	[Header("Animation settings")]
	[SerializeField] [Range(0.5f, 0.9f)] float minAnimDuration;
	[SerializeField] [Range(0.9f, 2f)] float maxAnimDuration;

	[SerializeField] Ease easeType;
	[SerializeField] float spread;

    ResourceMenu hud;

	private int _c = 0;

	public int Coins
	{
		get { return _c; }
		set
		{
			_c = value;
			//update UI text whenever "Coins" variable is changed
			//coinUIText.text = Coins.ToString();
		}
	}

	void Awake()
	{


		//prepare pool
		PrepareCoins();
	}

	void PrepareCoins()
	{
		GameObject coin;
		for (int i = 0; i < maxCoins; i++)
		{
			coin = Instantiate(animatedCoinPrefab);
			coin.transform.parent = transform;
			coin.SetActive(false);
			coinsQueue.Enqueue(coin);
		}
	}

	void MoveSource(Vector3 start, Vector3 end, string property, int amount, Action action)
	{
		for (int i = 0; i < amount; i++)
		{
			//check if there's coins in the pool
			if (coinsQueue.Count > 0)
			{
				//extract a coin from the pool
				GameObject coin = coinsQueue.Dequeue();
				coin.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("resource/" + property);
				coin.SetActive(true);

				//move coin to the collected coin pos
				coin.transform.position = start + new Vector3(Random.Range(-spread, spread), 0f, 0f);




				//animate coin to target position
				float duration = Random.Range(minAnimDuration, maxAnimDuration);
				coin.transform.DOMove(end, duration)
				.SetEase(easeType)
				.OnComplete(() =>
				{
					//executes whenever coin reach target position
					coin.SetActive(false);
					coinsQueue.Enqueue(coin);
					action();
				});
			}
		}
	}

	//public void RemoveCoins(Vector3 removedCoinPosition, List<PairInfo<int>> resource, bool onlyForNonIncreasingResrouce = false)
	//{
	//    if (resource == null)
	//    {
	//        return;
	//    }
	//    foreach (var pair in resource)
	//    {
	//        //if(onlyForNonIncreasingResrouce && PlantsManager.Instance.isIncreasingResource(pair.Key))
	//        //         {
	//        //	continue;
	//        //         }
	//        var amount = pair.Value;
	//        //get target position
	//        var target = HUD.Instance.hudByProperty[pair.Key].GetComponent<OneStatHud>().image.transform;
	//        Vector3 screenPoint = target.position + new Vector3(0, 0, 5);  //the "+ new Vector3(0,0,5)" ensures that the object is so close to the camera you dont see it

	//        //find out where this is in world space
	//        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPoint);




	//        MoveSource(worldPos, removedCoinPosition, pair.Key, pair.Value, () =>
	//        {
	//            PlantsManager.Instance.currentResource[pair.Key] -= 1;
	//            BeeManager.Instance.updateGenerateTime();
	//            PestManager.Instance.updateGenerateTime();
	//        });
	//    }
	//}

	public void AddCoins(Vector3 collectedCoinPosition, List<PairInfo<int>> resource, bool isIncreasingResource = true)
    {
        foreach (var pair in resource)
        {
            //if (PlantsManager.Instance.isIncreasingResource(pair.Key) != isIncreasingResource)
            //{
            //    continue;
            //}
            var amount = pair.Value;
            //get target position
            var target = hud.getCellTransform(resource[0].Key); //HUD.Instance.hudByProperty[pair.Key].GetComponent<OneStatHud>().image.transform;
            Vector3 screenPoint = target.position + new Vector3(0, 0, 5);  //the "+ new Vector3(0,0,5)" ensures that the object is so close to the camera you dont see it

            //find out where this is in world space
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPoint);


            MoveSource(collectedCoinPosition, worldPos, pair.Key, pair.Value, () =>
            {
				ResourceManager.Instance.addResource(pair.Key, 1);
                //PlantsManager.Instance.currentResource[pair.Key] += 1;
                //BeeManager.Instance.updateGenerateTime();
                //PestManager.Instance.updateGenerateTime();
                //TutorialManager.Instance.collectResource(pair.Key);
            });
        }

    }
    private void Start()
    {
        hud = GameObject.FindObjectOfType<ResourceMenu>(true);
    }
}