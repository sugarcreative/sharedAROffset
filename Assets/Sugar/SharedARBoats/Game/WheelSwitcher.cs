using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSwitcher : MonoBehaviour
{
    public int startWheel = 0;
    public float fadeDuration;
    public Vector3 fadeInDirection, fadeOutDirection;
    public float switchDelay = 0.5f;

    private int currentWheel, oldWheel;
    private float lerpValueA, lerpValueB, lerpValueC, lerpValueD;

    [SerializeField] private MeshRenderer[] wheels;

    public MeshRenderer splinters;
    public SkinnedMeshRenderer ribbon;

    public bool resetAtStart;

    private void Start()
    {
        if (resetAtStart)
        {
            NewWheel();
        }
    }

    public void NewWheel()
    {
        StopAllCoroutines();
        currentWheel = startWheel;
        oldWheel = startWheel;

        if (splinters != null)
        {
            splinters.material.SetFloat("_FadeShift", 0);
            splinters.gameObject.SetActive(false);
        }

        if (ribbon != null)
        {
            StartCoroutine(FadeMeshIn(ribbon));
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].gameObject.SetActive(false);
        }

        StartCoroutine(FadeWheelIn(startWheel));
    }

    public void SwitchWheel(int index)
    {
        if (index != currentWheel)
        {
            StopAllCoroutines();
            wheels[currentWheel].material.SetFloat("_FadeShift", 1);
            wheels[oldWheel].material.SetFloat("_FadeShift", 0);

            if (oldWheel != currentWheel)
            {
                wheels[oldWheel].gameObject.SetActive(false);
            }

            StartCoroutine(FadeWheelOut(currentWheel));
            StartCoroutine(FadeWheelIn(index));

            if (ribbon != null)
            {
                ribbon.material.SetFloat("_FadeShift", 1);
                ribbon.gameObject.SetActive(true);
            }

            if (splinters != null)
            {
                if (index == 2)
                {
                    StartCoroutine(FadeMeshIn(splinters));
                }
                else
                {
                    if (splinters.gameObject.activeSelf)
                    {
                        StartCoroutine(FadeMeshOut(splinters));
                    }
                }
            }

            oldWheel = currentWheel;
            currentWheel = index;
        }
    }

    public void WheelDeath()
    {
        StopAllCoroutines();

        if (splinters.gameObject.activeSelf)
        {
            StartCoroutine(FadeMeshOut(splinters));
        }

        if (ribbon.gameObject.activeSelf)
        {
            StartCoroutine(FadeMeshOut(ribbon));
        }

        StartCoroutine(FadeWheelOut(currentWheel));

        oldWheel = currentWheel;
    }

    IEnumerator FadeWheelIn(int wheel)
    {
        yield return new WaitForSeconds(switchDelay);

        wheels[wheel].gameObject.SetActive(true);

        wheels[wheel].material.SetVector("_FadeDirection", fadeInDirection);

        float startValue = 0f;
        float endValue = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {

            lerpValueA = Mathf.Lerp(startValue, endValue, elapsedTime / fadeDuration);

            wheels[wheel].material.SetFloat("_FadeShift", lerpValueA);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final value is exactly the end value
        lerpValueA = endValue;

        wheels[wheel].material.SetFloat("_FadeShift", lerpValueA);
    }

    IEnumerator FadeWheelOut(int wheel)
    {
        wheels[wheel].material.SetVector("_FadeDirection", fadeOutDirection);

        float startValue = 1f;
        float endValue = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {

            lerpValueB = Mathf.Lerp(startValue, endValue, elapsedTime / fadeDuration);

            wheels[wheel].material.SetFloat("_FadeShift", lerpValueB);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final value is exactly the end value
        lerpValueB = endValue;

        wheels[wheel].material.SetFloat("_FadeShift", lerpValueB);
        wheels[wheel].gameObject.SetActive(false);

    }

    IEnumerator FadeMeshIn(Renderer mr)
    {
        mr.gameObject.SetActive(false);

        yield return new WaitForSeconds(switchDelay);

        mr.gameObject.SetActive(true);
        mr.material.SetVector("_FadeDirection", fadeOutDirection);

        float startValue = 0;
        float endValue = 1;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {

            lerpValueC = Mathf.Lerp(startValue, endValue, elapsedTime / fadeDuration);

            mr.material.SetFloat("_FadeShift", lerpValueC);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final value is exactly the end value
        lerpValueC = endValue;

        mr.material.SetFloat("_FadeShift", lerpValueC);
    }

    IEnumerator FadeMeshOut(Renderer mr)
    {
        mr.material.SetVector("_FadeDirection", fadeOutDirection);

        float startValue = 1;
        float endValue = 0;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {

            lerpValueD = Mathf.Lerp(startValue, endValue, elapsedTime / fadeDuration);

            mr.material.SetFloat("_FadeShift", lerpValueD);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final value is exactly the end value
        lerpValueD = endValue;

        mr.material.SetFloat("_FadeShift", lerpValueD);
        mr.gameObject.SetActive(false);
    }
}
