using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{   
    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private Scene idleScene;
    private Scene interactionScene;
    private string currentScene;

    private float timeLeftToGoIdle;
    private float idleTime = 5.0f;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isLoadingScene;

    void Start ()
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
        TimerHandler();
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
                    Debug.Log("Load the Idle scene and unload the interaction");

                }
            }   
        }   
        else //In case of the user is interacting with something....
        {   
            timeLeftToGoIdle = idleTime;
            Cursor.visible = true;

            if(!SceneManager.GetSceneByName(interactionSceneName).isLoaded)
            {
                Debug.Log("Load the Interaction scene and unload the idle");
            }
        }   
    }

    public IEnumerator LoadSceneCoroutine(string sceneToLoad, string sceneToUnload)
    {
        isLoadingScene = true;

        var previousScene = SceneManager.GetSceneByName(sceneToUnload);
        var previousSceneName = previousScene.name;

        // load target scene
        var loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        yield return loadOperation;

        //var targetScene = SceneManager.GetSceneByName(sceneToLoad);
        //SceneManager.SetActiveScene(targetScene);

        // unload previous scene
        var unloadSceneOperation = SceneManager.UnloadSceneAsync(previousScene);

        unloadSceneOperation.allowSceneActivation = false;

        while (!unloadSceneOperation.isDone)
            yield return null;

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
