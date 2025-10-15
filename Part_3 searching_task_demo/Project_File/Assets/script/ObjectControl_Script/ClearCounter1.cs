using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter1 : MonoBehaviour
{
    [SerializeField] private GameObject selectedCounter;
    [SerializeField] private GameObject KitchenobjectPrefb;
    [SerializeField] private Transform topPoint;
    public void Interact()
    {
        GameObject go = GameObject.Instantiate(KitchenobjectPrefb, topPoint);
        go.transform.localPosition = Vector3.zero;
        print(this.gameObject + " is interacting ...");
    }

    public void SelectedCounter()
    {
        selectedCounter.SetActive(true);
    }

    public void CancelSeclect()
    {
        selectedCounter.SetActive(false);
    }
}
