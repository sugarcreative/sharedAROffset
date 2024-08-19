using System.Collections;
using TMPro;
using UnityEngine;

public class CannonLogic : MonoBehaviour
{

    [SerializeField] private RaycastCannon[] cannons;

    [SerializeField] private RaycastCannon[] newCannons;

    private void Awake()
    {
        newCannons = GetComponentsInChildren<RaycastCannon>(true);
    }

    public void ShootCannons()
    {
        StartCoroutine(FireCannons());
    }

    IEnumerator FireCannons()
    {
        foreach (var cannon in newCannons)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
    }
}
