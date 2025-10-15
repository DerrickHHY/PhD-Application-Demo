using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIINPUT : MonoBehaviour
{
    [Header("UI Panel Setting")]
    public CanvasGroup panel1;
    // Start is called before the first frame update
    void Start()
    {
        if (panel1 != null)
        {
            SetPanelVisibility(true);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TogglePanel()
    {
        if (panel1 != null)
        {
            bool isvisuble = panel1.alpha > 0.5;
            SetPanelVisibility(!isvisuble);
        }
    }
    public void ShowPanel()
    {
        SetPanelVisibility(true);
    }

    public void HidePanel()
    {
        SetPanelVisibility(false);
    }

    private void SetPanelVisibility(bool isvisuble)
    {
        panel1.alpha = isvisuble ? 1f : 0f;
        panel1.interactable = isvisuble;
        panel1.blocksRaycasts = isvisuble;
    }

    public bool IsPanelVisible()
    {
        return panel1 != null && panel1.alpha > 0.5f;
    }
}
