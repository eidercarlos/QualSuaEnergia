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

    SceneHandler sceneHandler;

    private void Start()
    {
        sceneHandler = SceneHandler.Instance;
    }

    void Update()
    {            
        if(sceneHandler != null && sceneHandler.IsOnInteraction)
        {   
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

                if(psEmission > 0f && psEmission < 250f)
                {   
                    UpdateParticlesEmission(psEmission);
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
        foreach (var particles in AllParticles)
        {
            particles.Pause();
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
        foreach (ParticleSystem effects in ParticlesColorEffects1)
        {
            var mainEffect = effects.main;

            if(effects.tag == "FogParticle")
            {
                mainEffect.startColor = new Color(selectedColor1.r, selectedColor1.g, selectedColor1.b, 0.2f);
            }
            else
            {
                mainEffect.startColor = selectedColor1;
            }           
        }

        //Change the all the colors of the Particles 2
        foreach (ParticleSystem effects in ParticlesColorEffects2)
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

        totalAngleX = 0f;
    }  
    
    private void UpdateParticlesEmission(float emission)
    {
        foreach (var emitters in ParticleEmitters)
        {   
            ParticleSystem currentPs = emitters.GetComponent<ParticleSystem>();
            var psEmission = currentPs.emission;
            psEmission.rateOverTime = emission;
        }
    }

    //A function which transform angles in int numbers
    private int GetAngleIndex(float angles)
    {   
        int angleIdx = (int)(angles/360);
        return angleIdx;
    }

}