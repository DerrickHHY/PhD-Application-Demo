using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static scr_model;
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float viewMoveSpeed = 15f;
    // [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float playerRadius = 0.5f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxInteractionDistance = 100f;
    [SerializeField] private LayerMask countersLayerMask; 
    [SerializeField] private LayerMask obstacleLayerMask;



    private bool isWalking;
    private bool isMouseMoving = false;
    private Coroutine hideCoroutine;
    private Vector2 currentMousePosition;
    private ClearCounter1 selectedCounter;

    private Vector3 lastInteractDir;
    private Vector3 PlayerRotation;

    [Header("Settings")]
    public PlayerCameraSetting playerCameraSetting;

    [Header("Cursor Settings")]
    public float cursorHideDelay = 0.7f;
    public float mouseMoveThreshold = 1f;

    private void Start()
    {

        gameInput.OnInteractAction += GameInput_OnInteraction; 
        gameInput.OnMouseInteractAction += Mouse_PreInteraction;
        PlayerRotation = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(PlayerRotation);

    }

    private void Mouse_PreInteraction(object sender, MouseInteractEventArgs e)
    {

        currentMousePosition  = e.ScreenPosition;

    }

    private void GameInput_OnInteraction(object sender, EventArgs e) 
    {
        if (currentMousePosition != null)
        {
        Ray ray = playerCamera.ScreenPointToRay(currentMousePosition);
        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, maxInteractionDistance))
            {
                if (hitinfo.transform.TryGetComponent<DemoObjectInteract>(out DemoObjectInteract demoObject))
                {
                    demoObject.Interact();
                }
            }
        }

    }

    private void Update()
    {
        HandleMovement();

        CursorShow();

    }
    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; 

        if (moveDir != Vector3.zero) 
        {
            lastInteractDir = moveDir;
        }


        RaycastHit hitinfo;
        if (Physics.Raycast(rayStart, lastInteractDir, out hitinfo, 2f, countersLayerMask))
        {
            if (hitinfo.transform.TryGetComponent<ClearCounter1>(out ClearCounter1 counter)) 
            {

                SetSelectedCounter(counter);
            }
            else
            {
                SetSelectedCounter(null);
            }

        }

        else
        {
            SetSelectedCounter(null);
        }

    }

    public void SetSelectedCounter(ClearCounter1 counter)
    {
        if (counter != selectedCounter)
        {
            selectedCounter?.CancelSeclect();
            counter?.SelectedCounter();
            this.selectedCounter = counter;
        }

    }



    private void HandleMovement()
    {

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = transform.forward * inputVector.y + transform.right * inputVector.x;
        float moveDistance = moveSpeed * Time.deltaTime;
        
        RaycastHit hit;
        bool canMove = !Physics.SphereCast(transform.position, playerRadius, moveDir, out hit, moveDistance, obstacleLayerMask);
        bool orignalcanMove = canMove;

        Vector3 finalMoveDir = moveDir;
        float speedMultiplier = 1f;
        if (!canMove && hit.collider != null)
        {

            Vector3 collisionNormal = hit.normal;


            Vector3 projectedDir = Vector3.ProjectOnPlane(moveDir, collisionNormal).normalized;

            if (projectedDir.magnitude > 0.1f)
            {

                bool canMoveProjected = !Physics.SphereCast(transform.position, playerRadius, projectedDir, out hit, moveDistance, obstacleLayerMask);
                if (canMoveProjected)
                {
                    finalMoveDir = projectedDir;
                    canMove = true;
                    speedMultiplier = 0.5f; 
                }
                else
                {

                    canMove = false;
                  
                }
            }
            else
            {
                canMove = false;

            }
        }

        if (canMove)
        {
            transform.position += finalMoveDir.normalized * moveSpeed * speedMultiplier * Time.deltaTime;
        }



        isWalking = moveDir != Vector3.zero;

        Vector2 ViewMoveInput = gameInput.GetViewVectorNormalized();
        PlayerRotation.y += playerCameraSetting.ViewXSensitivity * ViewMoveInput.x * Time.deltaTime * viewMoveSpeed;
        PlayerRotation.x += playerCameraSetting.ViewYSensitivity * ViewMoveInput.y * Time.deltaTime * -1 * viewMoveSpeed;
        transform.localRotation = Quaternion.Euler(PlayerRotation);

    }
    private void CursorShow()
    {
        Vector2 mouseDelta = gameInput.GetMousMovement();
        float magnitude = mouseDelta.magnitude;
        
        if (magnitude > mouseMoveThreshold)
        {
            if (!isMouseMoving)
            {
                isMouseMoving = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                

                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                    hideCoroutine = null;
                }
            }
        }
        else if (isMouseMoving && hideCoroutine == null)  
        {
            isMouseMoving = false;
            hideCoroutine = StartCoroutine(HideCursorAfterDelay());
        }
    }
    private IEnumerator HideCursorAfterDelay()
    {
        yield return new WaitForSecondsRealtime(cursorHideDelay);


        if (!isMouseMoving)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        hideCoroutine = null; 
    }
    private void TryConservativeAvoidance(Vector3 originalDir, Vector3 collisionNormal, float moveDistance, out Vector3 outDir, out bool outCanMove, out float outSpeedMultiplier)

    {
        outDir = originalDir;
        outCanMove = false;
        outSpeedMultiplier = 0.3f;


        Vector3[] testDirections = {
            Vector3.ProjectOnPlane(originalDir, collisionNormal).normalized,
            Vector3.Cross(originalDir, collisionNormal).normalized,
            -Vector3.Cross(originalDir, collisionNormal).normalized
        };

        foreach (Vector3 testDir in testDirections)
        {
            if (testDir.magnitude < 0.1f) continue;

            if (!Physics.SphereCast(transform.position, playerRadius, testDir, out RaycastHit testHit, moveDistance, obstacleLayerMask))
            {
                outDir = testDir;
                outCanMove = true;
                return;
            }
        }
    }


    private void TryLateralMovement(Vector3 originalDir, Vector3 collisionNormal,float moveDistance, out Vector3 outDir, out bool outCanMove, out float outSpeedMultiplier)
    {
        outDir = originalDir;
        outCanMove = false;
        outSpeedMultiplier = 0.4f;
        

        Vector3 lateralDir1 = Vector3.Cross(originalDir, collisionNormal).normalized;
        Vector3 lateralDir2 = -lateralDir1;
        

        if (!Physics.SphereCast(transform.position, playerRadius, lateralDir1, out RaycastHit hit, moveDistance, obstacleLayerMask))
        {
            outDir = lateralDir1;
            outCanMove = true;
        }
        else if (!Physics.SphereCast(transform.position, playerRadius, lateralDir2, out hit, moveDistance, obstacleLayerMask))
        {
            outDir = lateralDir2;
            outCanMove = true;
        }
    }
}
