using System; 
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


    public static class InteractionEvents
    {

        public static event EventHandler<ObjectInteractionEventArgs> OnObjectInteracted;


        public static event EventHandler<ObjectInteractionEventArgs> OnObjectInteractionPerformed;


        public static void BroadcastObjectDetection(string objectName, GameObject obj = null)
        {
            OnObjectInteracted?.Invoke(null, new ObjectInteractionEventArgs(objectName, obj));

        }


        public static void BroadcastInteractionPerformed(string objectName, GameObject obj = null)
        {
            OnObjectInteractionPerformed?.Invoke(null, new ObjectInteractionEventArgs(objectName, obj));

        }
    }
    #endregion

    #region 
    [Serializable]
    public class GameEndEventArgs : EventArgs
    {
        public bool SearchResult { get; private set; }  
        
        public GameEndEventArgs(bool searchResult)
        {
            SearchResult = searchResult;
        }
    }
    #endregion
}
