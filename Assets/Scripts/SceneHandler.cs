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
    public GameObject CanvasIdle;
    public GameObject CanvasInteraction;
    public GameObject TimerPanel;
    public GameObject YourEnergyPanel;
    public GameObject BarcodePanel;
    public GameObject UsernamePanel;
    public GameObject LoadingPanel;
    public GameObject SuccessPanel;
    public GameObject ErrorPanel;
    public InputField InputBarcode;
    public Text TxtUsername;
    public Text TxtError;
    public Text TxtTimer;

    private string idleSceneName = "Idle";
    private string interactionSceneName = "Interaction";
    private float timeLeftToGoIdle;
    private float timeOfInteractionStart;
    private float timeAfterStopMovingMouse;
    private float cursorPosition;
    private bool catchCursor = true;
    private bool isOnInteraction = false;
    private bool isMovingMouse = false;   
    private string printScrFileName;
    private Users currentUser;
    private Users userFromJsonFile;

    public static SceneHandler Instance { get; set; }
    public Settings ConfigItems { get; set; }

    public bool IsOnInteraction
    {         
        get
        {        
            return isOnInteraction;
        }   
    }

    public bool IsMovingMouse
    {
        get
        {   
            return isMovingMouse;
        }   
    }

    public float TimeAfterStopMovingMouse
    {
        get
        {   
            return timeAfterStopMovingMouse;
        }   
    }

    public float TimeOfInteractionStart
    {   
        get
        {
            return timeOfInteractionStart;
        }
    }

    void Awake()
    {         
        Cursor.visible = false;
        Instance = this;
                                        
        //Check if the Idle scene is already loaded
        if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
        {      
            SceneManager.LoadScene(idleSceneName, LoadSceneMode.Additive);
            if(!CanvasIdle.activeSelf)
                CanvasIdle.SetActive(true);
        }

        //Ensure the CanvasInteraction is Hide
        if (CanvasInteraction.activeSelf)
            CanvasInteraction.SetActive(false);
    }

    void Start()
    {
        //Load the configurations from settings.json
        using (StreamReader jsonFile = new StreamReader("settings.json"))
        {
            string jsonFileContent = jsonFile.ReadToEnd();
            ConfigItems = JsonConvert.DeserializeObject<Settings>(jsonFileContent);
        }
    }

    void Update()
    {   
        TimerHandler();
        
        //Its time to take the screen shot
        if( (isOnInteraction) && ((Time.time - timeOfInteractionStart) > ConfigItems.time_print_after_start_interaction) )
        {                                 
            PauseScene();
            StartCoroutine(ShowYourEnergyPanel());
        }               
    }  
        
    private IEnumerator ShowYourEnergyPanel()
    {      
        PanelManagerActive("energy");
        yield return new WaitForSeconds(ConfigItems.time_show_energy_panel);
        PanelManagerActive("barcode");
    }   

    /// <summary>
    /// The function responsible to handle all behaviors which depends on time
    /// </summary>
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
            isMovingMouse = false;
            timeAfterStopMovingMouse += Time.deltaTime;
            timeLeftToGoIdle -= Time.deltaTime;
            if(timeLeftToGoIdle < 0)
            {   
                timeLeftToGoIdle = ConfigItems.time_get_idle;
                catchCursor = true;

                if(!SceneManager.GetSceneByName(idleSceneName).isLoaded)
                {   
                    PanelManagerActive("hide");

                    if(CanvasInteraction.activeSelf)
                        CanvasInteraction.SetActive(false);

                    LoadTheScene(idleSceneName, interactionSceneName);

                    if(!CanvasIdle.activeSelf)
                        CanvasIdle.SetActive(true);

                    isOnInteraction = false;
                }   
            }   
        }       
        else//In case of the user is moving the mouse....
        {            
            isMovingMouse = true;
            timeAfterStopMovingMouse = 0f;
            timeLeftToGoIdle = ConfigItems.time_get_idle;

            if(!SceneManager.GetSceneByName(interactionSceneName).isLoaded)
            {
                if (CanvasIdle.activeSelf)
                    CanvasIdle.SetActive(false);

                LoadTheScene(interactionSceneName, idleSceneName);

                if (!CanvasInteraction.activeSelf)
                    CanvasInteraction.SetActive(true);

                PanelManagerActive("hide");

                timeOfInteractionStart = Time.time;
                isOnInteraction = true;

                PanelManagerActive("timer");
            }
        }

        if(IsOnInteraction)
        {
            UpdateTimeLeftIdle();
        }
    }

    private void UpdateTimeLeftIdle()
    {   
        float timeLeftToPrint = ConfigItems.time_print_after_start_interaction - (Time.time - TimeOfInteractionStart);
        //string minutes = ((int)timeLeftToPrint / 60).ToString();
        //string seconds = (timeLeftToPrint % 60).ToString("f1");
        TxtTimer.text = timeLeftToPrint.ToString("f0");
    }   

    private void LoadTheScene(string sceneToLoadName, string sceneToUnloadName)
    {   
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneToUnloadName);
        SceneManager.UnloadScene(sceneToUnload);
        SceneManager.LoadScene(sceneToLoadName, LoadSceneMode.Additive);
    }   

    /// <summary>
    /// An event which is invoked when changing the barcode text field input
    /// </summary>
    private void InputBarcodeEndEdit()
    {      
        if(InputBarcode.text.Length > 0)
        {   
            ResetTimeLeftToIdle();
            //Remove all listener so it can't be called another time...
            InputBarcode.onValueChanged.RemoveAllListeners();
            StartCoroutine(ProcessBarcodeInput());
        }   
    }

    /// <summary>
    /// Function responsible to increase the time left to get in order to prevent from get idle...
    /// </summary>
    private void ResetTimeLeftToIdle()
    {
        timeLeftToGoIdle = ConfigItems.time_get_idle;
    }

    /// <summary>
    /// Function responsible to wait some time after the last change on the barcode input and call a function to submit the value...
    /// </summary>
    /// <returns>The return calls a function responsible to read and process the barcode input</returns>
    private IEnumerator ProcessBarcodeInput()
    {

        yield return new WaitForSeconds(1.3f);
        
        //string[] userInputSeparated = userInputQrCode.Split(new[] {ConfigItems.input_separator}, StringSplitOptions.None);

        //Input Data Validation
        if(InputBarcode.text != "" && InputBarcode.text != null)
        {   
            char lastUrlChar = ConfigItems.rest_api_url[ConfigItems.rest_api_url.Length - 1];
            if (lastUrlChar != '/')
            {
                ConfigItems.rest_api_url += '/';
            }

            //string apiParamsID = userInputSeparated[0];
            //string apiParamsEmail = userInputSeparated[1];
            string urlParams = ConfigItems.rest_api_url + InputBarcode.text;

            if(ConfigItems.exec_teste)
            {
                yield return ReadJsonUserData(InputBarcode.text);
            }
            else
            {
                yield return GetRequest(urlParams);
            }
        }
        else
        {   
            string errorMsg = "Formato de entrada de dados incorreto, Faça a leitura novamente";
            yield return ShowError(errorMsg);
        }
    }

    private IEnumerator ReadJsonUserData(string barcodeText)
    {         
        if(barcodeText != "" && barcodeText != null)
        {   
            int userId;
            if (int.TryParse(barcodeText, out userId))
            {
                //Load the configurations from settings.json
                Lead userData;
                using (StreamReader jsonFile = new StreamReader("leads.json"))
                {   
                    string jsonFileContent = jsonFile.ReadToEnd();
                    userFromJsonFile = JsonConvert.DeserializeObject<Users>(jsonFileContent);
                    userData = userFromJsonFile.lead[0];
                }   

                if (userData.name != "" && userData.name != null)
                {   
                    yield return ShowUserNamePanel(userData);
                    yield return TakeScreenShot(userData);
                }
                else
                {
                    //O código de barras não retornou nenhum registro...
                    string msgError = "A consulta não retornou nenhum usuário, Faça a leitura do código novamente.";
                    yield return ShowError(msgError);
                }
            }
            else
            {   
                //Debug.Log("Erro na requisição dos dados da API REST:"+request.error);
                string msgError = "Valor de entrada inválido, tente novamente!";
                yield return ShowError(msgError);
            }
        }
        else
        {
            string msgError = "Valor de entrada vazio, tente novamente!";
            yield return ShowError(msgError);
        }
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
            Lead lead = currentUser.lead[0];

            if(lead.name != "" && lead.email != null)
            {   
                yield return ShowUserNamePanel(lead);
                yield return TakeScreenShot(lead);
            }   
            else
            {             
                //O código de barras não retornou nenhum registro...
                string msgError = "A consulta não retornou nenhum usuário, Faça a leitura do código novamente.";
                yield return ShowError(msgError);
            }   
        }      
        else
        {      
            //Debug.Log("Erro na requisição dos dados da API REST:"+request.error);
            string msgError = "Erro na requisição dos dados da API REST";
            yield return ShowError(msgError);
        }
    }
                        
    private IEnumerator ShowUserNamePanel(Lead user)
    {         
        PanelManagerActive("username");
        TxtUsername.text = user.name;
        yield return new WaitForSeconds(ConfigItems.time_show_username_panel);
    }
        
    private IEnumerator TakeScreenShot(Lead currentUser)
    {
        bool printSuccess = true;
        string errorMsg = "";
        
        //The File
        printScrFileName = ConfigItems.print_file_name + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

        //Creating the folder to save the file
        string pathToSave = @ConfigItems.print_path + currentUser.email;
        var directoryInf = @System.IO.Directory.CreateDirectory(pathToSave);
        string printScrPath = @Path.Combine(pathToSave, printScrFileName);

        while(!directoryInf.Exists)
        {
            ResetTimeLeftToIdle();
            yield return null;
        }

        ScreenCapture.CaptureScreenshot(printScrPath, ConfigItems.print_quality_level);

        while (!System.IO.File.Exists(printScrPath))
        {   
            ResetTimeLeftToIdle();
            yield return null;
        }

        yield return ShowLoadingPanel();

        if (printSuccess)
        {   
            ResetTimeLeftToIdle();
            yield return ShowSuccess();
            yield return ReturnToIdle();
        }   
        else
        {   
            ShowError(errorMsg);
        }   
    }

    private IEnumerator ShowLoadingPanel()
    {
        PanelManagerActive("loading");
        yield return new WaitForSeconds(ConfigItems.time_show_loading_panel);
    }   

    private IEnumerator ShowSuccess()
    {
        PanelManagerActive("success");
        yield return new WaitForSeconds(ConfigItems.time_show_success_panel);
    }

    private IEnumerator ShowError(string errorMsg)
    {      
        if(!CanvasInteraction.activeSelf)
            CanvasInteraction.SetActive(true);

        PanelManagerActive("error");

        TxtError.text = errorMsg;

        //Wait some time to show the error msg...
        yield return new WaitForSeconds(4f);

        //Activate the barcode canvas in order to get the barcode data again...
        PanelManagerActive("barcode");
    }

    private IEnumerator ReturnToIdle()
    {            
        isOnInteraction = false;
        CanvasInteraction.SetActive(false);
        LoadTheScene(idleSceneName, interactionSceneName);
        CanvasIdle.SetActive(true);
        yield return null;
    }

    private void PauseScene()
    {
        isOnInteraction = false;
    }   

    private void PanelManagerActive(string panelName)
    {
        switch(panelName)
        {   
            case "barcode":

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);
                
                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                if (!BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(true);

                InputBarcode.text = "";
                InputBarcode.Select();
                InputBarcode.ActivateInputField();

                //Adds a listener to the input field which invokes a method when the value changes.
                InputBarcode.onValueChanged.AddListener(delegate { InputBarcodeEndEdit(); });

                break;

            case "username":

                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                if (!UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(true);

                TxtUsername.text = "";

                break;

            case "loading":

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                if (!LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(true);

                break;

            case "success":

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);

                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                if (!SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(true);

                break;

            case "error":

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);

                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                if (!ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(true);

                TxtError.text = "";

                break;

            case "energy":

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);

                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (!YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(true);

                break;

            case "timer":

                if (!TimerPanel.activeSelf)
                    TimerPanel.SetActive(true);

                break;

            case "hide":

                if (UsernamePanel.activeSelf)
                    UsernamePanel.SetActive(false);

                if (LoadingPanel.activeSelf)
                    LoadingPanel.SetActive(false);

                if (ErrorPanel.activeSelf)
                    ErrorPanel.SetActive(false);

                if (SuccessPanel.activeSelf)
                    SuccessPanel.SetActive(false);

                if (TimerPanel.activeSelf)
                    TimerPanel.SetActive(false);

                if (BarcodePanel.activeSelf)
                    BarcodePanel.SetActive(false);

                if (YourEnergyPanel.activeSelf)
                    YourEnergyPanel.SetActive(false);

                break;
        }   
    }
}