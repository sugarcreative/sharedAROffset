using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCard : MonoBehaviour
{

    [SerializeField] private TMP_Text playerName;

    [SerializeField] public Image colorIcon;

    [SerializeField] private GameObject colorPalette;

    [SerializeField] private Button[] colorsToChoseFrom;

    public void Initialize(FixedString64Bytes name)
    {
        playerName.text = name.ToString();
    }
}
