using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLookAt : MonoBehaviour
{
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void Update()
    {
        // make the canvas look at the camera
        transform.rotation = Quaternion.LookRotation((transform.position - Camera.main.transform.position).normalized);
    }
}
