using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{   
    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private string currentScene;
    private float timeLeftToGoIdle;
    private float timeIdleAfterStart = 4.0f;
    private float timeOfInteractionStart;
    private float timeToPrintInteraction = 10.0f; 
    private float idleTime = 5.0f;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private string printScrPath = "";

    void Awake ()
    {              
        //Check if the Idle scene is already loaded
        if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
        {      
            currentScene = idleSceneName;
            SceneManager.LoadScene(idleSceneName, LoadSceneMode.Additive);                            
        }   
    }

    void Update()
    {
        if(Time.time > timeIdleAfterStart)
        {   
            TimerHandler();

            if( (isOnInteraction) && ((Time.time - timeOfInteractionStart) > timeToPrintInteraction) )
            {   
                Debug.Log("taking a picture...");
                ScreenCapture.CaptureScreenshot("NicePicture.png", 4);
                isOnInteraction = false;
            }   
        }
    }

    private void TimerHandler()
    {   
        if(catchCursor)
        {   
            catchCursor = false;
            cursorPosition = Input.GetAxis("Mouse X");
        }   

        //Preparing to active the idle scene
        if(cursorPosition == Input.GetAxis("Mouse X"))
        {   
            timeLeftToGoIdle -= Time.deltaTime;
            if(timeLeftToGoIdle < 0)
            {   
                timeLeftToGoIdle = idleTime;
                Cursor.visible = false;
                catchCursor = true;

                if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
                {
                    LoadTheScene(idleSceneName, interactionSceneName);
                    isOnInteraction = false;
                }   
            }   
        }   
        else //In case of the user is interacting with something....
        {   
            timeLeftToGoIdle = idleTime;
            Cursor.visible = true;

            if(!SceneManager.GetSceneByName(interactionSceneName).isLoaded)
            {   
                LoadTheScene(interactionSceneName, idleSceneName);
                timeOfInteractionStart = Time.time;
                isOnInteraction = true;
            }
        }   
    }

    private void LoadTheScene(string sceneToLoadName, string sceneToUnloadName)
    {   
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneToUnloadName);
        SceneManager.UnloadSceneAsync(sceneToUnload);
        SceneManager.LoadScene(sceneToLoadName, LoadSceneMode.Additive);
    }

}

/*Original Script
float timeLeft;
     float visibleCursorTimer = 10.0f;
     float cursorPosition;
     bool catchCursor = true;
     void Update(){
         if(catchCursor){
             catchCursor = false;
             cursorPosition = Input.GetAxis("Mouse X");
         }
         if(Input.GetAxis("Mouse X") == cursorPosition)
              {
                  print("Mouse stop");
                  timeLeft -= Time.deltaTime;
                  if ( timeLeft < 0 )
                      {                
                      timeLeft = visibleCursorTimer;
                       Cursor.visible = false;
                       catchCursor=true;
                      }
              }else{
                  timeLeft = visibleCursorTimer;
                  Cursor.visible = true;
              }            
     } 
*/
