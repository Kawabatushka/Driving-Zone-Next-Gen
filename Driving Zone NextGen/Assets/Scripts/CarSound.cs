using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEditor.Rendering;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    AudioSource carAudio;

    [Range(0.01f, 1f)] public float minPitch = 0.15f;
    [Range(1f, 3f)] public float maxPitch = 2.5f;
    //private float pitchFromCar;
    
    CarController _carController = new CarController();

    
    void Start()
    {
        carAudio = GetComponent<AudioSource>();
        _carController = GetComponent<CarController>();
    }

    void FixedUpdate()
    {
        EngineSound();
        //Debug.Log("currentSpeed / maxSpeed = " + Mathf.Abs(_carController.carSpeed / _carController.maxSpeed) * 2);
    }
    
    
    void EngineSound()
    {
        if (_carController.carSpeed < _carController.minSpeed)
        {
            carAudio.pitch = minPitch;
        }
        if (_carController.carSpeed > _carController.minSpeed && _carController.carSpeed <= _carController.maxSpeed)
        {
            //carAudio.pitch = Mathf.Lerp(minPitch, maxPitch, (_carController.carSpeed / _carController.maxSpeed) * 2);
            carAudio.pitch = minPitch + Mathf.Abs(_carController.carSpeed / _carController.maxSpeed) * 2 /*+ Time.deltaTime*/ /*Mathf.Clamp(pitchFromCar, 1f, 3f)*/;
        }
        
        //currentSpeed = _carController.carSpeed;
        //pitchFromCar = (carRb.velocity.magnitude * 3.6f) / maxPitch;
        //carAudio.pitch = (_carController.carSpeed / _carController.maxSpeed) * 2 + 0.2f /*Mathf.Clamp(pitchFromCar, 1f, 3f)*/;
        /*if (_carController.carSpeed > minPitch && _carController.carSpeed< maxPitch)
        {
            carAudio.pitch = minPitch + Mathf.Clamp(pitchFromCar, 1f, 3f);
        }*/
    }
    
}