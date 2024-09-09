using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CannonLogic : MonoBehaviour
{
    [SerializeField] private RaycastCannon[] newCannons;

    [SerializeField] private Button Button;

    [SerializeField] private ulong objectId;

    private void Awake()
    {
        newCannons = GetComponentsInChildren<RaycastCannon>(true);
        if (gameObject.name == "StarboardCannons")
        {
            objectId = 0;
        } else if (gameObject.name == "PortCannons")
        {
            objectId = 1;
        }
    }

    public void OnEnable()
    {
        Button.enabled = true;
    }

    public void ShootCannons()
    {
        StartCoroutine(FireCannons());
        NetworkEntityManager.Instance.OnShootServerRpc(NetworkManager.Singleton.LocalClientId, objectId);
    }

    [ContextMenu("printName")]
    private void printName()
    {
        Debug.Log(gameObject.name);
    }

    IEnumerator FireCannons()
    {
        Button.interactable = false;
        foreach (RaycastCannon cannon in newCannons)
        {
            cannon.FireCannon();
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(1f);
        Button.interactable = true;
    }
}
