using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BannerInstructionsManager : MonoBehaviour
{

    public Sprite basePip, currentPip;
    public Transform pipParent;
    public List<Image> allPips = new List<Image>();

    public Button PreviousButton, NextButton;
    public Color32 DisabledButtonColour;

    public Transform InstructionTabParent;
    public List<ModalFade> InstructionTabs = new List<ModalFade>();

    public int tabIndex = 0;

    private void Awake() {
        allPips = pipParent.GetComponentsInChildren<Image>().ToList();
        for (int i = 0; i < allPips.Count; i++) {
            allPips[i].sprite = basePip;
        }
        allPips[0].sprite = currentPip;

        PreviousButton.GetComponent<Image>().color = DisabledButtonColour;
        PreviousButton.interactable = false;

        foreach (Transform child in InstructionTabParent) {
            InstructionTabs.Add(child.gameObject.GetComponent<ModalFade>());
        }

        for (int i = 0; i < InstructionTabs.Count; i++) {
            //InstructionTabs[i].ForceOff();
        }
        InstructionTabs[0].ForceOn();
        tabIndex = 0;
    }

    public void PreviousPressed() {
        if (tabIndex == 0) {
            return;
        }

        NextButton.GetComponent<Image>().color = Color.white;
        NextButton.interactable = true;

        InstructionTabs[tabIndex].Hide();
        allPips[tabIndex].sprite = basePip;
        tabIndex--;
        allPips[tabIndex].sprite = currentPip;
        InstructionTabs[tabIndex].Show();

        if(tabIndex == 0) {
            PreviousButton.GetComponent<Image>().color = DisabledButtonColour;
            PreviousButton.interactable = false;
        }

    }

    public void NextPressed() {
        if (tabIndex == InstructionTabs.Count - 1) {
            return;
        }

        PreviousButton.GetComponent<Image>().color = Color.white;
        PreviousButton.interactable = true;

        InstructionTabs[tabIndex].Hide();
        allPips[tabIndex].sprite = basePip;
        tabIndex++;
        allPips[tabIndex].sprite = currentPip;
        InstructionTabs[tabIndex].Show();

        if (tabIndex == InstructionTabs.Count - 1) {
            NextButton.GetComponent<Image>().color = DisabledButtonColour;
            NextButton.interactable = false;
        }
    }
}
