using System;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] TMP_Text textGameObject;
    float FPSInfoUpdateTime = 0f;
    
    
    void Update()
    {
        try
        {
            if (FPSInfoUpdateTime < 0.2f)
            {
                FPSInfoUpdateTime += Time.deltaTime;
            }
            else if (FPSInfoUpdateTime >= 0.2f)
            {
                textGameObject.text = "FPS:" + (int)(1.0f / Time.deltaTime);
                FPSInfoUpdateTime = 0f;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}