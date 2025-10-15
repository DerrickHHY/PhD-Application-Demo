using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInputFake : MonoBehaviour
{
    [Header("Supervisor Seting")]
    public UIINPUT uiControl;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (uiControl != null && !uiControl.IsPanelVisible())
        {
            ExecuteSomething();
        }
    }

    private void ExecuteSomething()
    {
        Debug.Log("检测到了panel1 消失");
    }
}
