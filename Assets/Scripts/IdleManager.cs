using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleManager : MonoBehaviour {

    SceneHandler sceneHandler;

    void Start()
    {
        sceneHandler = SceneHandler.Instance;

        float pos_x = sceneHandler.ConfigItems.orb_x_position;
        float pos_y = sceneHandler.ConfigItems.orb_y_position;
        float pos_z = sceneHandler.ConfigItems.orb_z_position;

        transform.position = new Vector3(pos_x, pos_y, pos_z);

    }

}
