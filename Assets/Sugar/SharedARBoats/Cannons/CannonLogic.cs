using System.Collections;
using UnityEngine;

public class CannonLogic : MonoBehaviour
{

    [SerializeField] private RaycastCannon[] cannons;

    public void ShootCannons()
    {
        StartCoroutine(FireCannons());
    }

    IEnumerator FireCannons()
    {
        foreach (var cannon in cannons)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
    }
}
