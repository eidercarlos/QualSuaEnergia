﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereInteraction : MonoBehaviour
{
    private Vector3 lastAngleX;
    private float totalAngleX;
    private float lastEmittedValue;
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

    private bool isEnergyBigger = false;

    SceneHandler sceneHandler;
    Animator anim;

    private void Start()
    {            
        sceneHandler = SceneHandler.Instance;
        anim = GetComponent<Animator>();

        float pos_x = sceneHandler.ConfigItems.orb_x_position;
        float pos_y = sceneHandler.ConfigItems.orb_y_position;
        float pos_z = sceneHandler.ConfigItems.orb_z_position;

        transform.position = new Vector3(pos_x, pos_y, pos_z);
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

            if(sceneHandler.ConfigItems.exchange_axis)
            {   
                float exchangeAxis = hRotation;
                hRotation = vRotation;
                vRotation = exchangeAxis;
            }   
            
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
                
                int intEmission = Mathf.CeilToInt(psEmission);
                int roudedEmission = roundUp(intEmission, 5);
                if(roudedEmission != lastEmittedValue)
                {                     
                    if(roudedEmission <= 120)
                    {
                        UpdateParticlesEmission(roudedEmission);
                    }
                    else
                    {
                        UpdateParticlesEmission(roudedEmission/4);
                    }
                }      
            }

        }

        if (!sceneHandler.IsOnInteraction && !IsParticlesFreezed)
        {
            FreezeParticles();
        }
    }
    private int roundUp(int numToRound, int multiple)
    {
        if (multiple == 0)
            return numToRound;

        int remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;

        return numToRound + multiple - remainder;
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

        lastEmittedValue = emission;
    }

    //A function which transform angles in int numbers
    private int GetAngleIndex(float angles)
    {   
        int angleIdx = (int)(angles/360);
        return angleIdx;
    }
}