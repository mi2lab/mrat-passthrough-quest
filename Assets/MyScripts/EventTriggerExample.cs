using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerExample : MonoBehaviour
{
    public MRATManager mratManager;
    public string text = "This is an example of event triggering";
    public int level = 1;
    public bool test_click = false;

    public void OnClickFunction()
    {
        Debug.Log("Event Triggered");
        mratManager.TriggerEvent(text, level);
    }

    public void Update()
    {
        if (test_click)
        {
            OnClickFunction();
            test_click = false;
        }
    }

}
