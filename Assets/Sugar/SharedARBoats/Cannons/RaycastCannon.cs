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

    [SerializeField] private GameObject _missParticles;

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
            Vector3 missPoint = transform.position + transform.forward * _cannonRange;

            RaycastHit groundHit;

            if (Physics.Raycast(missPoint, Vector3.down, out groundHit, Mathf.Infinity))
            {
                SpawnLocalMissParticles(_missParticles, groundHit.point, groundHit.normal);
            }
        }
    }

    void SpawnLocalHitParticles(GameObject particles, Vector3 position, Vector3 rotation)
    {
        if (particles == null) return;
        
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(particles, position, rotationQ);
    }

    void SpawnLocalMissParticles(GameObject particles, Vector3 rayCast, Vector3 rotation)
    {
        if (particles == null) return;
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(particles, rayCast, rotationQ);
    }

    private void SpawnLocalShootingParticles(GameObject shootParticles)
    {
        GameObject instantiatedParticles = Instantiate(shootParticles, transform.position, transform.rotation);
    }
}
