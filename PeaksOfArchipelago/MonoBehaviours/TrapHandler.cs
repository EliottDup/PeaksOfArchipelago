using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.Extensions;
using PeaksOfArchipelago.Traps;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.MonoBehaviours
{
    internal class TrapHandler : MonoBehaviour
    {
        public static TrapHandler Instance = null;

        private Transform trapDisplay;

        private List<Trap> traps = [
            ];

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Initialize(GameObject trapDisplay)
        {
            this.trapDisplay = trapDisplay.transform;
        }

        public void StartRandomTrap()
        {
            List<Trap> availableTraps = traps.Where(t => t.IsAvailable()).ToList();
            if (availableTraps.Count == 0) return;
            Trap trap = availableTraps[UnityEngine.Random.Range(0, availableTraps.Count)];
            PeaksOfArchipelago.ui.SendNotification(trap.Message);
            StartCoroutine(trap.Execute(this));
        }

        internal void StartTimer(string name, float duration, Action onEnd)
        {
            StartCoroutine(Timer(name, duration, onEnd));
        }
        
        private IEnumerator Timer(string name, float duration, Action onEnd)
        {
            yield return new WaitForEndOfFrame();
            Instantiate(PeaksOfAssets.TrapTimer, trapDisplay);
            
            Text text = trapDisplay.FindDeep("TEXT").GetComponent<Text>();
            text.text = name;
            
            Slider timer = trapDisplay.FindDeep("SLIDER").GetComponent<Slider>();
            timer.normalizedValue = 1f;
            
            float t = 0;
            while (t <= 1)
            {
                t += Time.deltaTime / duration;
                timer.normalizedValue = 1f - t;
                yield return null;
            }
            
            Destroy(timer.gameObject);
            onEnd?.Invoke();
        }
    }
}
