using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RaycastCannon : NetworkBehaviour
{
    [SerializeField] private float _cannonRange;

    public LayerMask _hitLayers;

    [SerializeField] private TMP_Text combatLog;

    [SerializeField] private GameObject _hitParticles;

    [SerializeField] private GameObject _shootParticles;


    public void FireCannon()
    {
        SpawnLocalShootingParticles(_shootParticles, transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, _cannonRange, _hitLayers))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NetworkPlayer"))
            {
                SpawnLocalHitParticles(_hitParticles, hit.point, hit.normal);
                ulong targetId = hit.collider.GetComponent<NetworkObject>().OwnerClientId;
                //combatLog.text = $"you have hit {targetId}!";
                NetworkEntityManager.Instance.ReduceHealthServerRpc(NetworkManager.Singleton.LocalClientId, targetId);
                //SendHitMessageServerRpc(NetworkManager.Singleton.LocalClientId, targetId);
            }
        }
        else
        {
            combatLog.text = $"{NetworkManager.Singleton.LocalClientId} has hit nothing with {gameObject.name}!";
        }
    }

    void SpawnLocalHitParticles(GameObject particles, Vector3 position, Vector3 rotation)
    {
        if (particles == null) return;
        
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(particles, position, rotationQ);
    }


    private void SpawnLocalShootingParticles(GameObject shootParticles, Vector3 position, Vector3 rotation)
    {
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(shootParticles, position, rotationQ);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendHitMessageServerRpc(ulong shooterId, ulong targetId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetId },
            }
        };

        UpdateCombatLogOnClientRpc(shooterId, clientRpcParams);
    }

    [ClientRpc]
    void UpdateCombatLogOnClientRpc(ulong shooterId, ClientRpcParams clientRpcParams)
    {
        combatLog.text = $"You have been shot by {shooterId}!";
    }
}
