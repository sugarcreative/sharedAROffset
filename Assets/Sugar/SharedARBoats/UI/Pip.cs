using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pip : MonoBehaviour
{

    public Image selected;

    void Awake()
    {
        selected.enabled = false;
    }
}
