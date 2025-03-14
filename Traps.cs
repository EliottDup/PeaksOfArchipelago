using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Traps : MonoBehaviour
{
    Bird hunter = null;
    Bird crow = null;
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
        StartCoroutine("BirdsTrap");
    }

    private IEnumerator BirdsTrap()
    {
        hunter.gameObject.SetActive(true);
        crow.gameObject.SetActive(true);
        yield return new WaitForSeconds(15f);
        hunter.gameObject.SetActive(false);
        crow.gameObject.SetActive(false);
    }
}