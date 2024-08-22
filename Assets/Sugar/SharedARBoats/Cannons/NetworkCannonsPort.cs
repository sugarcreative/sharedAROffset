using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCannonsPort : MonoBehaviour
{
    [SerializeField] private RaycastCannon[] newCannons;
    [SerializeField] private GameObject shootinParticles;

    private void Awake()
    {
        //newCannons = GetComponentsInChildren<RaycastCannon>(true);
    }

    public void ShootCannons()
    {
        StartCoroutine(FireCannons());
    }

    IEnumerator FireCannons()
    {
        foreach (RaycastCannon cannon in newCannons)
        {
            Instantiate(shootinParticles, cannon.gameObject.transform.position, cannon.gameObject.transform.rotation);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
