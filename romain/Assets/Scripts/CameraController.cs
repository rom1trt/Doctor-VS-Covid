using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform target; // target to follow
    public Transform restingPosition; // resting position during charater selection
    public Vector3 offset; // target follow offset
    public float cameraDistance = 2f; // camera distance from target
    public float cameraHeight = 1f; // camrea height from target
    public float sensitivity = 5f; // camera mouse rotation sensitivity

    // script temp values
    Transform cameraTransform;
    float x;
    float y;
    float distanceOffset;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // set position to target and look at it
        if (target != null)
        {
            transform.position = target.position + offset;
            cameraTransform.LookAt(target.position + offset);

            // original position before applying collision offset
            Vector3 basePos = transform.TransformPoint(new Vector3(0, cameraHeight, -cameraDistance));

            // check for camera wall collision
            if (Physics.Linecast(transform.position, basePos, out RaycastHit hit))
                distanceOffset = Vector3.Distance(basePos, hit.point) + 0.3f;
            else
                distanceOffset = 0;

            // adjust wall collision offset
            distanceOffset = Mathf.Clamp(distanceOffset, 0, cameraDistance);

            // camera mouse rotation
            if (Input.GetKey(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject() && !GameManager.ended)
            {
                x += Input.GetAxis("Mouse X") * sensitivity;
                y += Input.GetAxis("Mouse Y") * sensitivity;
            }

            // clamp to avoid overflipping
            y = Mathf.Clamp(y, -45, 45);

            // adjust camera distance and height
            cameraTransform.localPosition = new Vector3(0, cameraHeight, -(cameraDistance - distanceOffset));

            // apply mouse rotation
            transform.rotation = Quaternion.Euler(-y, x, 0);
        }
        else
        {
            // if match not started then set camera to it's resting position
            transform.position = restingPosition.position;
            transform.rotation = restingPosition.rotation;
            cameraTransform.localPosition = new Vector3(0, cameraHeight, -cameraDistance);
            cameraTransform.localRotation = Quaternion.identity;
        }
    }
}
