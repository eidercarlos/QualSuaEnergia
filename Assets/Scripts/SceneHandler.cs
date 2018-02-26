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
using System;

public class SceneHandler : MonoBehaviour
{   
    public GameObject CanvasBarcode;
    public GameObject CanvasLogo;
    public GameObject CanvasTop;
    public GameObject CanvasFooter;
    public GameObject CanvasInteractionMsg;
    public GameObject CanvasSuccess;
    public GameObject CanvasError;
    public InputField InputBarcode;
    public Text TxtUsername;
    public Text TxtError;
    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private float timeLeftToGoIdle;
    private float timeOfInteractionStart;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private string printScrFileName;
    private Users currentUser;

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
        Cursor.visible = false;
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
            if(!CanvasInteractionMsg.activeInHierarchy || !CanvasTop.activeInHierarchy)
            {   
                SetActiveIdleCanvas(true);
            }
        }   
    }   

    void Update()
    {   
        TimerHandler();
        
        //Its time to take the screen shot
        if( (isOnInteraction) && ((Time.time - timeOfInteractionStart) > ConfigItems.time_print_after_start_interaction) )
        {      
            PauseScene();            
            ActiveBarcodeReader();
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
                timeLeftToGoIdle = ConfigItems.time_get_idle;
                Cursor.visible = false;
                catchCursor = true;

                if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
                {      
                    SetActiveInputPanel(false);
                    LoadTheScene(idleSceneName, interactionSceneName);
                    SetActiveIdleCanvas(true);
                    isOnInteraction = false;
                }   
            }   
        }   
        else //In case of the user is interacting with something....
        {   
            timeLeftToGoIdle = ConfigItems.time_get_idle;

            if(!SceneManager.GetSceneByName(interactionSceneName).isLoaded)
            {   
                LoadTheScene(interactionSceneName, idleSceneName);
                SetActiveIdleCanvas(false);
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

    private void ActiveBarcodeReader()
    {   
        //Adds a listener to the input field and invokes a method when the value changes.
        if(InputBarcode != null)
        {   
            InputBarcode.onValueChanged.AddListener(delegate { InputBarcodeEndEdit(); });
        }   
        else
        {   
            Debug.Log("Objeto Input não encontrado ou associado...");
        }   

        if(CanvasBarcode != null)
        {
            SetActiveInputPanel(true);
        }
    }

    private void SetActiveInputPanel(bool isActive)
    {   
        if(isActive)
        {   
            CanvasBarcode.SetActive(true);
            InputBarcode.Select();
            InputBarcode.ActivateInputField();
        }   
        else
        {   
            InputBarcode.text = "";
            CanvasBarcode.SetActive(false);
        }   
    }   
    
    //Invoked when the value of the text field changes.
    //Wait some time after the last change (char) and call a function to submit the value...
    private void InputBarcodeEndEdit()
    {   
        if(InputBarcode.text.Length > 0)
        {
            Debug.Log("Changing the barcode...");
            
            //Increasing the time left to get idle to prevent from get idle...
            timeLeftToGoIdle = ConfigItems.time_get_idle;
            //Remove all listener so it can't be called another time...
            InputBarcode.onValueChanged.RemoveAllListeners();
            StartCoroutine(ProcessBarcodeInput());
        }   
    } 
                
    private IEnumerator ProcessBarcodeInput()
    {   
        yield return new WaitForSeconds(1.3f);
        yield return GetRequest(ConfigItems.rest_api_url+InputBarcode.text);        
    }   

    private IEnumerator GetRequest(string uri)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.SendWebRequest();

        while(!request.isDone)
            yield return null;

        if(!request.isHttpError && !request.isNetworkError)
        {      
            string jsonResult = request.downloadHandler.text;
            Users currentUser = JsonConvert.DeserializeObject<Users>(jsonResult);
                            
            if(currentUser.nome != "" && currentUser.nome != null)
            {   
                if(CanvasBarcode.activeInHierarchy)
                    SetActiveInputPanel(false);

                ShowUserNameCanvas(currentUser);

                yield return TakeScreenShot(currentUser);
            }   
            else
            {                  
                //O código de barras não retornou nenhum registro...
                Debug.Log("A consulta não retornou nenhum usuário, Tente novamente fazer a leitura...");
                InputBarcode.text = "";
                if (!CanvasBarcode.activeInHierarchy)
                    CanvasBarcode.SetActive(true);

                ActiveBarcodeReader();
            }

        }   
        else
        {   
            Debug.Log("Erro na requisição dos dados da API REST:"+request.error);
            InputBarcode.text = "";
            if (!CanvasBarcode.activeInHierarchy)
                CanvasBarcode.SetActive(true);

            ActiveBarcodeReader();
        }
    }
                        
    private void ShowUserNameCanvas(Users user)
    {   
        CanvasFooter.SetActive(true);
        TxtUsername.text = user.nome.ToUpper();
    }
    
    private void HideUserNameCanvas()
    {
        CanvasFooter.SetActive(false);
        TxtUsername.text = "";
    }

    private IEnumerator TakeScreenShot(Users currentUser)
    {   
        bool printSuccess = true;

        printScrFileName = ConfigItems.print_file_name + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        string printScrPath = Path.Combine(Application.dataPath, printScrFileName);
        ScreenCapture.CaptureScreenshot(printScrPath, ConfigItems.print_quality_level);

        while (!System.IO.File.Exists(printScrPath))
            yield return null;

        try
        {   
            string pathToSave = @ConfigItems.print_path + currentUser.email;
            System.IO.Directory.CreateDirectory(pathToSave);
            File.Move(printScrPath, Path.Combine(pathToSave, printScrFileName));

            Debug.Log("Print Sucesso!");
        }
        catch (Exception e)
        {
            printSuccess = false;
            Debug.Log("The process failed: {0}" + e.Message);
        }

        if(printSuccess)
        {   
            yield return ReturnToIdle();
        }   
        else
        {   
            Debug.Log("Lets try again...");
        }   
    }

    private IEnumerator ShowSuccess()
    {
        yield return null;
    }

    private IEnumerator ReturnToIdle()
    {      
        yield return new WaitForSeconds(4f);
        HideUserNameCanvas();
        SetActiveInputPanel(false);
        isOnInteraction = false;
        if (CanvasFooter.activeInHierarchy)
            CanvasFooter.SetActive(false);
        LoadTheScene(idleSceneName, interactionSceneName);
        SetActiveIdleCanvas(true);
    } 
    
    private void SetActiveIdleCanvas(bool key)
    {
        if(key)
        {
            CanvasTop.SetActive(true);
            CanvasInteractionMsg.SetActive(true);
        }
        else
        {
            CanvasTop.SetActive(false);
            CanvasInteractionMsg.SetActive(false);
        }

    }

    private string FormatPath(string path)
    {
        string newPath = path.Replace("/", "\\");

        return newPath;
    }

    private void PauseScene()
    {
        isOnInteraction = false;
    }   

    private void ContinueScene()
    {
        isOnInteraction = true;
    }
}