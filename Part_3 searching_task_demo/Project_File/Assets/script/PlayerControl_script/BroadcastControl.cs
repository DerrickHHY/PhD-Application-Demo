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
            // 显示 UI
            SetPanelVisibility(true);
            string cleanName = CleanObjectName(e.objectName);
            objectNameText.text = cleanName;

            Debug.Log($"UI: Showing {e.objectName}");
            hideCoroutine = StartCoroutine(HideAfterDelay());

            
        }
        // else
        // {
        //     // 隐藏 UI
        //     SetPanelVisibility(false);
        //     Debug.Log("UI: Hidden");
        // }
    }
    private string CleanObjectName(string dirtyName)
    {

        if (string.IsNullOrEmpty(dirtyName))
            return dirtyName;

        string pattern = @"\s*\([^)]*\)$";
        string step1 = Regex.Replace(dirtyName, pattern, "").Trim();
        // 去除多余的前后空格
        step1 = step1.Trim();
        // 第2步：替换下划线为空格
        string step2 = step1.Replace("_", " ");

        // 第3步：每个单词首字母大写
        string cleanName = ToTitleCase(step2);

        // 如果清理后为空，返回原始名称（防止误删）
        return string.IsNullOrEmpty(cleanName) ? dirtyName : cleanName;
    }
    private string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 使用 CultureInfo 创建 TextInfo，支持本地化标题格式
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    private void SetPanelVisibility(bool isVisible)
    {
        broadcastPanel.alpha = isVisible ? 1f : 0f;
    }
    private IEnumerator HideAfterDelay()
    {
        // 等待延迟时间
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
