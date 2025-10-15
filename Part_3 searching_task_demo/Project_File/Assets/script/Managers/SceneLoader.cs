using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
        

        // [Header("Timing")]
        // public float totalTime = 50f; // 总运行时间（秒）
        // public float introDuration = 5f; // 前5秒UI轮流时间

        // void Start()
        // {
        //     StartCoroutine(RunSceneTimer());
        // }

        // IEnumerator RunSceneTimer()
        // {
        //     // 前5秒：轮流显示UI面板
        //     // float elapsed = 0f;

        //     // 等待剩余时间（50 - 5 = 45秒）
        //     yield return new WaitForSecondsRealtime(totalTime - introDuration);
        //     Debug.Log("SceneTimer finished, quitting");
        //     #if UNITY_EDITOR
        //     UnityEditor.EditorApplication.isPlaying = false; // 在 Editor 中停止播放
        //     #else
        //     Application.Quit(); // 在构建版本中退出游戏
        //     #endif
        //     // 50秒结束：加载下一个场景（替换为你的场景名，或用Application.Quit()退出）
        //     // SceneManager.LoadScene("YourNextSceneName"); // 或 SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        // }
            
}
