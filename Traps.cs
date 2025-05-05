using System;
using System.Collections;
using BepInEx.Logging;
using PeaksOfArchipelago;
using UnityEngine;
using UnityEngine.UI;

class Traps : MonoBehaviour
{
    enum Trap
    {
        Birds,
        Night,
        Holds,
        Gravity,
        OneTime
    }
    enum SingleTimeTrap
    {
        Ropes,
        Chalk,
        Coffee,
        Seeds
    }
    Bird hunter = null;
    Bird crow = null;
    public static ManualLogSource logger;
    private GameObject trapList;
    public static Traps instance;
    public static PlayerData playerData;
    bool birdTrapRunning = false;
    private bool nightTrapRunning;
    private bool holdsTrapRunning;
    private bool gravityTrapRunning;

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
        trapList.SetActive(false);
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
            case Trap.Holds:
                {
                    if (!StartHoldsTrap())
                    {
                        StartOneTimeTrap();
                    }
                    break;
                }
            case Trap.Gravity:
                {
                    if (!StartGravityTrap())
                    {
                        StartOneTimeTrap();
                    }
                    break;
                }
            case Trap.OneTime:
                {
                    StartOneTimeTrap();
                    break;
                }
        }
    }

    public bool StartGravityTrap()
    {
        if (!gravityTrapRunning)
        {
            StartCoroutine(GravityTrap());
            return true;
        }
        return false;
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

    public void StartOneTimeTrap()
    {
        switch ((SingleTimeTrap)UnityEngine.Random.Range(0, 4))
        {
            case SingleTimeTrap.Ropes:
                {
                    RopeAnchor ra = GameObject.FindObjectOfType<RopeAnchor>();
                    if (ra == null || ra.anchorsInBackpack <= 0)
                    {
                        break;
                    }
                    ra.anchorsInBackpack -= 1;
                    ra.StartCoroutine("RopesLeftTooltip");
                    UIHandler.instance.Notify("You dropped a rope");
                    return;
                }
            case SingleTimeTrap.Coffee:
                {
                    CoffeeDrink cd = GameObject.FindObjectOfType<CoffeeDrink>();
                    if (cd == null || cd.coffeeSipsLeft <= 0)
                    {
                        break;
                    }
                    cd.coffeeSipsLeft -= 1;
                    cd.StartCoroutine("CoffeeLeftToolTip");
                    UIHandler.instance.Notify("Your coffee spilled");
                    return;
                }
            case SingleTimeTrap.Chalk:
                {
                    ChalkBag cb = GameObject.FindObjectOfType<ChalkBag>();
                    if (cb == null || cb.chalkBagUsesLeft <= 0)
                    {
                        break;
                    }
                    cb.chalkBagUsesLeft -= 1;
                    cb.StartCoroutine("ChalkLeftToolTip", false);
                    UIHandler.instance.Notify("You found a hole in your chalk bag");
                    return;
                }
            case SingleTimeTrap.Seeds:
                {
                    ChalkBag cb = GameObject.FindObjectOfType<ChalkBag>();
                    if (cb == null || cb.birdseedsUsesLeft <= 0)
                    {
                        break;
                    }
                    cb.birdseedsUsesLeft -= 1;
                    cb.StartCoroutine("ChalkLeftToolTip", true);
                    UIHandler.instance.Notify("You realise you forgot a portion of bird seeds");
                    return;
                }
        }
        UIHandler.instance.Notify("Nothing Happened...");
    }

    public bool StartHoldsTrap()
    {
        if (!holdsTrapRunning)
        {
            string[] traps = ["CrimpsTrap", "SlopersTrap", "PitchesTrap"];
            StartCoroutine(traps[UnityEngine.Random.Range(0, traps.Length)]);
            return true;
        }
        return false;
    }

    private IEnumerator NightTrap()
    {
        yield return new WaitForEndOfFrame();
        UIHandler.instance.Notify("Sudden Eclipse");
        logger.LogInfo("Starting Night Trap");
        GameManager.control.alps_statue_sundown_InUse = true;
        FindObjectOfType<EnterPeakScene>().PublicSetSundown();
        GameManager.control.alps_statue_sundown_InUse = false;

        if (playerData.items.lamp)
        {
            oilLamp.lampObj.SetActive(true);
            oilLamp.StartCoroutine("StartOilLamp", false);
        }
        nightTrapRunning = true;

        yield return StartTimer("Eclipse", 60f);

        nightTrapRunning = false;
        if (playerData.items.lamp)
        {
            oilLamp.lampObj.SetActive(false);
        }
        FindObjectOfType<EnterPeakScene>().PublicSetSundown();

    }

    private IEnumerator BirdsTrap()
    {
        if (hunter != null && crow != null)
        {
            yield return new WaitForEndOfFrame();
            UIHandler.instance.Notify("You've awakened the birds's wrath");
            logger.LogInfo("activating birds");
            hunter.gameObject.SetActive(true);
            crow.gameObject.SetActive(true);
            birdTrapRunning = true;
            hunter.InitiateBird();
            crow.InitiateBird();

            yield return StartTimer("Birds!", 30f);

            birdTrapRunning = false;
            logger.LogInfo("deactivating birds");
            hunter.gameObject.SetActive(false);
            crow.gameObject.SetActive(false);
        }
        else
        {
            UIHandler.instance.AddChatMessage("Something went wrong, please report this to the dev :) \n (you can keep playing)");
            logger.LogInfo("birds are null?");
        }
    }

    private IEnumerator CrimpsTrap()
    {
        yield return new WaitForEndOfFrame();
        holdsTrapRunning = true;
        GameObject[] array = GameObject.FindGameObjectsWithTag("Climbable");
        UIHandler.instance.Notify("All the holds seem so small today");
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].name != "ClimbableSloper_Normal" && array[i].name != "ClimbableSloper_Slippery" &&
                array[i].name != "ClimbableSloper_Easy" && array[i].name != "ClimbableSloper_Rain" &&
                array[i].name != "ClimbableSloper_Rain_Brick")
            {
                array[i].tag = "ClimbableMicroHold";
            }
        }

        yield return StartTimer("Oops, all crimps!", 30f);

        yield return new WaitForEndOfFrame();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].name != "ClimbableSloper_Normal" && array[i].name != "ClimbableSloper_Slippery" &&
                array[i].name != "ClimbableSloper_Easy" && array[i].name != "ClimbableSloper_Rain" &&
                array[i].name != "ClimbableSloper_Rain_Brick")
            {
                array[i].tag = "Climbable";
            }
        }
        holdsTrapRunning = false;
    }

    private IEnumerator SlopersTrap()
    {
        yield return new WaitForEndOfFrame();
        UIHandler.instance.Notify("Your hands feel greasy");
        holdsTrapRunning = true;

        GameObject[] array = GameObject.FindGameObjectsWithTag("Climbable");
        string[] array2 = new string[array.Length];
        for (int j = 0; j < array.Length; j++)
        {
            array2[j] = array[j].name;
            array[j].name = "ClimbableSloper_Easy";
        }
        yield return StartTimer("Oops, all slopers!", 15f);

        yield return new WaitForEndOfFrame();

        for (int j = 0; j < array.Length; j++)
        {
            array[j].name = array2[j];
        }

        holdsTrapRunning = false;
    }

    private IEnumerator PitchesTrap()
    {
        yield return new WaitForEndOfFrame();
        UIHandler.instance.Notify("You seem to tire quicker than usual");
        holdsTrapRunning = true;

        GameObject[] array = GameObject.FindGameObjectsWithTag("Climbable");
        for (int j = 0; j < array.Length; j++)
        {
            array[j].tag = "ClimbablePitch";
        }
        yield return StartTimer("Oops, all pitches!", 30f);

        yield return new WaitForEndOfFrame();

        for (int j = 0; j < array.Length; j++)
        {
            array[j].tag = "Climbable";
        }

        holdsTrapRunning = false;
    }

    private IEnumerator GravityTrap()
    {
        yield return new WaitForEndOfFrame();
        UIHandler.instance.Notify("How much porridge did ye have for breakfast?");
        gravityTrapRunning = true;
        Physics.gravity = new Vector3(0f, -10.692901f, 0f);

        yield return StartTimer("Feeling Heavy", 30f);
        Physics.gravity = new Vector3(0f, -9.81f, 0f);
        gravityTrapRunning = false;
    }

    private IEnumerator StartTimer(string name, float duration)
    {
        yield return new WaitForEndOfFrame();
        trapList.SetActive(true);
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
        if (trapList.transform.childCount == 0)
        {
            trapList.SetActive(false);
        }
    }
}