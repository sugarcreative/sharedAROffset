using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ModalFade : MonoBehaviour
{
    public float lerptime = 0.25f;
    private CanvasGroup TheCanvas;
    //public float opacityValue;

    [HideInInspector] public bool showing = false;
    public bool ShowOnStart = false;

    void OnEnable()
    {
        TheCanvas = GetComponent<CanvasGroup>();
    }

    private void Awake() {
        if (ShowOnStart == false) {
            TheCanvas = GetComponent<CanvasGroup>();
            ForceOff();
        } else {
            showing = true;
        }
    }

    public void ForceOn() {
        TheCanvas = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        TheCanvas.alpha = 1;
        showing = true;
        StopAllCoroutines();
    }

    public void ForceOff() {
        TheCanvas = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        TheCanvas.alpha = 0;
        showing = false;
        StopAllCoroutines();
    }

    public void Toggle() {
        if(showing) {
            Hide();
        } else {
            Show();
        }
    }

    public void Show()
    {
        //Debug.Log("SHOWING MODAL");
        if (!showing)
        {
            
            gameObject.SetActive(true);
            StopAllCoroutines();

            showing = true;
            //opacityValue = 0;
            StartCoroutine(Fade(1, false));
        }
    }

    public void Hide()
    {
        if (showing)
        {
            if (gameObject.activeSelf == false)
            {
                return;
            }
            StopAllCoroutines();

            showing = false;
            //opacityValue = 1;
            StartCoroutine(Fade(0, true));
        }
    }

    public IEnumerator Fade(float end, bool turnOffAfter)
    {

        float Start = TheCanvas.alpha;
        float End = end;

        for (float t = 0f; t < lerptime; t += Time.deltaTime)
        {
            
            float normalizedTime = t / lerptime;
            TheCanvas.alpha = Mathf.Lerp(Start, End, normalizedTime);
            //TheCanvas.alpha = opacityValue;
            yield return null;
        }

        TheCanvas.alpha = end;

        if (turnOffAfter)
        {
            this.gameObject.SetActive(false);
        }
    }
}
