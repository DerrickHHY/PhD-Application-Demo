using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoObjectInteract : MonoBehaviour
{
    
    public void NotifyDetected()
    {
        scr_model.InteractionEvents.BroadcastObjectDetection(this.gameObject.name, this.gameObject);
        Debug.Log($"[{this.gameObject.name}] is under the mouse ");
    }
    public void Interact()
    {
        scr_model.InteractionEvents.BroadcastInteractionPerformed(this.gameObject.name, this.gameObject);
        // Debug.Log($"[{this.gameObject.name}] Interaction performed!");
        // 这里添加具体的交互逻辑：开门、拾取等
    }

}


