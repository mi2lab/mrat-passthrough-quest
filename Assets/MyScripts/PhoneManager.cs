using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager : MonoBehaviour
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

    }
}
