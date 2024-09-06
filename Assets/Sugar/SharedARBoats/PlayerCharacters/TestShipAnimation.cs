using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShipAnimation : MonoBehaviour
{
    public GameObject ship;

    public void SinkShip()
    {
        ship.GetComponent<ShipEffectController>().IsRespawn();
    }
}
