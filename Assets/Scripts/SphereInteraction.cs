using System;
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

    public ParticleSystem[] ParticleEffects;
    public ParticleSystem[] ParticleEmitters;

    const float TOLERANCE = 0.0001f;
    //static string[] colorProperties = { "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor", "_MainColor", "_CoreColor", "_FresnelColor", "_CutoutColor" };

    public struct HSBColor
    {
        public float H;
        public float S;
        public float B;
        public float A;

        public HSBColor(float h, float s, float b, float a)
        {
            this.H = h;
            this.S = s;
            this.B = b;
            this.A = a;
        }
    }

    void Start()
    {   
        lastAngle = transform.eulerAngles;
        lastAngleIndex = 0;
        currentEffectIndex = 0;
    }   

    void Update()
    {            
        if(SceneHandler.Instance != null && SceneHandler.Instance.IsOnInteraction)
        {   
            float hRotation = horizontalSpeed * Input.GetAxis("Mouse X");
            float vRotation = verticalSpeed * Input.GetAxis("Mouse Y");

            if(SceneHandler.Instance.ConfigItems.invertAxisX)
                hRotation = hRotation * (-1);

            if (SceneHandler.Instance.ConfigItems.invertAxisY)
                vRotation = vRotation * (-1);

            transform.Rotate(vRotation, -hRotation, 0, Space.World);
            
            if(transform.hasChanged)
            {         
                float colorHUE = transform.eulerAngles.x;
                float psEmission = transform.eulerAngles.y;

                if(colorHUE > 0f)
                {   
                    UpdateParticlesCollor(colorHUE / 360f);
                }
                
                if(psEmission > 0f)
                {
                    UpdateParticlesEmission(psEmission);
                }
            } 
        }
    }

    private void UpdateParticlesCollor(float HUEColor)
    {   
        foreach (var effects in ParticleEffects)
        {   
            effects.startColor = ConvertRGBColorByHUE(effects.startColor, HUEColor);
        }
    }  
    
    private void UpdateParticlesEmission(float emission)
    {
        foreach (var emitters in ParticleEmitters)
        {   
            ParticleSystem currentPs = emitters.GetComponent<ParticleSystem>();
            var psEmission = currentPs.emission;
            //float emissionOffset = 0f;

            //if (emission <= 340)
            //    emissionOffset = emission + 20;
            //else
            //    emissionOffset = 360f;

            //psEmission.rateOverTime = UnityEngine.Random.Range(emission, emissionOffset);
            psEmission.rateOverTime = emission;
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
        if (currentEffectIndex > ParticleEffects.Length - 1)
            currentEffectIndex = 0;
        else if (currentEffectIndex < 0)
            currentEffectIndex = ParticleEffects.Length - 1;

    }

    public static Color ConvertRGBColorByHUE(Color oldColor, float hue)
    {   
        var brightness = ColorToHSV(oldColor).B;
        if (brightness < TOLERANCE)
            brightness = TOLERANCE;
        var hsv = ColorToHSV(oldColor/ brightness);
        hsv.H = hue;
        var color = HSVToColor(hsv) * brightness;
        color.a = oldColor.a;
        return color;
    }

    public static HSBColor ColorToHSV(Color color)
    {   
        HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

        float r = color.r;
        float g = color.g;
        float b = color.b;

        float max = Mathf.Max(r, Mathf.Max(g, b));

        if (max <= 0)
            return ret;

        float min = Mathf.Min(r, Mathf.Min(g, b));
        float dif = max - min;

        if (max > min)
        {
            if (Math.Abs(g - max) < TOLERANCE)
                ret.H = (b - r) / dif * 60f + 120f;
            else if (Math.Abs(b - max) < TOLERANCE)
                ret.H = (r - g) / dif * 60f + 240f;
            else if (b > g)
                ret.H = (g - b) / dif * 60f + 360f;
            else
                ret.H = (g - b) / dif * 60f;
            if (ret.H < 0)
                ret.H = ret.H + 360f;
        }
        else
            ret.H = 0;

        ret.H *= 1f / 360f;
        ret.S = (dif / max) * 1f;
        ret.B = max;

        return ret;
    }

    public static Color HSVToColor(HSBColor hsbColor)
    {   
        float r = hsbColor.B;
        float g = hsbColor.B;
        float b = hsbColor.B;
        if (Math.Abs(hsbColor.S) > TOLERANCE)
        {
            float max = hsbColor.B;
            float dif = hsbColor.B * hsbColor.S;
            float min = hsbColor.B - dif;

            float h = hsbColor.H * 360f;

            if (h < 60f)
            {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.A);
    }

}   
