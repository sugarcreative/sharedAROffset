using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDepthMaskController : MonoBehaviour
{
    public bool lerpOnStart;
    public bool destroyOnFinish;
    public float lerpDuration = 2.0f;

    private MeshRenderer mr;
    private float lerpValue = 0f;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();

        if (mr != null)
        {
            mr.material.SetFloat("_FadeShift", 0);
        }

        if (lerpOnStart)
        {
            Dissolve();
        }
    }

    public void Dissolve()
    {
        StartCoroutine(StartDissolve());
    }

    IEnumerator StartDissolve()
    {
        if (mr != null)
        {
            float startValue = 0f;
            float endValue = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < lerpDuration)
            {
                // Calculate the Lerp value based on elapsed time and duration
                lerpValue = Mathf.Lerp(startValue, endValue, elapsedTime / lerpDuration);
                mr.material.SetFloat("_FadeShift", lerpValue);

                // Update the elapsed time
                elapsedTime += Time.deltaTime;

                // Yield until the next frame
                yield return null;
            }

            // Ensure the final value is exactly the end value
            lerpValue = endValue;
            if (destroyOnFinish)
            {
                Destroy(this.gameObject);
            }
        }
    }
}