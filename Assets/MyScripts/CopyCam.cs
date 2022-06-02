using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCam : MonoBehaviour
{

    public Camera cameraToCopy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().fieldOfView = cameraToCopy.fieldOfView;
    }
}
