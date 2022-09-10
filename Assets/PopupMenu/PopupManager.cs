using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    public GameObject EventMenuPrefab;
    public Canvas mainCanvas;
    public bool playTest = false;
    private void Start()
    {
        if (!mainCanvas)
        {
            mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
            if (!mainCanvas)
            {
                Debug.LogError("no main canvas found!");
                mainCanvas = GetComponent<Canvas>();
            }
        }
        if (playTest)
        {

            StartCoroutine(test());
        }
    }

    IEnumerator test()
    {

        showEvent("just show some text");

        yield return new WaitForSeconds(1);
        showEvent("Show some text with special button text", "NOOO!");

        yield return new WaitForSeconds(1);

        showEvent(new List<string>() { "Show some text with multiple follow ups", "follow up 1", "follow up 2" }, "Done");
        yield return new WaitForSeconds(1);

        showEvent(new List<string>() { "Show some text with multiple follow ups", "follow up 1", "follow up 2" }, "Done");
        yield return new WaitForSeconds(1);

        showEvent("Show some text with multiple selection", new List<EventButtonInfo>(){ new EventButtonInfo("make background red", () =>
        {
            Camera.main.backgroundColor = Color.red;
        }),

         new EventButtonInfo("make background blue", () =>
        {

            Camera.main.backgroundColor = Color.blue;
        }),
        });
    }

    public void showEvent(string t, string button1Text = "OK")
    {
        // only show one text and can close it.
        var go = Instantiate(EventMenuPrefab, mainCanvas.transform);

        List<EventButtonInfo> simpleTwoOption = new List<EventButtonInfo>();
        //simpleTwoOption.Add(new EventButtonInfo(t, button1Text, null));
        go.GetComponent<PopupMenu>().Init(t, simpleTwoOption);
        go.GetComponent<PopupMenu>().showView();
    }
    public void showEvent(string t, Action action,string button1Text = "OK")
    {
        // only show one text and can close it.
        var go = Instantiate(EventMenuPrefab, mainCanvas.transform);

        List<EventButtonInfo> simpleTwoOption = new List<EventButtonInfo>();
        simpleTwoOption.Add(new EventButtonInfo(t, action));
        go.GetComponent<PopupMenu>().Init(t, simpleTwoOption);
        go.GetComponent<PopupMenu>().showView();
    }

    public void showEvent(List<string> t, string button1Text = "OK")
    {
        List<EventButtonInfo> eventButtons = new List<EventButtonInfo>();
        int index = 0;
        //eventButtons.Add(new EventButtonInfo(t[index], button1Text, () => { }));
        showEvent(t, eventButtons);
    }

    public void showEvent(string t, List<EventButtonInfo> bf, Sprite image = null)
    {
        showEvent(new List<string>() { t }, bf, image);
    }

    public void showEvent(List<string> t, List<EventButtonInfo> bf, Sprite image = null)
    {
        var go = Instantiate(EventMenuPrefab, mainCanvas.transform);
        go.GetComponent<PopupMenu>().Init(t, bf, image);
        go.GetComponent<PopupMenu>().showView();

    }

    public void showEventImageString(List<string> t, List<EventButtonInfo> bf, string image = null)
    {

        if (image != null)
        {
            var sprite = Resources.Load<Sprite>("icons/" + image);
            if (sprite)
            {

                showEvent(t, bf, sprite);
            }
            else
            {
                showEvent(t, bf, null);

            }
        }
        else
        {
            showEvent(t, bf, null);
        }
    }


}
