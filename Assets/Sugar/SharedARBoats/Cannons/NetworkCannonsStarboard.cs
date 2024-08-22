using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCannonsStarboard : MonoBehaviour
{
    [SerializeField] private RaycastCannon[] newCannons;
    [SerializeField] private GameObject shootingParticles;

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
            Instantiate(shootingParticles, cannon.gameObject.transform.position, cannon.gameObject.transform.rotation);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
