using System.Collections;
using BepInEx.Logging;
using UnityEngine;

class Traps : MonoBehaviour
{
    Bird hunter = null;
    Bird crow = null;
    public ManualLogSource logger;
    public void Start()
    {
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

    public void StartBirdTrap()
    {

        logger.LogInfo("HEEELP");
        StartCoroutine("BirdsTrap");
    }

    private IEnumerator BirdsTrap()
    {
        if (hunter != null && crow != null)
        {
            yield return new WaitForEndOfFrame();
            logger.LogInfo("activating birds");
            hunter.enabled = false;
            hunter.gameObject.SetActive(true);
            crow.enabled = false;
            crow.gameObject.SetActive(true);
            logger.LogInfo("activated birds");
            yield return new WaitForSeconds(15f);
            logger.LogInfo("deactivating birds");
            hunter.gameObject.SetActive(false);
            crow.gameObject.SetActive(false);
        }
        else
        {
            logger.LogInfo("birds are null?");
        }
    }
}