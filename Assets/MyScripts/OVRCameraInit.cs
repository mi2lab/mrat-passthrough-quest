using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRCameraInit : MonoBehaviour
{
    public GameObject RightEyeAnchor;
    // Start is called before the first frame update
    void Start()
    {
        Camera cam = RightEyeAnchor.GetComponent<Camera>();
        cam.cullingMask = cam.cullingMask & ~(1 << 7);
    }

    // Update is called once per frame
    void Update()
    {

        LayerMask.GetMask("Default", "Path3D");
    }
}
