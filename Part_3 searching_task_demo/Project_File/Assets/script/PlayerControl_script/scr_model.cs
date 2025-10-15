using System; // 这里从using System.Collections修改为using System
using System.Collections.Generic;
using UnityEngine;

public static class scr_model
{
    #region -Player View-
    [Serializable]
    public class PlayerCameraSetting
    {
        [Header("View Setting")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;
    }
    #endregion

    #region -Interaction Events-
    /// <summary>
    /// 交互事件参数
    /// </summary>
    [Serializable]
    public class ObjectInteractionEventArgs : EventArgs
    {
        public string objectName { get; private set; }
        public GameObject interactedObject { get; private set; }

        public ObjectInteractionEventArgs(string name, GameObject obj = null)
        {
            objectName = name;
            interactedObject = obj;
        }
    }

    /// <summary>
    /// 交互事件管理器
    /// </summary>
    public static class InteractionEvents
    {
        // 物体被触碰/检测到时广播
        public static event EventHandler<ObjectInteractionEventArgs> OnObjectInteracted;

        // 具体交互执行时广播（如按E键）
        public static event EventHandler<ObjectInteractionEventArgs> OnObjectInteractionPerformed;

        /// <summary>
        /// 广播物体被检测到
        /// </summary>
        public static void BroadcastObjectDetection(string objectName, GameObject obj = null)
        {
            OnObjectInteracted?.Invoke(null, new ObjectInteractionEventArgs(objectName, obj));
            // Debug.Log($"[Event] Broadcast: {objectName}");
        }

        /// <summary>
        /// 广播交互执行
        /// </summary>
        public static void BroadcastInteractionPerformed(string objectName, GameObject obj = null)
        {
            OnObjectInteractionPerformed?.Invoke(null, new ObjectInteractionEventArgs(objectName, obj));
            // Debug.Log($"[Event] Interaction Performed: {objectName}");
        }
    }
    #endregion

    #region 
    [Serializable]
    public class GameEndEventArgs : EventArgs
    {
        public bool SearchResult { get; private set; }  // true=成功找到目标, false=自然超时
        
        public GameEndEventArgs(bool searchResult)
        {
            SearchResult = searchResult;
        }
    }
    #endregion
}
