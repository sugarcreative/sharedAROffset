using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RaycastCannon : MonoBehaviour
{
    [SerializeField] private float _cannonRange;

    public LayerMask _hitLayers;

    [SerializeField] private TMP_Text combatLog;

    [SerializeField] private GameObject _hitParticles;

    [SerializeField] private GameObject _shootParticles;

    public void FireCannon()
    {
        SpawnLocalShootingParticles(_shootParticles);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, _cannonRange, _hitLayers))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NetworkPlayer"))
            {
                SpawnLocalHitParticles(_hitParticles, hit.point, hit.normal);
                ulong targetId = hit.collider.GetComponent<NetworkObject>().OwnerClientId;
                NetworkEntityManager.Instance.ReduceHealthServerRpc(NetworkManager.Singleton.LocalClientId, targetId);
            }
        }
        else
        {
            if (combatLog != null)
            {
                combatLog.text = $"{NetworkManager.Singleton.LocalClientId} has hit nothing with {gameObject.name}!";
            }
        }
    }

    void SpawnLocalHitParticles(GameObject particles, Vector3 position, Vector3 rotation)
    {
        if (particles == null) return;
        
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(particles, position, rotationQ);
    }


    private void SpawnLocalShootingParticles(GameObject shootParticles)
    {
        GameObject instantiatedParticles = Instantiate(shootParticles, transform.position, transform.rotation);
    }
}
