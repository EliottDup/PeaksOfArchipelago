using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PeaksOfArchipelago.MonoBehaviours
{
    public class Notificator : MonoBehaviour
    {
        public GameObject notificationPrefab;
        private Transform notificationParent;

        private void Awake()
        {
            notificationParent = transform.GetChild(0);
        }

        public void CreateNotification(string message)
        {
            GameObject notifLocation = new GameObject("NotifLoc", typeof(RectTransform));
            notifLocation.transform.SetParent(notificationParent);
            notifLocation.transform.SetAsFirstSibling();

            GameObject notification = Instantiate(notificationPrefab, transform);
            Notification notificationScript = notification.AddComponent<Notification>();
            if (notificationScript)
            {
                notification.SetActive(true);
                notificationScript.Init(message, notifLocation.transform, transform.position + Vector3.up * 500f);
            }
        }
    }
}
