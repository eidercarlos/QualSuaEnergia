using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInput : MonoBehaviour
{
        	
	// Update is called once per frame
	void Update ()
    {         
        if(Input.inputString != "")
        {   
            Debug.Log("inputString = " + Input.inputString);
            //StartCoroutine(GetUserInfo(Input.inputString.Trim()));
        }   
    }   
}   
