using System;
using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

class Traps : MonoBehaviour
{
    enum Trap
    {
        Birds,
        Rope,
    }
    Bird hunter = null;
    Bird crow = null;
    public static ManualLogSource logger;
    private GameObject trapList;
    public static Traps instance;
    bool birdTrapRunning = false;

    public void Start()
    {
        // trapList = UIHandler.CreateClock("panel", new Color(1, 1, 1, 1), UIHandler.instance.canvas.transform, new Vector2(0, 0), new Vector2(100, 100), 0.5f, 0.5f);
        trapList = UIHandler.CreatePanel("trap list", new Color(0, 0, 0, 0.8f), UIHandler.instance.canvas.transform, new Vector2(10, 10), 0, 0);

        StartCoroutine(StartTimer("Test1", 60f));
        StartCoroutine(StartTimer("Test2", 10f));
        StartCoroutine(StartTimer("Test3", 20f));
        StartCoroutine(StartTimer("Test4", 30f));

        Bird[] birds = GameObject.FindObjectsOfType<Bird>();
        foreach (Bird bird in birds)
        {
            print(bird.name + " " + bird.isHunter + " " + bird.isCrow);
            if (bird.isHunter)
            {
                if (bird.isCrow)
                {
                    print("Found Crow");
                    crow = bird;
                }
                else
                {
                    print("found Hunter");
                    hunter = bird;
                }
            }
        }
    }

    public void StartTrap()
    {
        Trap trap = ((Trap[])Enum.GetValues(typeof(Trap)))[UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trap)).Length)];
        switch (trap)
        {
            case Trap.Birds:
                {
                    if (!StartBirdTrap())
                    {
                        StartOneTimeTrap();
                    }
                    break;
                }
            case Trap.Rope:
                {
                    break;
                }
        }
    }

    void StartOneTimeTrap()
    {

    }


    public bool StartBirdTrap()
    {
        if (!birdTrapRunning)
        {
            StartCoroutine(BirdsTrap());
            return false;
        }
        return true;
    }

    private IEnumerator BirdsTrap()
    {
        if (hunter != null && crow != null)
        {
            yield return new WaitForEndOfFrame();
            logger.LogInfo("activating birds");
            hunter.gameObject.SetActive(true);
            crow.gameObject.SetActive(true);
            birdTrapRunning = true;

            yield return StartTimer("Birds!", 60f);

            birdTrapRunning = false;
            logger.LogInfo("deactivating birds");
            hunter.gameObject.SetActive(false);
            crow.gameObject.SetActive(false);
        }
        else
        {
            logger.LogInfo("birds are null?");
        }
    }


    private IEnumerator StartTimer(string name, float duration)
    {
        GameObject p = UIHandler.CreatePanel(name + " clock", new Color(0, 0, 0, 0.8f), trapList.transform, Vector2.zero, vertical: false);
        Text t = UIHandler.CreateTextElem("name", 32, p.transform);
        t.text = name;
        GameObject c = UIHandler.CreateClock("clock", Color.white, p.transform, Vector2.zero, 36);
        Image i = c.GetComponent<Image>();
        // i.rectTransform.sizeDelta = Vector2.one * t.rectTransform.sizeDelta.y;
        logger.LogInfo(t.rectTransform.sizeDelta.y);
        logger.LogInfo(i.rectTransform.sizeDelta);
        yield return null;
        logger.LogInfo(i.rectTransform.sizeDelta);


        float timer = 0;
        while (timer <= 1)
        {
            timer += Time.deltaTime / duration;
            i.fillAmount = 1 - timer;
            yield return null;
        }
        logger.LogInfo(i.rectTransform.sizeDelta);
        Destroy(p);
    }
}