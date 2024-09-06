using UnityEngine;

public class WheelSwitcher : MonoBehaviour
{

    [SerializeField] private GameObject[] wheels;
    
    public void SwitchWheel(int index)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (i == index)
            {
                wheels[i].SetActive(true);
            }
            else
            {
                wheels[i].SetActive(false);
            }
        }
    }
}
