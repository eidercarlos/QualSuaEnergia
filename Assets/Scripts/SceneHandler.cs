using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour
{   
    public GameObject CanvasQryEmail;
    public InputField InputBarcode;
    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private string currentScene;
    private float timeLeftToGoIdle;
    private float timeIdleAfterStart = 3.0f;
    private float timeOfInteractionStart;
    private float timeToPrintInteraction = 7.0f; 
    private float idleTime = 5.0f;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private string printScrPath;
    private string printScrFileName;
    private int printScrQuality = 3;
    private float timeOfInputBarcodeValueChanged;

    void Awake ()
    {   
        //Check if the Idle scene is already loaded
        if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
        {   
            currentScene = idleSceneName;
            SceneManager.LoadScene(idleSceneName, LoadSceneMode.Additive);                            
        }   
    }   

    //string path = @"C:\Folder1\Folder2\Folder3\Folder4";
    //string newPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
    //Note This goes two levels up.The result would be:  newPath = @"C:\Folder1\Folder2\";

    //A option: string directory = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString();

    void Update()
    {   
        if(Time.time > timeIdleAfterStart)
        {   
            TimerHandler();
            
            if( (isOnInteraction) && ((Time.time - timeOfInteractionStart) > timeToPrintInteraction) )
            {
                StartCoroutine(ProcessPrint());
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

    private IEnumerator ProcessPrint()
    {   
        Debug.Log("Taking a picture...");
        isOnInteraction = false;

        //Adds a listener to the input field and invokes a method when the value changes.
        if(InputBarcode != null)
        {
            InputBarcode.onValueChanged.AddListener(delegate { InputBarcodeEndEdit(); });
        }
        else
        {
            Debug.Log("Objeto Input não encontrado ou associado...");
        }

        yield return TakeScreenShot();

        if (CanvasQryEmail != null)
        {
            CanvasQryEmail.SetActive(true);
            InputBarcode.Select();
        }

    }

    private IEnumerator TakeScreenShot()
    {
        //Debug.Log(Application.persistentDataPath);
        //Debug.Log(Application.dataPath); //D:/Projetos/QualSuaEnergia/QualSuaEnergia/Assets
        //Debug.Log(System.IO.Directory.GetDirectoryRoot(Application.dataPath));
        //System.IO.File.Move(printScrFileName, printScrPath+printScrFileName);
        string defaultPath = Application.dataPath; //D:\Projetos\QualSuaEnergia\QualSuaEnergia\Assets
        printScrPath = Path.GetFullPath(Path.Combine(defaultPath, @"..\..\"));
        printScrFileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        ScreenCapture.CaptureScreenshot(printScrFileName, printScrQuality);
        
        while (!System.IO.File.Exists(printScrFileName))
            yield return null;

        Debug.Log("Print Sucesso!");

    }

    //Invoked when the value of the text field changes.
    //Wait some time after the last change (char) and call a function to submit the value...
    private void InputBarcodeEndEdit()
    {   
        if(InputBarcode.text.Length > 0)
        {
            //Remove all listener so it can't be called another time...
            InputBarcode.onValueChanged.RemoveAllListeners();
            StartCoroutine(ProcessInputBarcode());
        }   
    } 
    
    private IEnumerator ProcessInputBarcode()
    {   
        yield return new WaitForSeconds(2f);
        Debug.Log("We have a total of: "+InputBarcode.text.Length+" characteres");
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
