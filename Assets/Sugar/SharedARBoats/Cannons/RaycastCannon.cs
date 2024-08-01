using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RaycastCannon : NetworkBehaviour
{
    [SerializeField] private float _cannonRange;

    //[SerializeField] private CannonLogic _cannonLogic;

    public LayerMask _hitLayers;

    public TMP_Text combatLog;

    private void Start()
    {
        combatLog = FindObjectOfType<CombatLog>().GetComponent<TMP_Text>();
    }

    public void FireCannon()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _cannonRange, _hitLayers))
        {
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
            //combatLog.text = $"{NetworkManager.Singleton.LocalClientId} has hit {hit.collider.GetComponent<NetworkObject>().OwnerClientId}";

        }
        else
        {
            combatLog.text = $"{NetworkManager.Singleton.LocalClientId} has hit nothing!";
        }
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
