using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    private bool isToggleEnabled = true;
    private bool isPanelVisible = false;
    public System.Action<bool> OnPanelStateChanged;
    [Header("UI Panel Setting")]
    public CanvasGroup PanelInteract;

    void Start()
    {
        if (PanelInteract != null)
        {
            SetPanelVisibility(true);
            isPanelVisible = true; 
        }

        if (PanelInteract != null && PanelInteract.GetComponentInChildren<UnityEngine.UI.Button>() == null)
        {
            Debug.LogWarning("Interaction PanelInteract does not have a Button component! Please add one.");
        }

    }


    void Update()
    {

    }

    public void TogglePanel()
    {
        if (!isToggleEnabled)
        {
            Debug.LogWarning("TogglePanel function has been baned, avoiding duplicate planel click");

        }

        if (PanelInteract != null)
        {
            bool isVisible = PanelInteract.alpha > 0.5;
            SetPanelVisibility(!isVisible);

            isToggleEnabled = false;
            Debug.Log("TogglePanel function has been baned");


            OnPanelStateChanged?.Invoke(!isVisible);


        }


    }

    private void SetPanelVisibility(bool isVisible)
    {
        PanelInteract.alpha = isVisible ? 1f : 0f;
        PanelInteract.interactable = isVisible;
        PanelInteract.blocksRaycasts = isVisible;
        isPanelVisible = isVisible; 
    }
    

    
}
