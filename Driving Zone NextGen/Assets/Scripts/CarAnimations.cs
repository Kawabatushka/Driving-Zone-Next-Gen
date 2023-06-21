using System;
using UnityEngine;

public class CarAnimations : MonoBehaviour
{
    public GameObject FLMesh;
    public GameObject FRMesh;
    public GameObject RLMesh;
    public GameObject RRMesh;
    
    private CarController carObject;
    
    private Quaternion wheelRotation;
    private Vector3 wheelPosition;
    
    void Awake()
    {
        carObject = gameObject.GetComponentInParent<CarController>();
    }

    // Update is called once per frame
    void Update() 
    {
        AnimateWheelMeshes();
    }
    
    
    // This method matches both the position and rotation of the WheelColliders with the WheelMeshes.
    void AnimateWheelMeshes()
    {
        try
        {
            carObject.FLCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            FLMesh.transform.position = wheelPosition;
            FLMesh.transform.rotation = wheelRotation;
            
            carObject.FRCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            FRMesh.transform.position = wheelPosition;
            FRMesh.transform.rotation = wheelRotation;
            
            carObject.RLCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            RLMesh.transform.position = wheelPosition;
            RLMesh.transform.rotation = wheelRotation;
            
            carObject.RRCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            RRMesh.transform.position = wheelPosition;
            RRMesh.transform.rotation = wheelRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }
}
