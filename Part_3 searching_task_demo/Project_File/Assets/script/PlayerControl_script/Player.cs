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
    [SerializeField] private LayerMask countersLayerMask; //可以在UI界面选择要和什么layr交互
    [SerializeField] private LayerMask obstacleLayerMask;



    private bool isWalking;
    private bool isMouseMoving = false;
    private Coroutine hideCoroutine;
    private Vector2 currentMousePosition;
    private ClearCounter1 selectedCounter;

    private Vector3 lastInteractDir;
    private Vector3 PlayerRotation;//视角移动

    [Header("Settings")]
    public PlayerCameraSetting playerCameraSetting;//视角移动

    [Header("Cursor Settings")]
    public float cursorHideDelay = 0.7f;
    public float mouseMoveThreshold = 1f;

    private void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        gameInput.OnInteractAction += GameInput_OnInteraction; // +=  gameInput.OnInteractAction（发布者）被订阅，如果激活 +右侧的实践代码就要激活
        gameInput.OnMouseInteractAction += Mouse_PreInteraction;
        PlayerRotation = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(PlayerRotation);

    }

    private void Mouse_PreInteraction(object sender, MouseInteractEventArgs e)
    {
        // 逻辑是射线扫到之后就会通报，而并不一定要做出交互才会通报
        currentMousePosition  = e.ScreenPosition;
        // Ray ray = playerCamera.ScreenPointToRay(currentMousePosition);
        // RaycastHit hitinfo;
        // if (Physics.Raycast(ray, out hitinfo, maxInteractionDistance))
        // {
        //     if (hitinfo.transform.TryGetComponent<DemoObjectInteract>(out DemoObjectInteract demoObject))
        //     {
        //         demoObject.NotifyDetected(); 
        //     }
        // }
    }

    private void GameInput_OnInteraction(object sender, EventArgs e) // 订阅者，上述gameInput.OnInteractAction 有活动的时候，GameInput_OnInteraction部分的代码要执行
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
        // selectedCounter?.Interact();
        // Debug.Log("GameInput_OnInteraction is working");
        //HandleInteractions(); // 放在这里而不放在 void upodata的时候，才会只有在按下交互案件的时候出发交互，否则就会一直交互
    }

    private void Update()
    {
        HandleMovement();
        //HandleInteractions();
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
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; // 从腰部高度发射

        if (moveDir != Vector3.zero) // !!!!!!!!!!!!!!!!!!!!!!!!!! 如果放在必须先按E 交互按键之后才能交互的话，也就是HandleInteractions 没有放在update里面，那么这个就容易出bug也就是无法交互，最好是能放在一个一直updata的代码中
        {
            lastInteractDir = moveDir;
        }


        RaycastHit hitinfo;
        if (Physics.Raycast(rayStart, lastInteractDir, out hitinfo, 2f, countersLayerMask))
        {
            if (hitinfo.transform.TryGetComponent<ClearCounter1>(out ClearCounter1 counter)) // 直接返回的是布尔值，counter 类似一个只在if语句存在的ClearCounter1 的临时遥控器，如果要执行ClearCounter1 内的功能，比如interact，就可以通过conter来实现 (ref siki15)
            {
                //counter.Interact(); // 直接返回的是布尔值，counter 类似一个只在if语句存在的ClearCounter1 的临时遥控器，如果要执行ClearCounter1 内的功能，比如interact，就可以通过conter来实现 (ref siki15)
                SetSelectedCounter(counter);
            }
            else
            {
                SetSelectedCounter(null);
            }

            //hitinfo.transform.GetComponent<ClearCounter1>().Interact();
            // ↑↑一个物体的总组件挂在一个名为ClerarCounter1 的脚本，然后就会
            //print(hitinfo.collider.gameObject); //用这个方法可以报告，射线看到了什么
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

    // private void OnCollisionStay(Collision collision)
    // {
    //     // 只有特定标签的物体才阻挡移动
    //     canMove = !collision.gameObject.CompareTag("Obstacle");
    // }

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
            // 获取碰撞点的法线（垂直于碰撞表面的向量）
            Vector3 collisionNormal = hit.normal;

            // 计算原始移动方向在碰撞平面上的投影（避开障碍物的方向）
            Vector3 projectedDir = Vector3.ProjectOnPlane(moveDir, collisionNormal).normalized;//去除normalized?

            // 检查投影方向是否有效（不是零向量）
            if (projectedDir.magnitude > 0.1f)
            {
                // 验证投影方向是否可以移动
                bool canMoveProjected = !Physics.SphereCast(transform.position, playerRadius, projectedDir, out hit, moveDistance, obstacleLayerMask);
                if (canMoveProjected)
                {
                    finalMoveDir = projectedDir;
                    canMove = true;
                    speedMultiplier = 0.5f; // 沿着表面移动时稍微减速
                }
                else
                {
                    // 如果投影方向也不行，尝试更保守的避障
                    canMove = false;
                    // TryConservativeAvoidance(moveDir, collisionNormal, moveDistance, out finalMoveDir, out canMove, out speedMultiplier);
                }
            }
            else
            {
                canMove = false;
                // // 如果投影结果是零向量（比如直接撞墙），尝试侧向移动
                // TryLateralMovement(moveDir, collisionNormal, moveDistance, out finalMoveDir, out canMove, out speedMultiplier);
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
        //因为之后的方向移动要通过鼠标来控制 所以以下代码不用了
        // if (moveDir != Vector3.zero)
        // {
        //     transform.forward = Vector3.Slerp(transform.forward, moveDir * 1, Time.deltaTime * rotateSpeed);//控制小人的转向，并且是缓慢转向
        //     //还有一个选项是Vector.Lerp 
        // }

        //Debug.Log(Time.deltaTime);

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
                
                // 取消之前的隐藏协程
                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                    hideCoroutine = null;
                }
            }
        }
        else if (isMouseMoving && hideCoroutine == null)  // 只在无协程时启动
        {
            isMouseMoving = false;
            hideCoroutine = StartCoroutine(HideCursorAfterDelay());
        }
    }
    private IEnumerator HideCursorAfterDelay()
    {
        yield return new WaitForSecondsRealtime(cursorHideDelay);

        // 延迟后检查：如果仍未移动，才隐藏
        if (!isMouseMoving)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        hideCoroutine = null;  // 清空引用
    }
    private void TryConservativeAvoidance(Vector3 originalDir, Vector3 collisionNormal, float moveDistance, out Vector3 outDir, out bool outCanMove, out float outSpeedMultiplier)

    {
        outDir = originalDir;
        outCanMove = false;
        outSpeedMultiplier = 0.3f;

        // 尝试几个可能的避障方向
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

    // 侧向移动策略
    private void TryLateralMovement(Vector3 originalDir, Vector3 collisionNormal,float moveDistance, out Vector3 outDir, out bool outCanMove, out float outSpeedMultiplier)
    {
        outDir = originalDir;
        outCanMove = false;
        outSpeedMultiplier = 0.4f;
        
        // 生成与碰撞法线垂直的侧向方向
        Vector3 lateralDir1 = Vector3.Cross(originalDir, collisionNormal).normalized;
        Vector3 lateralDir2 = -lateralDir1;
        
        // 测试两个侧向方向
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
