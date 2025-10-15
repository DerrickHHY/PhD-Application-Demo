using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    public event EventHandler OnInteractAction; // 这部分也就是为了让其他代码来订阅的event， player里面的订阅的就是这个（例如gameInput.OnInteractAction +=）
    public event EventHandler<MouseInteractEventArgs> OnMouseInteractAction; //<>内的内容是通过PlayerAnimator.cs额外设置的一个事件信息传递方式
    private PlayerInputActions playerInputActions; //激活input system 相关代码的 step1 
    private bool waitingForFirstPanelClick = true;
    private bool isInputBlocked = true;
    [Header("UI Panels")]
    public GameObject fixationPanel; // 第一个UI面板
    public GameObject searchCuePanel; // 第二个UI面板
    public GameObject startingPanel; // 第二个UI面板

    public GameObject VictoryPanel;
    public GameObject FailePanel;

    [Header("UI Control Reference")]
    public UIControl uiControl;

    [Header("UI Timing")]
    public float introDuration = 10f; // 前5秒UI轮流时间
    public float panelDisplayTime = 2.5f; // 每个面板显示时间（可调整）
    private float searchTime = 45f;
    [SerializeField] private GameManager gameManager;

    private void Awake()                               //激活input system 相关代码的 step2 
    {
        playerInputActions = new PlayerInputActions(); //激活input system 相关代码的 step2 
        playerInputActions.Player.Enable(); //激活input system 相关代码的 step3 启用对应的控制组件 可视化界面的中的第一列 action map 部分playerInputActions.Player.Disable() 就可以禁用
        playerInputActions.Player.Interact.performed += Interact_performed;//左边的interact的案件激活的话， 右侧的订阅者活动就会激活
        playerInputActions.Player.MouseClick.performed += ClickInteraction; //点击左键后开始传输信息
        gameManager.OnGameEnded += onGameEnded;

        if (fixationPanel != null) fixationPanel.SetActive(false);
        if (searchCuePanel != null) searchCuePanel.SetActive(false);
        if (startingPanel != null) startingPanel.SetActive(false);

        if (VictoryPanel != null) VictoryPanel.SetActive(false);
        if (FailePanel != null) FailePanel.SetActive(false);
    }
    private void Start()
    {
        if (fixationPanel != null)
        {
            fixationPanel.SetActive(true);
            waitingForFirstPanelClick = true;
        }
        else
        {
            Debug.LogError("fixationPanel is not assigned!");
        }
        
        if (uiControl != null)
        {
            uiControl.OnPanelStateChanged += OnPanelStateChanged;
            // 根据初始面板状态设置游戏状态
        }
        else
        {
            Debug.LogError("UIControl reference is missing in GameInput!");
        }
    }

    private void OnPanelStateChanged(bool obj)
    {
       
        if (waitingForFirstPanelClick)
        {
            waitingForFirstPanelClick = false;
            // if (fixationPanel != null) fixationPanel.SetActive(false);

            // 启动后续 UI 切换协程（从点击开始）
            StartCoroutine(UIControler());

        }
        ;
    }

    private void OnDestroy()
    {
        // 清理事件订阅 // 这部分的逻辑是什么 原本只有click interaction, 后续根据gork的建议有吧其他的订阅加购上了

        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.MouseClick.performed -= ClickInteraction;
        uiControl.OnPanelStateChanged -= OnPanelStateChanged;
        gameManager.OnGameEnded -= onGameEnded;
        playerInputActions.Dispose();
    }



    private void ClickInteraction(InputAction.CallbackContext context)
    {
        if (!isInputBlocked)
        {
            Vector2 screenPosition = playerInputActions.Player.MouseClick.ReadValue<Vector2>();
            OnMouseInteractAction?.Invoke(this, new MouseInteractEventArgs
            {
                ScreenPosition = screenPosition
            });
        }
    }

    private void Interact_performed(InputAction.CallbackContext obj) //订阅者的激活是什么呢？如下//另外在gork中的建议这里不是obj是context, 这两个有什么区别？？
    {
        if (!isInputBlocked)
        {
            OnInteractAction?.Invoke(this, EventArgs.Empty);
            //为了方便理解 可以看以下代码，是等效的
            // if (OnInteractAction != null)//这个是在检测是否有其他的script订阅了这个代码，而不是在赋值，比喻为：名为OnInteractAction的电视台是否有观众
            // {
            //     OnInteractAction(this, EventArgs.Empty);
            //                     //发送者是this, 这个发送者传递的是空的参数
            // }
        }

    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (isInputBlocked)
        {
            return Vector2.zero; // 阻止输入时返回零向量
        }
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
    public Vector2 GetViewVectorNormalized() //视角移动 https://www.youtube.com/watch?v=uPlXdMKM5pc
    {

        //newCameraRotation.x += playerCameraSetting.ViewYSensitivity * rawViewInput.y * Time.deltaTime;
        // newCameraRotation = cameraholder.localRotation.eulerAngles;
        // cameraholder.localRotation = Quaternion.Euler(newCameraRotation);
        if (isInputBlocked)
        {
            return Vector2.zero; // 阻止输入时返回零向量
        }
        //Vector2 rawViewInput = playerInputActions.Player.View.ReadValue<Vector2>();
        //return rawViewInput;
        Vector2 ViewMoveInput = playerInputActions.Player.ViewMove.ReadValue<Vector2>();
        return ViewMoveInput.normalized;
    }
    public Vector2 GetMousMovement() //视角移动 https://www.youtube.com/watch?v=uPlXdMKM5pc
    {
        Vector2 ViewMoveInput = playerInputActions.Player.MouseMoving.ReadValue<Vector2>();
        // Debug.Log(ViewMoveInput);
        return ViewMoveInput;
    }

    IEnumerator UIControler()
    {

        isInputBlocked = true;

        float[] displayTimes = { 1f, 5f, 1f, 1f };
        GameObject[] panels = { fixationPanel, searchCuePanel, fixationPanel, startingPanel };
        // 前5秒：轮流显示UI面板
        float elapsed = 0f;
        Debug.Log(elapsed);
        int currentPanelIndex = 0;

        while (elapsed < introDuration)
        {
            foreach (var panel in panels)
            {
                panel.SetActive(false);
            }
            
            if (currentPanelIndex < panels.Length)
            {
                panels[currentPanelIndex].SetActive(true);
                yield return new WaitForSecondsRealtime(displayTimes[currentPanelIndex]);
                elapsed += displayTimes[currentPanelIndex];
                currentPanelIndex++;
            }
            else
            {
                break; // 所有面板显示完毕，退出循环
            }
        }

        // 隐藏所有UI面板
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        isInputBlocked = false;
        // 启动场景计时器（从点击开始，总 50 秒）
        GameManager.Instance.StartSceneTimer(searchTime);

    }
    private void onGameEnded(object sender, scr_model.GameEndEventArgs e)
    {
        // isInputBlocked = true;
        StartCoroutine(EndningUIControler(e.SearchResult));
        // Debug.LogWarning("输入已经被阻止了");
    }

    IEnumerator EndningUIControler(bool searchResult)
    {
        isInputBlocked = true;
        // 前5秒：轮流显示UI面板
        float elapsed = 0f;

        while (elapsed < introDuration)
        {
            if (searchResult)
            {
                VictoryPanel.SetActive(true);
            }
            else
            {
                FailePanel.SetActive(true);
            }

            yield return new WaitForSecondsRealtime(panelDisplayTime); // 显示当前面板2.5秒
            elapsed += panelDisplayTime;
        }

        // 隐藏所有UI面板
        VictoryPanel.SetActive(false);
        FailePanel.SetActive(false);
    }
    // IEnumerator SceneTimer()
    // {
    //     Debug.Log("SceneTimer started from UIInteraction click, waiting for " + searchTime + " seconds");
    //     yield return new WaitForSecondsRealtime(searchTime);
    //     Debug.Log("SceneTimer finished, quitting");
    //     #if UNITY_EDITOR
    //     UnityEditor.EditorApplication.isPlaying = false;
    //     #else
    //     Application.Quit();
    //     #endif
    // }

}