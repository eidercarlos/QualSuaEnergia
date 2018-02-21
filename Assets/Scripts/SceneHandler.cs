﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;


public class SceneHandler : MonoBehaviour
{   
    public GameObject CanvasQryEmail;
    public InputField InputBarcode;
    private string idleSceneName = "Idle";
    private string interactionSceneName = "MainInteraction";
    private float timeLeftToGoIdle;
    private float timeOfInteractionStart;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private string printScrFileName;
    public GameObject IdleStars;
    public GameObject IdleSun;

    public static SceneHandler Instance { get; set; }
    public Settings ConfigItems { get; set; }

    public bool IsOnInteraction
    {   
        get
        {   
            return isOnInteraction;
        }   
    }   

    void Awake ()
    {
        Instance = this;
        
        //Load the configurations from settings.json
        using(StreamReader jsonFile = new StreamReader("settings.json"))
        {   
            string jsonFileContent = jsonFile.ReadToEnd();
            ConfigItems = JsonConvert.DeserializeObject<Settings>(jsonFileContent);            
        }   

        //Check if the Idle scene is already loaded
        if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
        {   
            SceneManager.LoadScene(idleSceneName, LoadSceneMode.Additive);                            
        }   
    }      

    //string path = @"C:\Folder1\Folder2\Folder3\Folder4";
    //string newPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
    //Note This goes two levels up.The result would be:  newPath = @"C:\Folder1\Folder2\";

    //A option: string directory = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString();

    void Update()
    {      
        TimerHandler();
                    
        if( (isOnInteraction) && ((Time.time - timeOfInteractionStart) > ConfigItems.timeToPrintAfterStartInteraction) )
        {   
            //StartCoroutine(ProcessPrint());
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
                timeLeftToGoIdle = ConfigItems.timeToGetIdle;
                Cursor.visible = false;
                catchCursor = true;

                if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
                {
                    ActivateIdleObjects();
                    LoadTheScene(idleSceneName, interactionSceneName);
                    isOnInteraction = false;
                }   
            }   
        }   
        else //In case of the user is interacting with something....
        {   
            timeLeftToGoIdle = ConfigItems.timeToGetIdle;
            Cursor.visible = true;

            if(!SceneManager.GetSceneByName(interactionSceneName).isLoaded)
            {   
                LoadTheScene(interactionSceneName, idleSceneName);
                DeactivateIdleObjects();
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
        //string defaultPath = Application.dataPath; //D:\Projetos\QualSuaEnergia\QualSuaEnergia\Assets
        //printScrPath = Path.GetFullPath(Path.Combine(defaultPath, @"..\..\"));
        printScrFileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

        //string printScrFile = Path.Combine(ConfigItems.printScrPath, ConfigItems.printScrFileName);
        ScreenCapture.CaptureScreenshot(printScrFileName, ConfigItems.printScrQualityLevel);
        
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
        
        yield return GetRequest(ConfigItems.APIRestURL+"?id="+InputBarcode.text);
        //Debug.Log("We have a total of: "+InputBarcode.text.Length+" characteres");
        //Store the inputbarcode text content into a string...
        InputBarcode.text = "";
        CanvasQryEmail.SetActive(false);
    }   
    
    private IEnumerator GetRequest(string uri)
    {   
        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        Users user = JsonConvert.DeserializeObject<Users>(request.downloadHandler.text);
        Debug.Log(request.downloadHandler.text);

        // Show results as text        
        Debug.Log(request.downloadHandler.text);
    }

    private string FormatPath(string path)
    {
        string newPath = path.Replace("/", "\\");

        return newPath;
    }

    public void DeactivateIdleObjects()
    {   
        IdleStars.SetActive(false);
        IdleSun.SetActive(false);
    }   

    public void ActivateIdleObjects()
    {   
        IdleStars.SetActive(true);
        IdleSun.SetActive(true);
    }   
}