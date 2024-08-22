using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CannonLogic : MonoBehaviour
{
    [SerializeField] private RaycastCannon[] newCannons;

    [SerializeField] private Button Button;

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
        Button.enabled = false;
        foreach (RaycastCannon cannon in newCannons)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(2f);
        Button.enabled = true;
    }
}
