using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    public OVRPassthroughLayer passthrough;
    public OVRInput.Button button;
    public OVRInput.Controller controller;

    public GameObject virtualRoom;

    // Start is called before the first frame update
    void Start()
    {
        passthrough.hidden = true;
        virtualRoom.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(button, controller))
        {
            passthrough.hidden = !passthrough.hidden;
        }
    }

    public void ToggleEnv(){
        passthrough.hidden = !passthrough.hidden;
        virtualRoom.SetActive(!virtualRoom.activeSelf);
    }
}
