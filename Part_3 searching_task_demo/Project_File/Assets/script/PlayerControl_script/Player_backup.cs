// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Player_backup : MonoBehaviour
// {
//     [SerializeField] private float moveSpeed = 4f;
//     // [SerializeField] private float rotateSpeed = 10f;
//     [SerializeField] private GameInput gameInput;
//     private Collider playerCollider;
//     [SerializeField] private Camera playerCamera;
//     [SerializeField] private float maxInteractionDistance = 5f;
//     [SerializeField] private LayerMask countersLayerMask; //可以在UI界面选择要和什么layr交互
//     private bool isWalking;
//     private ClearCounter1 selectedCounter;

//     private Vector3 lastInteractDir;
//     private Vector3 PlayerRotation;//视角移动

//     [Header("Settings")]
//     public PlayerCameraSetting playerCameraSetting;//视角移动

//     private void Start()
//     {
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//         playerCollider = GetComponent<Collider>();
//         gameInput.OnInteractAction += GameInput_OnInteraction; // +=  gameInput.OnInteractAction（发布者）被订阅，如果激活 +右侧的实践代码就要激活
//         gameInput.OnMouseInteractAction += MouseClisk_OnInteraction;
//     }

//     private void MouseClisk_OnInteraction(object sender, MouseInteractEventArgs e)
//     {
//         // Vector2 screenPosition = Mouse.current.position.ReadValue();
//         Vector2 screenPosition = e.ScreenPosition;
//         Ray ray = playerCamera.ScreenPointToRay(screenPosition);
//         // Debug.DrawRay(ray.origin, ray.direction * maxInteractionDistance, Color.red, 1f);
//         RaycastHit hitinfo;
//         if (Physics.Raycast(ray, out hitinfo, maxInteractionDistance))
//         {
//             if (hitinfo.transform.TryGetComponent<DemoObjectInteract>(out DemoObjectInteract demoObject))
//             {
//                 demoObject.Interact();
//             }
//         }
//     }

//     private void GameInput_OnInteraction(object sender, EventArgs e) // 订阅者，上述gameInput.OnInteractAction 有活动的时候，GameInput_OnInteraction部分的代码要执行
//     {

//         selectedCounter?.Interact();
//         //HandleInteractions(); // 放在这里而不放在 void upodata的时候，才会只有在按下交互案件的时候出发交互，否则就会一直交互

//     }

//     private void Update()
//     {
//         HandleMovement();
//         //HandleInteractions();

//     }
//     public bool IsWalking()
//     {
//         return isWalking;
//     }

//     private void HandleInteractions()
//     {
//         Vector2 inputVector = gameInput.GetMovementVectorNormalized();
//         Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
//         Vector3 rayStart = transform.position + Vector3.up * 0.5f; // 从腰部高度发射

//         if (moveDir != Vector3.zero) // !!!!!!!!!!!!!!!!!!!!!!!!!! 如果放在必须先按E 交互按键之后才能交互的话，也就是HandleInteractions 没有放在update里面，那么这个就容易出bug也就是无法交互，最好是能放在一个一直updata的代码中
//         {
//             lastInteractDir = moveDir;
//         }

//         // float interactionDistance = 2f;
//         // if (Physics.Raycast(rayStart, lastInteractDir, out RaycastHit raycastHit, interactionDistance, countersLayerMask))
//         // //      射线起始点    ，   方向         ，  把射线接触到的信息 也可以用hitinfor替代， 射线距离    , 只会和countersLayerMask的表现惊醒交互和检测
//         // {
//         //     if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
//         //     {
//         //         //clearCounter.Interact();    
//         //     }

//         // }

//         RaycastHit hitinfo;
//         if (Physics.Raycast(rayStart, lastInteractDir, out hitinfo, 2f, countersLayerMask))
//         {
//             if (hitinfo.transform.TryGetComponent<ClearCounter1>(out ClearCounter1 counter)) // 直接返回的是布尔值，counter 类似一个只在if语句存在的ClearCounter1 的临时遥控器，如果要执行ClearCounter1 内的功能，比如interact，就可以通过conter来实现 (ref siki15)
//             {
//                 //counter.Interact(); // 直接返回的是布尔值，counter 类似一个只在if语句存在的ClearCounter1 的临时遥控器，如果要执行ClearCounter1 内的功能，比如interact，就可以通过conter来实现 (ref siki15)
//                 SetSelectedCounter(counter);
//             }
//             else
//             {
//                 SetSelectedCounter(null);
//             }

//             //hitinfo.transform.GetComponent<ClearCounter1>().Interact();
//             // ↑↑一个物体的总组件挂在一个名为ClerarCounter1 的脚本，然后就会
//             //print(hitinfo.collider.gameObject); //用这个方法可以报告，射线看到了什么
//         }

//         else
//         {
//             SetSelectedCounter(null);
//         }

//     }

//     public void SetSelectedCounter(ClearCounter1 counter)
//     {
//         if (counter != selectedCounter)
//         {
//             selectedCounter?.CancelSeclect();
//             counter?.SelectedCounter();
//             this.selectedCounter = counter;
//         }

//     }


//     private void HandleMovement()
//     {

//         Vector2 inputVector = gameInput.GetMovementVectorNormalized();
//         // Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);// 这是原本地狱厨房视角的WASD逻辑
//         Vector3 moveDir = transform.forward * inputVector.y + transform.right * inputVector.x;
//         moveDir.y = 0f; // 确保不会向上或向下移动
//         float moveDistance = moveSpeed * Time.deltaTime;
//         float playerRadius = .4f;
//         float playerHeight = 1f;

//         //canMove 也有一个可以替代的方法就是通过给player增加一collider组间，和给那些物体赋予碰撞体积是一样的方法 可见siki 13
//         // bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
//         bool canMove = !Physics.SphereCast(transform.position, playerRadius, moveDir, out RaycastHit hit, moveDistance);

//         bool orignalcanMove = canMove;
//         //float playerSize = .7f;
//         //bool canMove = ! Physics.Raycast(transform.position, moveDir, playerSize);

//         if (!canMove)
//         {
//             Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
//             canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

//             if (canMove)
//             {
//                 moveDir = moveDirX;
//             }

//             else
//             {
//                 Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
//                 canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

//                 if (canMove)
//                 {
//                     moveDir = moveDirZ;
//                 }

//             }
//         }

//         if (canMove)
//         {
//             moveDir = moveDir.normalized;
//             if (orignalcanMove)
//             {
//                 transform.position += moveDir * Time.deltaTime * moveSpeed;
//             }
//             else
//             {
//                 transform.position += moveDir * Time.deltaTime * moveSpeed * 0.5f; //造成有碰撞时候的粘滞感
//             }

//         }

//         //transform.position += (Vector3)inputVector;


//         isWalking = moveDir != Vector3.zero;

//         Vector2 rawViewInput = gameInput.GetMousVector();
//         PlayerRotation.y += playerCameraSetting.ViewXSensitivity * rawViewInput.x * Time.deltaTime;
//         transform.localRotation = Quaternion.Euler(PlayerRotation);
//         //因为之后的方向移动要通过鼠标来控制 所以以下代码不用了
//         // if (moveDir != Vector3.zero)
//         // {
//         //     transform.forward = Vector3.Slerp(transform.forward, moveDir * 1, Time.deltaTime * rotateSpeed);//控制小人的转向，并且是缓慢转向
//         //     //还有一个选项是Vector.Lerp 
//         // }

//         //Debug.Log(Time.deltaTime);

//     }

//     private void DrawDebugSphere(Vector3 position, float playerRadius, Color color)
//     {
//         throw new NotImplementedException();
//     }
// }
