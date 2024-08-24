using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroLogic : MonoBehaviour
{
    [SerializeField] private List<ModalFade> tabs;

    [SerializeField] private int currentTab = 0;

    [SerializeField] private Pip[] pips;

    [SerializeField] private Button nextButton, prevButton;

    void Start()
    {
        tabs[currentTab].Show();
        pips[currentTab].selected.enabled = true;
    }

    public void NextTabButton()
    {
        tabs[currentTab].Hide();
        pips[currentTab].selected.enabled = false;
        currentTab++;
        tabs[currentTab].Show();
        pips[currentTab].selected.enabled = true;
        if (currentTab == tabs.Count - 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
        if (currentTab == 0)
        {
            prevButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
        }

    }

    public void PrevTabButton()
    {
        tabs[currentTab].Hide();
        pips[currentTab].selected.enabled = false;
        currentTab--;
        tabs[currentTab].Show();
        pips[currentTab].selected.enabled = true;
        if (currentTab == tabs.Count - 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
        if (currentTab == 0)
        {
            prevButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
        }
    }
}
