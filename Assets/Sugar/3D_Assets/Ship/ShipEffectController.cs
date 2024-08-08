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

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void CheckFurl()
    {

    }

    public void FireLeft()
    {
        anim.Play("FireLeft", 0);
    }

    public void FireRight()
    {
        anim.Play("FireRight", 0);
    }

    public void IsSunk()
    {
        anim.Play("Sink", 0);
    }
}
