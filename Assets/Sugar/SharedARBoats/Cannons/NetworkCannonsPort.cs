using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCannonsPort : MonoBehaviour
{
    [SerializeField] private NetworkRaycastCannon[] newCannons;
    [SerializeField] private GameObject shootinParticles;

    private void Awake()
    {
        newCannons = GetComponentsInChildren<NetworkRaycastCannon>(true);
    }

    public void ShootCannons()
    {
        StartCoroutine(FireCannons());
    }

    IEnumerator FireCannons()
    {
        foreach (NetworkRaycastCannon cannon in newCannons)
        {
            //Instantiate(shootinParticles, cannon.gameObject.transform.position, cannon.gameObject.transform.rotation);
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
    }
}
