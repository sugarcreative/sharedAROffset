using System.Collections;
using TMPro;
using UnityEngine;

public class CannonLogic : MonoBehaviour
{
    [SerializeField] private RaycastCannon[] newCannons;

    //private void Awake()
    //{
    //    newCannons = GetComponentsInChildren<RaycastCannon>(true);
    //}

    //private void Start()
    //{
    //    newCannons = GetComponentsInChildren<RaycastCannon>(true);
    //}

    public void ShootCannons()
    {
        newCannons = GetComponentsInChildren<RaycastCannon>(true);
        StartCoroutine(FireCannons());
    }

    IEnumerator FireCannons()
    {
        foreach (RaycastCannon cannon in newCannons)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
    }
}
