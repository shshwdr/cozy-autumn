using Doozy.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventButtonInfo
{
    public string text;
    //public bool isRestart;
    public Action action;

    public EventButtonInfo(string t, Action a)
    {
        text = t;
        action = a;
        //isRestart = re;
        // resultText = r;
    }
}
public class PopupMenu : BaseView
{

    public Text text;
    public List<Button> reactButtons;

    public Image eventImage;
    public GameObject eventObject;

    List<EventButtonInfo> buttonInfo;
    public override void showView()
    {
        base.showView();
        GetComponent<UIView>().Show();
    }

    public override void hideView()
    {

        base.hideView();
        Destroy(gameObject);

        GetComponent<UIView>().Hide();


    }

    void clearButton()
    {
        foreach (var button in reactButtons)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    void updateImage(Sprite image)
    {
        if (image != null)
        {
            eventImage.sprite = image;
            eventObject.SetActive(true);
        }
        else
        {
            eventObject.SetActive(false);
        }
    }

    void updateRealButtons(List<string> t, List<EventButtonInfo> bf)
    {

        clearButton();
        text.text = t[t.Count-1];
        for (int i = 0; i < buttonInfo.Count; i++)
        {
            reactButtons[i].gameObject.SetActive(true);
            reactButtons[i].GetComponentInChildren<Text>().text = buttonInfo[i].text;
            var info = buttonInfo[i];
            reactButtons[i].onClick.AddListener(() =>
            {
                hideView();
                if (info.action != null)
                {
                    info.action();
                }
            }
            );
        }
    }

    void updateMainTextView(List<string> t, List<EventButtonInfo> bf, int textIndex)
    {
        
        clearButton();
        text.text = t[textIndex];

        textIndex++;

        if (textIndex >= t.Count)
        {
            if (bf!=null)
            {

                updateRealButtons(t, bf);
            }
            else
            {

                hideView();
            }
        }
        else
        {
            reactButtons[0].gameObject.SetActive(true);
            reactButtons[0].GetComponentInChildren<Text>().text = "Continue";
            reactButtons[0].onClick.AddListener(() =>
            {
                updateMainTextView(t, bf,textIndex);
            }
            );
        }
    }

    public void Init(string t, List<EventButtonInfo> bf, Sprite image = null)
    {
        Init(new List<string>() { t }, bf, image);
    }

    public void Init(List<string> t, List<EventButtonInfo> bf, Sprite image = null)
    {
        if (t.Count <= 0)
        {
            Debug.LogError("no text");
            return;
        }
        text.text = t[0];
        buttonInfo = bf;

        updateImage(image);

        updateMainTextView(t, bf, 0);
    }

    public void destroy()
    {
        Destroy(gameObject);
    }

    //void setupTutorialNextLine(string resultText)
    //{
    //    var nextLine = TutorialManager.Instance.getNextLine(resultText);
    //    if (nextLine != null)
    //    {

    //        text.text = TutorialManager.Instance.getTutorialLine(nextLine);

    //        clearButton();
    //        reactButtons[0].gameObject.SetActive(true);
    //        reactButtons[0].GetComponentInChildren<Text>().text = "OK";
    //        reactButtons[0].onClick.AddListener(() =>
    //        {
    //            setupTutorialNextLine(nextLine);
    //        });
    //    }
    //    else
    //    {

    //        hideView();
    //    }
    //}

}
