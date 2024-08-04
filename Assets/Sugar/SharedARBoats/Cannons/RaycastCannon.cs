using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RaycastCannon : NetworkBehaviour
{
    [SerializeField] private float _cannonRange;

    //[SerializeField] private CannonLogic _cannonLogic;

    public LayerMask _hitLayers;

    public TMP_Text combatLog;

    [SerializeField] private GameObject _hitParticles;

    private void Start()
    {
        combatLog = FindObjectOfType<CombatLog>().GetComponent<TMP_Text>();
    }

    public void FireCannon()
    {

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, _cannonRange, _hitLayers))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NetworkPlayer"))
            {
                SpawnParticles(_hitParticles, hit.point, hit.normal);
                ulong targetId = hit.collider.GetComponent<NetworkObject>().OwnerClientId;
                combatLog.text = $"you have hit {targetId}!";
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { targetId },
                    }
                };
                SendHitMessageServerRpc(NetworkManager.Singleton.LocalClientId, targetId);
            }

            //ulong targetId = hit.collider.GetComponent<NetworkObject>().OwnerClientId;
            //combatLog.text = $"you have hit {targetId}!";
            //ClientRpcParams clientRpcParams = new ClientRpcParams
            //{
            //    Send = new ClientRpcSendParams
            //    {
            //        TargetClientIds = new ulong[] { targetId },
            //    }
            //};
            //SendHitMessageServerRpc(NetworkManager.Singleton.LocalClientId, targetId);
        }
        else
        {
            combatLog.text = $"{NetworkManager.Singleton.LocalClientId} has hit nothing!";
        }
    }

    void SpawnParticles(GameObject particles, Vector3 position, Vector3 rotation)
    {
        if (particles == null) return;
        
        Quaternion rotationQ = Quaternion.Euler(rotation);
        GameObject instantiatedParticles = Instantiate(particles, position, rotationQ);
        Destroy(instantiatedParticles.gameObject, 3f);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendHitMessageServerRpc(ulong shooterId, ulong targetId)
    {
        UpdateCombatLogOnClientRpc(shooterId, targetId);
    }

    [ClientRpc]
    void UpdateCombatLogOnClientRpc(ulong shooterId, ulong targetId)
    {
        if (targetId != NetworkManager.Singleton.LocalClientId) return;

        combatLog.text = $"You have been shot by {shooterId}!";
    }
}
