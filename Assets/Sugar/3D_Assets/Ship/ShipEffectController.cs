using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ShipEffectController : MonoBehaviour
{
    private Animator anim;

    public GameObject shipDeathParticles, waterDeathParticles;
    //public RaycastCannon[] cannonFirePositionsLeft, cannonFirePositionsRight;
    public Transform deathExplosionPosition;

    public SkinnedMeshRenderer sailsUnfurled;
    public GameObject sailsFurled;
    public float furlTrigger = 95;

    public float waterHeight;

    private MeshRenderer mr;

    public float respawnDuration = 2.0f;
    private float lerpValue;

    public List<Renderer> allRenderers = new List<Renderer>();

    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
       
        anim = GetComponent<Animator>();

        mr = GetComponent<MeshRenderer>();

        if (sailsFurled != null && sailsUnfurled != null)
        {
            sailsFurled.SetActive(false);

            float a = sailsUnfurled.GetBlendShapeWeight(0);

            if (a > furlTrigger)
            {
                sailsFurled.SetActive(true);
            }
            else
            {
                sailsUnfurled.gameObject.SetActive(true);
            }
        }

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        allRenderers.AddRange(meshRenderers);

        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        allRenderers.AddRange(skinnedMeshRenderers);
    }


    public void CheckFurl(float value)
    {
        if (sailsFurled != null && sailsUnfurled != null)
        {
            if (value > furlTrigger)
            {
                if (sailsFurled.activeInHierarchy == false)
                {
                    sailsUnfurled.gameObject.SetActive(false);
                    sailsFurled.SetActive(true);
                }
            }
            else
            {
                if (sailsFurled.activeInHierarchy)
                {
                    sailsUnfurled.gameObject.SetActive(true);
                    sailsFurled.SetActive(false);
                    
                }
                sailsUnfurled.SetBlendShapeWeight(0, 100 - value * 100);
                NetworkEntityManager.Instance.OnSailServerRpc(NetworkManager.Singleton.LocalClientId, value);
            }
        }
    }

    public void SetFurl(float value)
    {
        if (sailsFurled != null && sailsUnfurled != null)
        {
            if (value > furlTrigger)
            {
                if (sailsFurled.activeInHierarchy == false)
                {
                    sailsUnfurled.gameObject.SetActive(false);
                    sailsFurled.SetActive(true);
                }
            }
            else
            {
                if (sailsFurled.activeInHierarchy)
                {
                    sailsUnfurled.gameObject.SetActive(true);
                    sailsFurled.SetActive(false);

                }
                sailsUnfurled.SetBlendShapeWeight(0, 100 - value * 100);
            }
        }
    }

    public void FireLeft()
    {
        //anim.Play("FireLeft", 0 , 0);
        anim.CrossFade("FireLeft", 0.1f, 0, 0);

        //StartCoroutine(FireCannons(cannonFirePositionsLeft));
    }

    public void FireRight()
    {
        //anim.Play("FireRight", 0, 0);
        anim.CrossFade("FireRight", 0.1f, 0, 0);

        //StartCoroutine(FireCannons(cannonFirePositionsRight));
    }


    //IEnumerator FireCannons( RaycastCannon[] t)
    //{
    //    foreach (var cannon in t)
    //    {
    //        cannon.FireCannon();
    //        yield return new WaitForSeconds(0.3f);
    //    }
    //}

    public void IsSunk()
    {
        deathExplosionPosition = transform;

        if (deathExplosionPosition != null && shipDeathParticles != null)
        {
            Instantiate(shipDeathParticles, deathExplosionPosition);
        }

        if (waterDeathParticles != null)
        {
            Instantiate(waterDeathParticles, new Vector3(transform.position.x, transform.position.y + 0.01f, transform.position.z), transform.rotation);
        }


        anim.CrossFade("Sink", 0.05f, 0, 0);
    }

    public void IsRespawn()
    {

        if (mr != null)
        {
            StopAllCoroutines();

            mr.material.SetFloat("_FadeShift", 0);

            StartCoroutine(StartRespawn());
        }

        anim.Play("Spawn", 0, 0);
    }

    IEnumerator StartRespawn()
    {
        if (mr != null)
        {
            float startValue = 0f;
            float endValue = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < respawnDuration)
            {
                lerpValue = Mathf.Lerp(startValue, endValue, elapsedTime / respawnDuration);
                foreach (Renderer r in allRenderers)
                {
                    r.material.SetFloat("_FadeShift", lerpValue);
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Ensure the final value is exactly the end value
            lerpValue = endValue;
            foreach (Renderer r in allRenderers)
            {
                r.material.SetFloat("_FadeShift", lerpValue);
            }
        }
    }
}
