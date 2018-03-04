using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereInteraction : MonoBehaviour
{
    private Vector3 lastAngleX;
    private float totalAngleX;
    public Color[] ENELColors;

    private Color selectedColor1;
    private Color selectedColor2;

    private bool isParticlesFreezed = false;
    public bool IsParticlesFreezed
    {
        get
        {
            return isParticlesFreezed;
        }

        private set
        {
            isParticlesFreezed = value;
        }
    }

    public ParticleSystem[] AllParticles;
    public ParticleSystem[] ParticlesColorEffects1;
    public ParticleSystem[] ParticlesColorEffects2;
    public ParticleSystem[] ParticleEmitters;

    public ParticleSystem ElectricBeanParticles;
    public ParticleSystem CircleParticles;
    public ParticleSystem RayParticles;


    #region ParticleSystemVars

    private ParticleSystem.MainModule mainElectricBeanParticle;
    private ParticleSystem.MainModule mainCircleParticle;
    private ParticleSystem.MainModule mainRayParticle;

    private Vector3 mainOrbSize_Start;
    private Vector3 mainOrbSize_End;

    private float circleSize_Start;
    private float circleSize_End;

    private float electricBeanMinSize_Start;
    private float electricBeanMaxSize_Start;
    private float electricBeanMinSize_End;
    private float electricBeanMaxSize_End;

    private float RayMinLifetime_Start;
    private float RayMaxLifetime_Start;
    private float RayMinSize_Start;
    private float RayMaxSize_Start;
    private float RayMinLifetime_End;
    private float RayMaxLifetime_End;
    private float RayMinSize_End;
    private float RayMaxSize_End;

    #endregion

    private bool isEnergyBigger = false;

    SceneHandler sceneHandler;
    Animator anim;

    private void Awake()
    {         
        sceneHandler = SceneHandler.Instance;
        anim = GetComponent<Animator>();

        //Main size
        mainOrbSize_Start = transform.localScale;
        mainOrbSize_End = new Vector3(3f, 3f, 3f);

        //ElectricBean Particles
        mainElectricBeanParticle = ElectricBeanParticles.main;
        var electricBeanStartSize = mainElectricBeanParticle.startSize;
        electricBeanMinSize_Start = mainElectricBeanParticle.startSize.constantMin;
        electricBeanMaxSize_Start = mainElectricBeanParticle.startSize.constantMax;
                        
        electricBeanMinSize_End = 5f;
        electricBeanMaxSize_End = 9f;

        //CircleParticles
        mainCircleParticle = CircleParticles.main;
        circleSize_Start = mainCircleParticle.startSize.constant;

        circleSize_End = 5f;

        //RayParticles
        mainRayParticle = RayParticles.main;
        RayMinLifetime_Start = mainRayParticle.startLifetime.constantMin;
        RayMaxLifetime_Start = mainRayParticle.startLifetime.constantMax;
        RayMinSize_Start = mainRayParticle.startSize.constantMin;
        RayMaxSize_Start = mainRayParticle.startSize.constantMax;

        RayMinLifetime_End = 3f;
        RayMaxLifetime_End = 5f;
        RayMinSize_End = 0.1f;
        RayMaxSize_End = 0.5f;
    }

    void Update()
    {            
        if(sceneHandler != null && sceneHandler.IsOnInteraction)
        {

            if(sceneHandler.IsMovingMouse && !isEnergyBigger)
            {
                anim.SetTrigger("GrowEnergy");
                isEnergyBigger = true;
            }

            if(!sceneHandler.IsMovingMouse && isEnergyBigger && sceneHandler.TimeAfterStopMovingMouse >= 4f)
            {
                anim.SetTrigger("ShrinkEnergy");
                isEnergyBigger = false;
            }

            float hRotation = sceneHandler.ConfigItems.horizontal_speed * Input.GetAxis("Mouse X");
            float vRotation = sceneHandler.ConfigItems.vertical_speed * Input.GetAxis("Mouse Y");
            
            if(sceneHandler.ConfigItems.invert_axis_x)
                hRotation = hRotation * (-1);

            if (sceneHandler.ConfigItems.invert_axis_y)
                vRotation = vRotation * (-1);

            transform.Rotate(vRotation, -hRotation, 0, Space.World);
            
            if(transform.hasChanged)
            {   
                float psEmission = transform.eulerAngles.y;
                var XangleOffset = transform.eulerAngles.x - lastAngleX.x;

                //if(XangleOffset > 90f) XangleOffset -= 180f;
                //if(XangleOffset < -90f) XangleOffset += 180f;

                lastAngleX = transform.eulerAngles;

                totalAngleX += XangleOffset;

                //Debug.Log("Total Angles: "+totalAngleX);

                if(Mathf.Abs(totalAngleX) > 180f)
                {   
                    //Debug.Log("Greater than 180");
                    UpdateParticlesColor();
                }   

                if(psEmission > 0f && psEmission <= 180f)
                {   
                    UpdateParticlesEmission(psEmission);
                }
                else
                {   
                    UpdateParticlesEmission(psEmission/2);
                }   

            }
        }

        if(!sceneHandler.IsOnInteraction && !IsParticlesFreezed)
        {
            FreezeParticles();
        }
    }

    private void FreezeParticles()
    {
        if(AllParticles.Length > 0)
        {
            foreach (var particles in AllParticles)
            {
                if(particles != null)
                {
                    particles.Pause();
                }
            }
        }

        IsParticlesFreezed = true;
    }   

    private void UpdateParticlesColor()
    {   
        int rangeColorIndex = UnityEngine.Random.Range(0, ENELColors.Length-1);        
        selectedColor1 = ENELColors[rangeColorIndex];
                                
        do
        {   
            rangeColorIndex = UnityEngine.Random.Range(0, ENELColors.Length - 1);
            selectedColor2 = ENELColors[rangeColorIndex];

        }while(selectedColor1.Equals(selectedColor2));

        //Change the all the colors of the Particles 1
        if(ParticlesColorEffects1.Length > 0)
        {
            foreach (ParticleSystem effects in ParticlesColorEffects1)
            {
                if(effects != null)
                {
                    var mainEffect = effects.main;

                    if (effects.tag == "FogParticle")
                    {
                        mainEffect.startColor = new Color(selectedColor1.r, selectedColor1.g, selectedColor1.b, 0.2f);
                    }
                    else
                    {
                        mainEffect.startColor = selectedColor1;
                    }
                }
            }
        }


        if(ParticlesColorEffects2.Length > 0)
        {
            //Change the all the colors of the Particles 2
            foreach (ParticleSystem effects in ParticlesColorEffects2)
            {
                if(effects != null)
                {
                    var mainEffect = effects.main;

                    if (effects.tag == "FogParticle")
                    {
                        mainEffect.startColor = new Color(selectedColor2.r, selectedColor2.g, selectedColor2.b, 0.2f);
                    }
                    else
                    {
                        mainEffect.startColor = selectedColor2;
                    }
                }
            }
        }

        totalAngleX = 0f;
    }  
    
    private void UpdateParticlesEmission(float emission)
    {
        if(ParticleEmitters.Length > 0)
        {
            foreach (var emitters in ParticleEmitters)
            {
                if(emitters != null)
                {
                    ParticleSystem currentPs = emitters.GetComponent<ParticleSystem>();
                    var psEmission = currentPs.emission;
                    psEmission.rateOverTime = emission;
                }
            }
        }
    }

    //A function which transform angles in int numbers
    private int GetAngleIndex(float angles)
    {   
        int angleIdx = (int)(angles/360);
        return angleIdx;
    }

    private IEnumerator GrowEnergy()
    {   
        float currentLerpTime = 0.0f; //The current time measured during the LERP
        float timeInSeconds = 5f; //The cost of time to do the LERP
        float lerpPercentage = 0.0f; //Starts in 0.0f (0%) and Ends in 1.0f (100%)

        while (lerpPercentage <= 1f)
        {         
            currentLerpTime += Time.deltaTime;
            lerpPercentage = (currentLerpTime * 100 / timeInSeconds) / 100;

            //Lerp the Main Scale
            transform.localScale = Vector3.Lerp(mainOrbSize_Start, mainOrbSize_End, lerpPercentage);

            //Lerp the ElectricBean vars
            var electricBeanStartSize = mainElectricBeanParticle.startSize;
            electricBeanStartSize.constantMin = Mathf.Lerp(electricBeanMinSize_Start, electricBeanMinSize_End, lerpPercentage);
            electricBeanStartSize.constantMax = Mathf.Lerp(electricBeanMaxSize_Start, electricBeanMaxSize_End, lerpPercentage);

            //Lerp the Circle vars
            mainCircleParticle.startSize = Mathf.Lerp(circleSize_Start, circleSize_End, lerpPercentage);

            //Lerp the RayParticle Lifetime and Size
            var rayParticleStartLifeTime = mainRayParticle.startLifetime;
            rayParticleStartLifeTime.constantMin = Mathf.Lerp(RayMinLifetime_Start, RayMinLifetime_End, lerpPercentage);
            rayParticleStartLifeTime.constantMax = Mathf.Lerp(RayMaxLifetime_Start, RayMaxLifetime_End, lerpPercentage);

            var rayParticleStartSize = mainRayParticle.startSize;
            rayParticleStartSize.constantMin = Mathf.Lerp(RayMinSize_Start, RayMinSize_End, lerpPercentage);
            rayParticleStartSize.constantMax = Mathf.Lerp(RayMaxSize_Start, RayMaxSize_End, lerpPercentage);

            yield return null;
        }

        isEnergyBigger = true;
    }

    private IEnumerator ShrinkEnergy()
    {   
        float currentLerpTime = 0.0f; //The current time measured during the LERP
        float timeInSeconds = 5f; //The cost of time to do the LERP
        float lerpPercentage = 0.0f; //Starts in 0.0f (0%) and Ends in 1.0f (100%)

        while (lerpPercentage <= 1f)
        {
            currentLerpTime += Time.deltaTime;
            lerpPercentage = (currentLerpTime * 100 / timeInSeconds) / 100;

            //Lerp the Main Scale
            transform.localScale = Vector3.Lerp(mainOrbSize_End, mainOrbSize_Start, lerpPercentage);

            //Lerp the ElectricBean vars
            var electricBeanStartSize = mainElectricBeanParticle.startSize;
            electricBeanStartSize.constantMin = Mathf.Lerp(electricBeanMinSize_End, electricBeanMinSize_Start, lerpPercentage);
            electricBeanStartSize.constantMax = Mathf.Lerp(electricBeanMaxSize_End, electricBeanMaxSize_Start, lerpPercentage);

            //Lerp the Circle vars
            mainCircleParticle.startSize = Mathf.Lerp(circleSize_End, circleSize_Start, lerpPercentage);

            //Lerp the RayParticle Lifetime and Size
            var rayParticleStartLifeTime = mainRayParticle.startLifetime;
            rayParticleStartLifeTime.constantMin = Mathf.Lerp(RayMinLifetime_End, RayMinLifetime_Start, lerpPercentage);
            rayParticleStartLifeTime.constantMax = Mathf.Lerp(RayMaxLifetime_End, RayMaxLifetime_Start, lerpPercentage);

            var rayParticleStartSize = mainRayParticle.startSize;
            rayParticleStartSize.constantMin = Mathf.Lerp(RayMinSize_End, RayMinSize_Start, lerpPercentage);
            rayParticleStartSize.constantMax = Mathf.Lerp(RayMaxSize_End, RayMaxSize_Start, lerpPercentage);

            yield return null;
        }

        isEnergyBigger = false;
    }



}