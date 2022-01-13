using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARModeManager : MonoBehaviour
{
    public GameObject phone;
    public GameObject fov;

    public Camera phoneCam;
    public Camera fovCam;

    void Start()
    {
        phone.SetActive(false);
        fov.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleVis(){
        phone.SetActive(!phone.activeSelf);
        fov.SetActive(!fov.activeSelf);

        if(fov.activeSelf){
            fovCam.cullingMask = LayerMask.GetMask("Default", "Path3D");
        }
        else{
            fovCam.cullingMask = LayerMask.GetMask("Default");
        }

        // phoneCam.cullingMask = (phone.activeSelf) ? LayerMask.GetMask("Default", "Path3D") : LayerMask.GetMask("Default");
        // fovCam.cullingMask = (fov.activeSelf) ? LayerMask.GetMask("Default", "Path3D") : LayerMask.GetMask("Default");

        // fov.activeSelf ? fovCam.cullingMask = LayerMask.GetMask("Default", "Path3D") : fovCam.cullingMask = LayerMask.GetMask("Default");
    }
}
