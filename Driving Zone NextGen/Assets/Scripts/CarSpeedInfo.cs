using System;
using UnityEngine;
using TMPro;
public class CarSpeedInfo : MonoBehaviour
{
    [SerializeField] CarController _carController;
    Rigidbody _caRigidbody;
    [SerializeField] TMP_Text textGameObject;
    public bool isDiscreteSpeedValue = true;
    float speedInfoUpdateTime = 0f;
    
    
    void Start()
    {
        _caRigidbody = _carController.GetComponent<Rigidbody>();
        if (textGameObject != null)
        {
            textGameObject.text = "0";
        }
    }
    
    void FixedUpdate()
    {
        if (isDiscreteSpeedValue)
        {
            try
            {
                if (speedInfoUpdateTime < 0.2f)
                {
                    speedInfoUpdateTime += Time.deltaTime;
                }
                else if (speedInfoUpdateTime >= 0.2f)
                {
                    textGameObject.text = Mathf.RoundToInt(Mathf.Abs(_caRigidbody.velocity.magnitude * 3.6f)).ToString();
                    speedInfoUpdateTime = 0f;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else if (!isDiscreteSpeedValue)
        {
            
        }
    }
}
