using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{

    [SerializeField] private ModalFade _fade;

    private int _numOfCollisions;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_numOfCollisions != 0) {
            _fade.Show();
        }
        else {
            _fade.Hide();
        }
        Debug.Log(_numOfCollisions);
    }


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("NetworkPlayer")) {
            
            _numOfCollisions++;
            Debug.Log($"OnTriggerEnter: {gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("NetworkPlayer")) {
            
            _numOfCollisions--;
            Debug.Log($"OnTriggerExit: {gameObject.name}");
        }
    }

    
}
