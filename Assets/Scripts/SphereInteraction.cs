using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereInteraction : MonoBehaviour
{   
    
    private float horizontalSpeed = 2.0f;
    private float verticalSpeed = 2.0f;

    private Vector3 lastAngle;
    private float totalAngleY = 0f;
    private int lastAngleIndex;
    private int currentEffectIndex;

    public GameObject[] Effects;
            
    public GameObject magicSphereModel;
    private GameObject magicSphereInstance;

    public Light Sun;
    public ReflectionProbe ReflectionProbe;
    public Light[] NightLights = new Light[0];

    private float startSunIntensity;
    private Quaternion startSunRotation;
    private Color startAmbientLight;
    private float startAmbientIntencity;
    private float startReflectionIntencity;
    private LightShadows startLightShadows;

    void Start()
    {   
        lastAngle = transform.eulerAngles;
        lastAngleIndex = 0;
        currentEffectIndex = 0;
        //GenerateNewEffect();
        GoNight();
    }   

    void Update()
    {            
        if(SceneHandler.Instance != null && SceneHandler.Instance.IsOnInteraction && magicSphereInstance != null)
        {   
            float hRotation = horizontalSpeed * Input.GetAxis("Mouse X");
            float vRotation = verticalSpeed * Input.GetAxis("Mouse Y");

            if (SceneHandler.Instance.ConfigItems.invertAxisX)
                hRotation = hRotation * (-1);

            if (SceneHandler.Instance.ConfigItems.invertAxisY)
                vRotation = vRotation * (-1);

            magicSphereInstance.transform.Rotate(vRotation, -hRotation, 0, Space.World);

            if(magicSphereInstance.transform.hasChanged)
            {   
                var YangleOffset = magicSphereInstance.transform.eulerAngles.y - lastAngle.y;
                float colorHUE = magicSphereInstance.transform.eulerAngles.x;

                if(YangleOffset > 180) YangleOffset -= 360f;
                if(YangleOffset < -180) YangleOffset += 360f;

                lastAngle = magicSphereInstance.transform.eulerAngles;

                totalAngleY += YangleOffset;

                int currentIndexEffect = GetAngleIndex(totalAngleY);

                if (currentIndexEffect > lastAngleIndex)
                {
                    ChangeCurrentEffect(+1);
                }
                else if (currentIndexEffect < lastAngleIndex)
                {
                    ChangeCurrentEffect(-1);
                }

                lastAngleIndex = currentIndexEffect;

                if(colorHUE > 0f)
                {      
                    var meshUpdater = magicSphereInstance.GetComponentInChildren<PSMeshRendererUpdater>();
                    if(meshUpdater != null)
                    {   
                        meshUpdater.UpdateColor(colorHUE / 360f);
                    }   
                }
            }      
        }
        else if(magicSphereInstance != null)
        {
            Destroy(magicSphereInstance);
        }
    }

    //A function which transform angles in int numbers
    private int GetAngleIndex(float angles)
    {   
        int angleIdx = (int)(angles/360);
        return angleIdx;
    }

    private void ChangeCurrentEffect(int delta)
    {
        currentEffectIndex += delta;
        if (currentEffectIndex > Effects.Length - 1)
            currentEffectIndex = 0;
        else if (currentEffectIndex < 0)
            currentEffectIndex = Effects.Length - 1;

        if (magicSphereInstance != null)
        {
            Destroy(magicSphereInstance);
            RemoveClones();
        }

        //currentInstance = Instantiate(Prefabs[currentNomber]);

        //Instantiation of the Character
        /*
        characterInstance = Instantiate(Character);
        characterInstance.GetComponent<ME_AnimatorEvents>().EffectPrefab = Prefabs[currentNomber];
        */

        //Current Instantiation and Config of the Sphere
        GenerateNewEffect();
    }

    private void GenerateNewEffect()
    {
        magicSphereInstance = Instantiate(magicSphereModel);

        var effectinstance = Instantiate(Effects[currentEffectIndex]);
        effectinstance.transform.parent = magicSphereInstance.transform;
        effectinstance.transform.localPosition = Vector3.zero;
        effectinstance.transform.localRotation = new Quaternion();

        var meshUpdater = effectinstance.GetComponent<PSMeshRendererUpdater>();
        meshUpdater.UpdateMeshEffect(magicSphereInstance);
    }

    private void RemoveClones()
    {
        var allGO = FindObjectsOfType<GameObject>();
        foreach (var go in allGO)
        {
            if (go.name.Contains("(Clone)")) Destroy(go);
        }
    }

    private void GoNight()
    {
        if(ReflectionProbe != null)
        {
            ReflectionProbe.RenderProbe();
        }

        Sun.intensity = 0.05f;
        Sun.shadows = LightShadows.None;
        foreach (var nightLight in NightLights)
        {
            nightLight.shadows = Sun.shadows;
        }

        Sun.transform.rotation = Quaternion.Euler(350, 30, 90);
        RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.2f);
        var lightInten = 1;
        RenderSettings.ambientIntensity = lightInten;
        RenderSettings.reflectionIntensity = 0.2f;
    }

}   
