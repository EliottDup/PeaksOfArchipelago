using System;
using System.Collections;
using BepInEx.Logging;
using PeaksOfArchipelago;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

class Traps : MonoBehaviour
{
    enum Trap
    {
        Birds,
        Night,
    }
    Bird hunter = null;
    Bird crow = null;
    public static ManualLogSource logger;
    private GameObject trapList;
    public static Traps instance;
    public static PlayerData playerData;
    bool birdTrapRunning = false;
    private bool nightTrapRunning;
    OilLamp oilLamp;

    public void Awake()
    {
        oilLamp = FindObjectOfType<OilLamp>();
        if (!oilLamp) logger.LogError("Didn't find Oil Lamp!");
        hunter = GameObject.Find("SeaBird_Hunter").GetComponent<Bird>();
        if (!hunter) logger.LogError("Didn't find Hunter!");
        crow = GameObject.Find("CrowBird_Hunter").GetComponent<Bird>();
        if (!crow) logger.LogError("Didn't find Crow!");
    }

    public void Start()
    {
        // trapList = UIHandler.CreateClock("panel", new Color(1, 1, 1, 1), UIHandler.instance.canvas.transform, new Vector2(0, 0), new Vector2(100, 100), 0.5f, 0.5f);
        trapList = UIHandler.CreatePanel("trap list", new Color(0, 0, 0, 0.8f), FindObjectOfType<Canvas>()?.transform, new Vector2(10, 10), 0, 0);
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
            case Trap.Night:
                {
                    if (!StartNightTrap())
                    {
                        StartOneTimeTrap();
                    }
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
            return true;
        }
        return false;
    }

    public bool StartNightTrap()
    {
        if (!nightTrapRunning)
        {
            StartCoroutine(NightTrap());
            return true;
        }
        return false;
    }

    private IEnumerator NightTrap()
    {
        yield return new WaitForEndOfFrame();
        GameManager.control.alps_statue_sundown_InUse = true;
        FindObjectOfType<EnterPeakScene>().PublicSetSundown();
        if (playerData.items.lamp)
        {
            oilLamp.lampObj.SetActive(true);
            oilLamp.StartCoroutine("StartOilLamp", false);
        }
        nightTrapRunning = true;

        yield return StartTimer("Sundown", 30f);

        nightTrapRunning = false;
        if (playerData.items.lamp)
        {
            oilLamp.lampObj.SetActive(false);
        }
        GameManager.control.alps_statue_sundown_InUse = false;
        FindObjectOfType<EnterPeakScene>().PublicSetSundown();

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
            hunter.InitiateBird();
            crow.InitiateBird();

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