using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereInteraction : MonoBehaviour
{
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;

    void Update()
    {      
        //Later lets check why the instance gets null after some time....
        if(SceneHandler.Instance != null && SceneHandler.Instance.IsOnInteraction)
        {   
            float hRotation = horizontalSpeed * Input.GetAxis("Mouse X");
            float vRotation = verticalSpeed * Input.GetAxis("Mouse Y");

            if (SceneHandler.Instance.ConfigItems.invertAxisX)
                hRotation = hRotation * (-1);

            if (SceneHandler.Instance.ConfigItems.invertAxisY)
                vRotation = vRotation * (-1);

            transform.Rotate(vRotation, -hRotation, 0, Space.World);
        }   
    }   
}   
