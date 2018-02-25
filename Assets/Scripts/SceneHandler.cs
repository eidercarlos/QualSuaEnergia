using System.Collections;
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
    public GameObject CanvasQryEmail;
    public GameObject CanvasLogo;
    public GameObject CanvasTop;
    public GameObject CanvasFooter;
    public InputField InputBarcode;
    public Text TxtUsername;
    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private float timeLeftToGoIdle;
    private float timeOfInteractionStart;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private string printScrFileName;
    public GameObject IdleStars;
    public GameObject IdleSun;
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
                    ActivateIdleObjects();
                    LoadTheScene(idleSceneName, interactionSceneName);
                    isOnInteraction = false;
                }   
            }   
        }   
        else //In case of the user is interacting with something....
        {   
            timeLeftToGoIdle = ConfigItems.time_get_idle;
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

        if(CanvasQryEmail != null)
        {
            SetActiveInputPanel(true);
        }
    }

    private void SetActiveInputPanel(bool isActive)
    {   
        if(isActive)
        {   
            CanvasQryEmail.SetActive(true);
            InputBarcode.Select();
            InputBarcode.ActivateInputField();
        }   
        else
        {   
            InputBarcode.text = "";
            CanvasQryEmail.SetActive(false);
        }   
    }   
    
    //Invoked when the value of the text field changes.
    //Wait some time after the last change (char) and call a function to submit the value...
    private void InputBarcodeEndEdit()
    {   
        if(InputBarcode.text.Length > 0)
        {   
            Debug.Log("Changing the barcode...");
            //Remove all listener so it can't be called another time...
            InputBarcode.onValueChanged.RemoveAllListeners();
            StartCoroutine(ProcessBarcodeInput());
        }   
    } 
                
    private IEnumerator ProcessBarcodeInput()
    {   
        yield return new WaitForSeconds(1f);
        yield return GetRequest(ConfigItems.rest_api_url+InputBarcode.text);
        SetActiveInputPanel(false);
    }

    private IEnumerator GetRequest(string uri)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.SendWebRequest();

        while (!request.isDone)
            yield return null;
        
        if(!request.isHttpError && !request.isNetworkError)
        {   
            string jsonResult = request.downloadHandler.text;
            Users currentUser = JsonConvert.DeserializeObject<Users>(jsonResult);
            
            if(CanvasQryEmail.activeInHierarchy)
                SetActiveInputPanel(false);
                                 
            yield return TakeScreenShot(currentUser);
        }   
        else
        {   
            Debug.Log("Erro na requisição dos dados da API REST:"+request.error);
        }   
    }   

    private IEnumerator TakeScreenShot(Users currentUser)
    {   
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
            Debug.Log("The process failed: {0}" + e.Message);
        }
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

    private void PauseScene()
    {
        isOnInteraction = false;
    }   

    private void ContinueScene()
    {
        isOnInteraction = true;
    }
}