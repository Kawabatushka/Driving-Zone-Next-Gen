using UnityEngine;

public class ButtonsTouchManager : MonoBehaviour
{
    //[FormerlySerializedAs("buttonPressFlag")] public bool isPressed = false;
    public bool isPressed = false;
    
    public void ButtonDown()
    {
        isPressed = true;
    }

    public void ButtonUp()
    {
        isPressed = false;
    }

    public void ButtonClick()
    {
        isPressed = !isPressed;
    }
}