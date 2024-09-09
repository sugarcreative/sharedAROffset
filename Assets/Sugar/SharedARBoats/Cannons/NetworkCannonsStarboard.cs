using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCannonsStarboard : MonoBehaviour
{
    [SerializeField] private NetworkRaycastCannon[] newCannons;
    [SerializeField] private GameObject shootingParticles;

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
            //Instantiate(shootingParticles, cannon.gameObject.transform.position, cannon.gameObject.transform.rotation);
            cannon.FireCannon();
            AudioManager.Instance.PlaySound("networkCannonFire");
            yield return new WaitForSeconds(0.3f);
        }
    }
}
