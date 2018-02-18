using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereInteraction : MonoBehaviour
{   
    private float horizontalSpeed = 2.0f;
    private float verticalSpeed = 2.0f;

    void Update()
    {      
        if(SceneHandler.Instance != null && SceneHandler.Instance.IsOnInteraction)
        {

            float hRotation = horizontalSpeed * Input.GetAxis("Mouse X");
            float vRotation = verticalSpeed * Input.GetAxis("Mouse Y");

            if (SceneHandler.Instance.ConfigItems.invertAxisX)
                hRotation = hRotation * (-1);

            if (SceneHandler.Instance.ConfigItems.invertAxisY)
                vRotation = vRotation * (-1);

            transform.Rotate(vRotation, -hRotation, 0, Space.World);

            if(transform.hasChanged)
            {
                Debug.Log("Local Rotation: " + transform.localEulerAngles);
            }
        }   
    }

}   
