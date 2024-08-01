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
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, _cannonRange, _hitLayers);

        foreach (RaycastHit hit in hits)
        {
            NetworkObject networkObj = hit.collider.GetComponent<NetworkObject>();
            if (networkObj != null && networkObj.OwnerClientId != OwnerClientId)
            {
                ulong networkObjId = networkObj.OwnerClientId;
                combatLog.text = $"{networkObjId} has been hit!";
                //SendHitMessageServerRpc(networkObjId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendHitMessageServerRpc(ulong ownerClientId)
    {
        UpdateCombatLogOnClientRpc(ownerClientId);
    }

    [ClientRpc]
    void UpdateCombatLogOnClientRpc(ulong ownerClientId)
    {
        combatLog.text = $"{ownerClientId} has been hit!";
    }
}
