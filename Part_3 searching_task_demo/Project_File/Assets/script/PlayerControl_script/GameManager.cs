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
    public float defaultSceneTime = 45f; 

    [Header("Quit Settings")]
    public bool quitOnTimerEnd = true; 
    public bool gameCompletedEarly = false;
    public float quiteDelayTime = 3f;
    private HashSet<string> completedTargets = new HashSet<string>();

    [Header("Target Object Settings")]
    [SerializeField] private List<string> targetObjectNames = new List<string>();  
    [SerializeField] private bool caseSensitiveMatch = false;  
    [SerializeField] private bool useCleanNameMatching = true;  


    private Coroutine sceneTimerCoroutine;
    private bool isTimerRunning = false;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
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
    



    private void InitializeTargets()
    {
        if (targetObjectNames.Count == 0)
        {
            Debug.LogWarning("GameManager: No target objects configured!");
        }

    }

    private void ObjectMatchingCheck(object sender, scr_model.ObjectInteractionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.objectName) || gameCompletedEarly)
            return;


        string matchName = useCleanNameMatching
            ? CleanObjectNameForMatching(e.objectName)
            : e.objectName;


        string normalizedMatchName = caseSensitiveMatch ? matchName : matchName.ToLower();


        foreach (string targetName in targetObjectNames)
        {
            string normalizedTarget = caseSensitiveMatch ? targetName : targetName.ToLower();

            if (normalizedMatchName == normalizedTarget && !completedTargets.Contains(normalizedTarget))
            {

                completedTargets.Add(normalizedTarget);

                if (completedTargets.Count == targetObjectNames.Count)
                {
                    Debug.Log("GameManager: All targets completed! Ending game early...");
                    CompleteGameEarly();
                }
                else
                {
                    Debug.Log($"GameManager: Target completed ({completedTargets.Count}/{targetObjectNames.Count}). Continue game.");
                }

                break;  
            }
        }
    }
    private void CompleteGameEarly()
    {
        gameCompletedEarly = true;

        StopSceneTimer();

        VictoryShowing();

        StartCoroutine(DelayedQuit(true));  
    }    
    private string CleanObjectNameForMatching(string dirtyName)
    {
        if (string.IsNullOrEmpty(dirtyName))
            return dirtyName;
        
        string CleanName = dirtyName;
        return CleanName;
    }

    public void StartSceneTimer(float duration = 0f)
    {
        if (isTimerRunning)
        {

            StopSceneTimer();
        }
        
        float actualDuration = duration > 0 ? duration : defaultSceneTime;
        sceneTimerCoroutine = StartCoroutine(SceneTimerCoroutine(actualDuration));
        isTimerRunning = true;

        Debug.Log($"GameManager: SceneTimer started, waiting for {actualDuration} seconds");
    }
    

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
    

    public bool IsSceneTimerRunning()
    {
        return isTimerRunning;
    }
    

    public float GetRemainingTime()
    {

        return 0f;
    }


    private IEnumerator SceneTimerCoroutine(float duration)
    {
        if (gameCompletedEarly)
        {
            yield break; 
        }
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;  
            float remainingTime = duration - elapsedTime;



            yield return null;
        }


        Debug.Log("GameManager: SceneTimer finished after " + duration + " seconds");

        if (quitOnTimerEnd && !gameCompletedEarly)
        {
            Debug.Log("GameManager: SceneTimer finished without successfual seraching");
            FailShowing();
            StartCoroutine(DelayedQuit(false));
        }
    }

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
        // Debug.LogWarning("yor find it ！！！！！！！！！！！！！！！！！！！");
    }
    private void FailShowing()
    {
        // Debug.LogWarning("you dont find it 。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
    }


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
