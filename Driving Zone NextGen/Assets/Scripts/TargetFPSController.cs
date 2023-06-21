using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFPSController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
            Debug.Log($"\t\t{Application.targetFrameRate}\t\t");
        Debug.Log("\t\tTarget FPS is changed\t\t");
#if UNITY_EDITOR
        Application.targetFrameRate = 90;
        Debug.Log("\t\tTarget FPS is changed\t\t");
#endif
        
#if UNITY_STANDALONE
        Application.targetFrameRate = 144;
#endif

#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

    }

    // Update is called once per frame
    void Update()
    {
            Debug.Log($"\t\t{Application.targetFrameRate}\t\t");

    }
}
