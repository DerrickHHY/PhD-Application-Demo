using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;
using TMPro;

public class BroadcastControl : MonoBehaviour
{
    private Coroutine hideCoroutine;
    public float broadcastHideDelay = 1.5f;

    [Header("UI Elements")]
    public CanvasGroup broadcastPanel;
    public TextMeshProUGUI objectNameText;
    private void Start()
    {
        // SetPanelVisibility(false);
        NullText();
    }
    private void OnEnable()
    {
        scr_model.InteractionEvents.OnObjectInteractionPerformed += OnObjectDetection;
    }
    
    private void OnDisable()
    {
        scr_model.InteractionEvents.OnObjectInteractionPerformed -= OnObjectDetection;
        StopHideCoroutine();
    }

    private void OnObjectDetection(object sender, scr_model.ObjectInteractionEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.objectName))
        {
            SetPanelVisibility(true);
            string cleanName = CleanObjectName(e.objectName);
            objectNameText.text = cleanName;

            Debug.Log($"UI: Showing {e.objectName}");
            hideCoroutine = StartCoroutine(HideAfterDelay());

            
        }

    }
    private string CleanObjectName(string dirtyName)
    {

        if (string.IsNullOrEmpty(dirtyName))
            return dirtyName;

        string pattern = @"\s*\([^)]*\)$";
        string step1 = Regex.Replace(dirtyName, pattern, "").Trim();

        step1 = step1.Trim();

        string step2 = step1.Replace("_", " ");

        string cleanName = ToTitleCase(step2);

        return string.IsNullOrEmpty(cleanName) ? dirtyName : cleanName;
    }
    private string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    private void SetPanelVisibility(bool isVisible)
    {
        broadcastPanel.alpha = isVisible ? 1f : 0f;
    }
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(broadcastHideDelay);

        // SetPanelVisibility(false);
        NullText();
        hideCoroutine = null;
    }
    private void NullText()
    {
        string nulltext = "...";
        objectNameText.text = nulltext;
    }
    private void StopHideCoroutine()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
}
