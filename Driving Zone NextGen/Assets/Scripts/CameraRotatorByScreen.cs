using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CameraRotatorByScreen : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Button button;
    [SerializeField] private int direction;
    
    
    void Start()
    {
        button.onClick.AddListener(RotateFunction);
    }

    
    void Update()
    {
        
    }

    public void RotateFunction()
    {
        Debug.Log("rotateeeeeee");
    }
}