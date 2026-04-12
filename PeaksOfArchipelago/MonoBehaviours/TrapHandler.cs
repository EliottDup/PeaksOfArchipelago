using BepInEx.Logging;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.Extensions;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.Traps;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.MonoBehaviours
{
    internal class TrapHandler : MonoBehaviour
    {
        public static TrapHandler Instance = null;

        private ManualLogSource logger;

        private Transform trapDisplay;

        private List<Trap> traps;

        Bird hunter;
        Bird crow;
        OilLamp oilLamp;
        EnterPeakScene peakEntry;
        GameObject[] holds;
        String[] holdNames;

        RopeAnchor ropeAnchor;
        CoffeeDrink coffeeDrink;
        ChalkBag chalkBag;

        bool holdsTrapRunning = false;

        private IEnumerator TestTrap()
        {
            yield return null;
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            logger = PeaksOfArchipelago.Logger;

            logger.LogInfo("Trap handler initialising");

            oilLamp = FindObjectOfType<OilLamp>();
            hunter = GameObject.Find("Seabird_Hunter")?.GetComponent<Bird>();
            crow = GameObject.Find("CrowBird_Hunter")?.GetComponent<Bird>();
            peakEntry = FindObjectOfType<EnterPeakScene>();
            holds = GameObject.FindGameObjectsWithTag("Climbable");
            holdNames = new string[holds.Length];
            ropeAnchor = FindObjectOfType<RopeAnchor>();
            coffeeDrink = FindObjectOfType<CoffeeDrink>();
            chalkBag = FindObjectOfType<ChalkBag>();

            for (int i = 0; i < holds.Length; i++)
            {
                holdNames[i] = holds[i].name;
            }

            Trap eclipse = new TimedTrap("Eclipse", "The moon briefly obscures the sun.", () =>
            {
                GameManager.control.alps_statue_sundown_InUse = !GameManager.control.alps_statue_sundown_InUse;
                peakEntry.PublicSetSundown();
                GameManager.control.alps_statue_sundown_InUse = !GameManager.control.alps_statue_sundown_InUse;

                if (Connection.Instance.slotData.HasTool(GameData.Tools.Lamp))
                {
                    oilLamp.lampObj.SetActive(true);
                    oilLamp.StartCoroutine("StartOilLamp", false);
                }
            }, () =>
            {
                peakEntry.PublicSetSundown();
            }, 60f, () => oilLamp != null && peakEntry != null);

            Trap birds = new TimedTrap("Bird Attack!", "You've stirred the nest.", () =>
            {
                hunter.gameObject.SetActive(true);
                crow.gameObject.SetActive(true);
                hunter.InitiateBird();
                crow.InitiateBird();
            }, () =>
            {
                hunter.gameObject.SetActive(false);
                crow.gameObject.SetActive(false);
            }, 30f);

            Trap gravity = new TimedTrap("Feeling heavy", "How much porridge did ye have for breakfast", () =>
            {
                Physics.gravity = new Vector3(0f, -10.692901f, 0f);
            }, () =>
            {
                Physics.gravity = new Vector3(0f, -9.81f, 0f);
            }, 30f);

            Trap crimps = new TimedTrap("Oops, all crimps!", "The holds feel small today", () =>
            {
                holdsTrapRunning = true;
                for (int i = 0; i < holds.Length; i++)
                {
                    if (!holds[i].name.StartsWith("ClimbableSloper_"))
                    {
                        holds[i].tag = "ClimbableMicroHold";
                    }
                }
            }, () =>
            {
                for (int i = 0; i < holds.Length; i++)
                {
                    if (holds[i].name.StartsWith("ClimbableSloper_"))
                    {
                        holds[i].tag = "Climbable";
                    }
                }
                holdsTrapRunning = false;
            }, 15f, () => !holdsTrapRunning);

            Trap slopers = new TimedTrap("Oops, all slopers!", "You feel your hands sweating profusely", () =>
            {
                holdsTrapRunning = true;
                for (int i = 0; i < holds.Length; i++)
                {
                    if (!holds[i].name.StartsWith("ClimbableSloper_"))
                    {
                        holds[i].name = "ClimbableSloper_Easy";
                    }
                }
            }, () =>
            {
                for (int i = 0; i < holds.Length; i++)
                {
                    holds[i].name = holdNames[i];
                }
                holdsTrapRunning = false;
            }, 15f, () => !holdsTrapRunning);

            Trap pitches = new TimedTrap("Oops, all pitches!", "Your fingers are tiring", () =>
            {
                holdsTrapRunning = true;
                for (int i = 0; i < holds.Length; i++)
                {
                    holds[i].tag = "ClimbablePitch";
                }
            }, () =>
            {
                for (int i = 0; i < holds.Length; i++)
                {
                    holds[i].tag = "Climbable";
                }
                holdsTrapRunning = false;
            }, 15f, () => !holdsTrapRunning);

            Trap ropeLoss = new InstantTrap("A rope slipped off of your body, -1 rope", () => ropeAnchor.anchorsInBackpack--, () => ropeAnchor.anchorsInBackpack > 0);
            Trap coffeeLoss = new InstantTrap("You found a leak in your coffee bottle, -1 coffee", () => coffeeDrink.coffeeSipsLeft--, () => coffeeDrink.coffeeSipsLeft > 0);
            Trap chalkLoss = new InstantTrap("There was a hole in your chalk bag, -1 chalk", () => chalkBag.chalkBagUsesLeft--, () => chalkBag.chalkBagUsesLeft > 0);
            Trap birdSeedLoss = new InstantTrap("There was a hole in your seeds bag, -1 seed use", () => chalkBag.birdseedsUsesLeft--, () => chalkBag.birdseedsUsesLeft > 0);
            
            traps = [eclipse, birds, gravity, crimps, slopers, pitches, ropeLoss, coffeeLoss, chalkLoss, birdSeedLoss];
        }



        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartRandomTrap()
        {
            List<Trap> availableTraps = traps.Where(t => t.IsAvailable()).ToList();
            if (availableTraps.Count == 0) {
                logger.LogInfo("No traps available");
                return;
            }
            Trap trap = availableTraps[UnityEngine.Random.Range(0, availableTraps.Count)];
            PeaksOfArchipelago.ui.SendNotification(trap.Message);
            trap.Execute(this);
        }

        internal void StartTimer(string name, float duration, Action onEnd)
        {
            this.trapDisplay = PeaksOfArchipelago.ui.trapDisplay.transform;
            StartCoroutine(Timer(name, duration, onEnd));
        }
        
        private IEnumerator Timer(string name, float duration, Action onEnd)
        {
            yield return new WaitForEndOfFrame();
            Transform timer = Instantiate(PeaksOfAssets.TrapTimer, trapDisplay).transform;
            
            Text text = timer.FindDeep("TRAPNAME").GetComponent<Text>();
            text.text = name;
            
            Slider slider = timer.FindDeep("SLIDER").GetComponent<Slider>();
            slider.normalizedValue = 1f;
            
            float t = 0;
            while (t <= 1)
            {
                t += Time.deltaTime / duration;
                slider.normalizedValue = 1f - t;
                yield return null;
            }
            
            Destroy(timer.gameObject);
            onEnd?.Invoke();
        }
    }
}
