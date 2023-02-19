using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsTouchManager : MonoBehaviour
{
    public bool buttonPressFlag = false;

    public void ButtonDown()
    {
        buttonPressFlag = true;
    }

    public void ButtonUp()
    {
        buttonPressFlag = false;
    }

    // пока костыльный метод
    public void ButtonClick()
    {
        //Debug.Log("1 step\t\t" + buttonPressFlag);
        buttonPressFlag = !buttonPressFlag;
        Debug.Log("2 step\t\t" + buttonPressFlag);
        /*buttonPressFlag = false;
        Debug.Log("3 step\t\t" + buttonPressFlag);*/
    }
}
