using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_model;
public class PlayerCam : MonoBehaviour
{

    // [SerializeField] private GameInput gameInput;
    // private Vector3 newCameraRotation;
    // // [Header("References")]
    // // public Transform cameraHolder;//如果使用这个的话，就可以通过拖拽挂载目标 holder, 后续的就可以用cameraHolder.localRotation 而不是 transform.locaLRotation
    // [Header("Settings")]
    // [SerializeField] private float viewClampYMin;
    // [SerializeField] private float viewClampYMax;
    // public PlayerCameraSetting playerCameraSetting;

    // private void Start()
    // {
    // newCameraRotation = transform.localEulerAngles;
    // transform.localRotation = Quaternion.Euler(newCameraRotation);
    // }
    // private void Update()
    // {
    //     CamRotation();
    // }
    // private void CamRotation()
    // {
    //     Vector2 rawViewInput = gameInput.GetMousVector();
    //     newCameraRotation.x += playerCameraSetting.ViewYSensitivity * rawViewInput.y * Time.deltaTime*-1;
    //     newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
    //     transform.localRotation = Quaternion.Euler(newCameraRotation);
    // }
}

