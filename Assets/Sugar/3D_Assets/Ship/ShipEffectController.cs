using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShipEffectController : MonoBehaviour
{
    private Animator anim;

    public GameObject shipDeathParticles, waterDeathParticles;
    public RaycastCannon[] cannonFirePositionsLeft, cannonFirePositionsRight;
    public Transform deathExplosionPosition;

    public SkinnedMeshRenderer sailsUnfurled;
    public GameObject sailsFurled;
    public float furlTrigger = 95;

    public float waterHeight;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        if (sailsFurled != null && sailsUnfurled != null)
        {
            sailsUnfurled.gameObject.SetActive(false);
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

            Debug.Log(a + " is the value");
        }
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
            }
        }
    }

    public void FireLeft()
    {
        anim.Play("FireLeft", 0 , 0);
        StartCoroutine(FireCannons(cannonFirePositionsLeft));

    }

    public void FireRight()
    {
        anim.Play("FireRight", 0, 0);
        StartCoroutine(FireCannons(cannonFirePositionsRight));

    }


    IEnumerator FireCannons( RaycastCannon[] t)
    {
        foreach (var cannon in t)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void IsSunk()
    {
        if (deathExplosionPosition != null && shipDeathParticles != null)
        {
            Instantiate(shipDeathParticles, deathExplosionPosition);
        }

        if (waterDeathParticles != null)
        {
            Instantiate(waterDeathParticles, new Vector3(transform.position.x, waterHeight + 0.01f, transform.position.z), transform.rotation);
        }


        anim.Play("Sink", 0, 0);
    }
}
