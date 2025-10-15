using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<scr_model.GameEndEventArgs> OnGameEnded;
    [Header("Scene Timer Settings")]
    public float defaultSceneTime = 45f;  // 默认场景持续时间

    [Header("Quit Settings")]
    public bool quitOnTimerEnd = true;  // 是否在计时结束时退出
    public bool gameCompletedEarly = false;
    public float quiteDelayTime = 3f;
    private HashSet<string> completedTargets = new HashSet<string>();

    [Header("Target Object Settings")]
    [SerializeField] private List<string> targetObjectNames = new List<string>();  // 目标物体名称列表
    [SerializeField] private bool caseSensitiveMatch = false;  // 是否大小写敏感
    [SerializeField] private bool useCleanNameMatching = true;  // 是否使用清理后的名称匹配

    // [Header("Debug")]
    // [SerializeField] private bool showTargetDebug = true;
    private Coroutine sceneTimerCoroutine;
    private bool isTimerRunning = false;

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景持久化
            SubscribeToEvents();
            InitializeTargets();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void SubscribeToEvents()
    {
        scr_model.InteractionEvents.OnObjectInteractionPerformed += ObjectMatchingCheck;
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>



    /// <summary>
    /// 初始化目标物体配置
    /// </summary>
    private void InitializeTargets()
    {
        if (targetObjectNames.Count == 0)
        {
            Debug.LogWarning("GameManager: No target objects configured!");
        }
        // else if (showTargetDebug)
        // {
        //     Debug.Log($"GameManager: {targetObjectNames.Count} target objects configured: {string.Join(", ", targetObjectNames)}");
        // }
    }

    private void ObjectMatchingCheck(object sender, scr_model.ObjectInteractionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.objectName) || gameCompletedEarly)
            return;

        // 获取用于匹配的名称（原始或清理后）
        string matchName = useCleanNameMatching
            ? CleanObjectNameForMatching(e.objectName)
            : e.objectName;

        // 标准化匹配（大小写处理）
        string normalizedMatchName = caseSensitiveMatch ? matchName : matchName.ToLower();

        // 检查是否匹配任何目标
        foreach (string targetName in targetObjectNames)
        {
            string normalizedTarget = caseSensitiveMatch ? targetName : targetName.ToLower();

            if (normalizedMatchName == normalizedTarget && !completedTargets.Contains(normalizedTarget))
            {
                // 目标匹配！标记完成并提前结束游戏
                completedTargets.Add(normalizedTarget);

                // if (showTargetDebug) //日语翻译要显示的话
                // {
                //     Debug.Log($"GameManager: Target matched! '{e.objectName}' -> '{matchName}' (target: '{targetName}')");
                // }

                // 检查是否所有目标都完成
                if (completedTargets.Count == targetObjectNames.Count)
                {
                    Debug.Log("GameManager: All targets completed! Ending game early...");
                    CompleteGameEarly();
                }
                else
                {
                    Debug.Log($"GameManager: Target completed ({completedTargets.Count}/{targetObjectNames.Count}). Continue game.");
                }

                break;  // 只处理第一个匹配的目标
            }
        }
    }
    private void CompleteGameEarly()
    {
        gameCompletedEarly = true;

        // 停止计时器
        StopSceneTimer();

        // 触发胜利逻辑（可扩展）
        VictoryShowing();

        // 延迟后退出游戏
        StartCoroutine(DelayedQuit(true));  // 2秒后退出，给玩家时间看到胜利信息
    }    
    private string CleanObjectNameForMatching(string dirtyName)
    {
        if (string.IsNullOrEmpty(dirtyName))
            return dirtyName;
        
        
        // // 移除 Unity 克隆标号
        // string step1 = System.Text.RegularExpressions.Regex.Replace(dirtyName, @"\s*\([^)]*\)$", "").Trim();
        
        // // 下划线转空格
        // string step2 = step1.Replace("_", " ");
        
        // // 规范化空格
        // string step3 = System.Text.RegularExpressions.Regex.Replace(step2, @"\s+", " ").Trim();
        // string CleanName = step3;
        string CleanName = dirtyName;
        return CleanName;
    }
    /// <summary>
    /// 启动场景计时器
    /// </summary>
    public void StartSceneTimer(float duration = 0f)
    {
        if (isTimerRunning)
        {
            // Debug.LogWarning("Scene timer is already running, stopping previous timer");
            StopSceneTimer();
        }
        
        float actualDuration = duration > 0 ? duration : defaultSceneTime;
        sceneTimerCoroutine = StartCoroutine(SceneTimerCoroutine(actualDuration));
        isTimerRunning = true;

        Debug.Log($"GameManager: SceneTimer started, waiting for {actualDuration} seconds");
    }
    
    /// <summary>
    /// 停止场景计时器
    /// </summary>
    public void StopSceneTimer()
    {
        if (sceneTimerCoroutine != null)
        {
            StopCoroutine(sceneTimerCoroutine);
            sceneTimerCoroutine = null;
            isTimerRunning = false;
            Debug.Log("GameManager: SceneTimer stopped");
        }
    }
    
    /// <summary>
    /// 检查计时器是否运行中
    /// </summary>
    public bool IsSceneTimerRunning()
    {
        return isTimerRunning;
    }
    
    /// <summary>
    /// 获取剩余时间（需要额外实现）
    /// </summary>
    public float GetRemainingTime()
    {
        // 可以通过协程的进度计算，这里简化实现
        return 0f;
    }

    /// <summary>
    /// 场景计时器协程
    /// </summary>
    private IEnumerator SceneTimerCoroutine(float duration)
    {
        if (gameCompletedEarly)
        {
            yield break;  // 提前结束，不执行计时
        }
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;  // 使用不受时间缩放影响的时间
            float remainingTime = duration - elapsedTime;

            // 可选：每秒广播剩余时间（供 UI 使用）
            // OnTimerTick?.Invoke(remainingTime);

            yield return null;
        }

        // 计时结束
        Debug.Log("GameManager: SceneTimer finished after " + duration + " seconds");

        if (quitOnTimerEnd && !gameCompletedEarly)
        {
            Debug.Log("GameManager: SceneTimer finished without successfual seraching");
            FailShowing();
            StartCoroutine(DelayedQuit(false));// 可以扩展为一个failed showing 对应于 vicotory showing
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    private IEnumerator DelayedQuit(bool seachResult)
    {
        var eventArgs = new scr_model.GameEndEventArgs(seachResult);
        OnGameEnded?.Invoke(this, eventArgs);
        yield return new WaitForSecondsRealtime(quiteDelayTime);
        QuitGame();
    }
    public void QuitGame()
    {
        Debug.Log("GameManager: Quitting game");

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }
    
    private void VictoryShowing()
    {
        // Debug.LogWarning("成功找到了打印机！！！！！！！！！！！！！！！！！！！");
    }
    private void FailShowing()
    {
        // Debug.LogWarning("没能找到打印机。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
    }


    /// <summary>
    /// 暂停计时器
    /// </summary>
    // public void PauseSceneTimer()
    // {
    //     if (sceneTimerCoroutine != null)
    //     {
    //         StopCoroutine(sceneTimerCoroutine);
    //         sceneTimerCoroutine = null;
    //         isTimerRunning = false;
    //         Debug.Log("GameManager: SceneTimer paused");
    //     }
    // }

    /// <summary>
    /// 恢复计时器（需要记录暂停时间，简化实现）
    /// </summary>
    // public void ResumeSceneTimer(float remainingTime)
    // {
    //     if (!isTimerRunning)
    //     {
    //         sceneTimerCoroutine = StartCoroutine(SceneTimerCoroutine(remainingTime));
    //         isTimerRunning = true;
    //         Debug.Log("GameManager: SceneTimer resumed with " + remainingTime + " seconds remaining");
    //     }
    // }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void UnsubscribeFromEvents()
    {
        scr_model.InteractionEvents.OnObjectInteractionPerformed -= ObjectMatchingCheck;
    }
}
