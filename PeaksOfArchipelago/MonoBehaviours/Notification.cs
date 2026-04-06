using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.MonoBehaviours
{
    public class Notification : MonoBehaviour
    {
        private Transform targetTransform;
        private Vector3 velocity = Vector3.zero;
        private Vector3 startPosition;
        private CanvasGroup canvasGroup;

        public void Init(string message, Transform target, Vector3 startPos)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Text t = GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = message;
            }
            targetTransform = target;
            startPosition = startPos;
            StartCoroutine(FadeTimer(2f, 0.5f));
        }

        private IEnumerator FadeTimer(float duration, float fadeDuration)
        {
            yield return null;
            transform.position = startPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            yield return new WaitForSeconds(duration);
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            yield return null;
            Destroy(targetTransform.gameObject);
            yield return null;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (targetTransform != null)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position, ref velocity, 0.5f);
            }
        }
    }
}