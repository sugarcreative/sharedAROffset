using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ShipEffectController : MonoBehaviour
{
    private Animator anim;

    //[SerializeField] private TMP_Text text;

    public GameObject shipDeathParticles, waterDeathParticles;
    //public RaycastCannon[] cannonFirePositionsLeft, cannonFirePositionsRight;
    public Transform deathExplosionPosition;

    public SkinnedMeshRenderer sailsUnfurled;
    public GameObject sailsFurled;
    public float furlTrigger = 95;

    public float waterHeight;

    public float respawnDuration = 2.0f;
    private float lerpValue;

    public List<Renderer> allRenderers = new List<Renderer>();

    [SerializeField] private MeshRenderer[] newMeshList;

    [SerializeField] private SkinnedMeshRenderer[] newSkinList;

    [SerializeField] private Transform worldPosRef;

    [SerializeField] private float ajustedYCoord;

    private float boatHeight;

    void Awake()
    {
        boatHeight = worldPosRef.position.y;

    }

    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();

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

        newMeshList = GetComponentsInChildren<MeshRenderer>(true);
        newSkinList = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        allRenderers.AddRange(meshRenderers);

        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        allRenderers.AddRange(skinnedMeshRenderers);

        foreach (MeshRenderer mr in newMeshList)
        {
            mr.material.SetFloat("_YOffset", boatHeight);
        }

        foreach (SkinnedMeshRenderer mr in newSkinList)
        {
            mr.material.SetFloat("_Yoffset", boatHeight);
        }
    }

    private void OnEnable()
    {
        anim.Play("Spawn", 0, 0);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
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
        anim.CrossFade("FireLeft", 0.1f, 0, 0);
    }

    public void FireRight()
    {
        anim.CrossFade("FireRight", 0.1f, 0, 0);
    }


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

        StopAllCoroutines(); //this works

        foreach (MeshRenderer mr in newMeshList)
        {
            mr.material.SetFloat("_FadeShift", 0);

        }

        foreach (SkinnedMeshRenderer mr in newSkinList)
        {
            mr.material.SetFloat("_FadeShift", 0);
        }

        StartCoroutine(StartRespawn());


        anim.Play("Spawn", 0, 0); //this works
    }



    IEnumerator StartRespawn()
    {

        float startValue = 0f;
        float endValue = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < respawnDuration)
        {

            lerpValue = Mathf.Lerp(startValue, endValue, elapsedTime / respawnDuration);


            foreach (MeshRenderer mr in newMeshList)
            {
                mr.material.SetFloat("_FadeShift", lerpValue);
            }

            foreach (SkinnedMeshRenderer mr in newSkinList)
            {
                mr.material.SetFloat("_FadeShift", lerpValue);
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final value is exactly the end value
        lerpValue = endValue;

        foreach (MeshRenderer mr in newMeshList)
        {
            mr.material.SetFloat("_FadeShift", lerpValue);

        }

        foreach (SkinnedMeshRenderer mr in newSkinList)
        {
            mr.material.SetFloat("_FadeShift", lerpValue);

        }
    }
}
