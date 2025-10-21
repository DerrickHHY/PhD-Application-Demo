using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    public event EventHandler OnInteractAction; 
    public event EventHandler<MouseInteractEventArgs> OnMouseInteractAction; 
    private PlayerInputActions playerInputActions; 
    private bool waitingForFirstPanelClick = true;
    private bool isInputBlocked = true;
    [Header("UI Panels")]
    public GameObject fixationPanel; 
    public GameObject searchCuePanel; 
    public GameObject startingPanel; 

    public GameObject VictoryPanel;
    public GameObject FailePanel;

    [Header("UI Control Reference")]
    public UIControl uiControl;

    [Header("UI Timing")]
    public float introDuration = 10f; 
    public float panelDisplayTime = 2.5f; 
    private float searchTime = 45f;
    [SerializeField] private GameManager gameManager;

    private void Awake()                              
    {
        playerInputActions = new PlayerInputActions(); 
        playerInputActions.Player.Enable(); 
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.MouseClick.performed += ClickInteraction; 
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

            StartCoroutine(UIControler());

        }
        ;
    }

    private void OnDestroy()
    {

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

    private void Interact_performed(InputAction.CallbackContext obj) 
    {
        if (!isInputBlocked)
        {
            OnInteractAction?.Invoke(this, EventArgs.Empty);

        }

    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (isInputBlocked)
        {
            return Vector2.zero; 
        }
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
    public Vector2 GetViewVectorNormalized() 
    {

        if (isInputBlocked)
        {
            return Vector2.zero; 
        }

        Vector2 ViewMoveInput = playerInputActions.Player.ViewMove.ReadValue<Vector2>();
        return ViewMoveInput.normalized;
    }
    public Vector2 GetMousMovement()
    {
        Vector2 ViewMoveInput = playerInputActions.Player.MouseMoving.ReadValue<Vector2>();

        return ViewMoveInput;
    }

    IEnumerator UIControler()
    {

        isInputBlocked = true;

        float[] displayTimes = { 1f, 5f, 1f, 1f };
        GameObject[] panels = { fixationPanel, searchCuePanel, fixationPanel, startingPanel };

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
                break; 
            }
        }


        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        isInputBlocked = false;

        GameManager.Instance.StartSceneTimer(searchTime);

    }
    private void onGameEnded(object sender, scr_model.GameEndEventArgs e)
    {

        StartCoroutine(EndningUIControler(e.SearchResult));

    }

    IEnumerator EndningUIControler(bool searchResult)
    {
        isInputBlocked = true;

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

            yield return new WaitForSecondsRealtime(panelDisplayTime); 
            elapsed += panelDisplayTime;
        }

        VictoryPanel.SetActive(false);
        FailePanel.SetActive(false);
    }
 

}