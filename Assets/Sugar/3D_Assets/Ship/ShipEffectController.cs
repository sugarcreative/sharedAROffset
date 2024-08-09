using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShipEffectController : MonoBehaviour
{
    private Animator anim;

    public GameObject fireParticles;
    public Transform[] cannonFirePositionsLeft, cannonFirePositionsRight;

    public SkinnedMeshRenderer sailsUnfurled;
    public GameObject sailsFurled;
    public float furlTrigger = 95;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        if (sailsFurled != null)
        {
            sailsFurled.SetActive(false);
        }
    }


    public void CheckFurl(float value)
    {
        if (sailsFurled != null)
        {
            if (value > furlTrigger)
            {
                if (sailsFurled.activeInHierarchy == false)
                {
                    //turn off sailsunfurl
                    sailsFurled.SetActive(true);
                }
            }
            else
            {
                if (sailsFurled.activeInHierarchy)
                {
                    //turn on sailsunfurl
                    sailsFurled.SetActive(false);
                }
            }
        }
    }

    public void FireLeft()
    {
        // insert particle spawning
        anim.Play("FireLeft", 0);
    }

    public void FireRight()
    {
        // insert particle spawning
        anim.Play("FireRight", 0);
    }

    public void IsSunk()
    {
        // insert particle spawning
        anim.Play("Sink", 0);
    }
}
