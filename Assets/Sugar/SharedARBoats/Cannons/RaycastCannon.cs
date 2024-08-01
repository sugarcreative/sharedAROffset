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
        int num = hits.Length;
        SendHitMessageServerRpc(num);
        //combatLog.text = hits.Length.ToString();

       

        foreach (RaycastHit hit in hits)
        {
            NetworkObject networkObj = hit.collider.GetComponent<NetworkObject>();
            if (networkObj != null && networkObj.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                ulong networkObjId = networkObj.OwnerClientId;
                //combatLog.text = $"{networkObjId} has been hit!";
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendHitMessageServerRpc(int num)
    {
        UpdateCombatLogOnClientRpc(num);
    }

    [ClientRpc]
    void UpdateCombatLogOnClientRpc(int num)
    {
        combatLog.text = $"{num} hits detected by client {NetworkManager.Singleton.LocalClientId}";
    }
}
