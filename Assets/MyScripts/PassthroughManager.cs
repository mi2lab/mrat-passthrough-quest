using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    [HideInInspector]
    public OVRPassthroughLayer passthrough;
    //public OVRInput.Button button;
    //public OVRInput.Controller controller;

    [HideInInspector]
    public GameObject VirtualEnv;

    // Start is called before the first frame update
    void Start()
    {
        passthrough.hidden = true;
        VirtualEnv.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(OVRInput.GetDown(button, controller))
        {
            passthrough.hidden = !passthrough.hidden;
        }
        */
    }

    public void ToggleEnv(){
        passthrough.hidden = !passthrough.hidden;
        VirtualEnv.SetActive(passthrough.hidden);
    }
}
