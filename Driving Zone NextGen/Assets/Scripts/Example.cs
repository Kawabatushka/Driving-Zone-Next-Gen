using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    
    public class Example : MonoBehaviour
    {
        [SerializeField] UnityEvent m_MyEvent = new UnityEvent();
        [SerializeField] private int direction;

        void Start()
        {
            Debug.Log("class \"Exampe\" is called");
            //Add a listener to the new Event. Calls MyAction method when invoked
            m_MyEvent.AddListener(MyAction);
        }

        void Update()
        {
            // Press Q to close the Listener
            if (Input.GetKeyDown("q") && m_MyEvent != null)
            {
                Debug.Log("Quitting");
                m_MyEvent.RemoveListener(MyAction);

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif

                Application.Quit();
            }

            //Press any other key to begin the action if the Event exists
            if (Input.anyKeyDown && m_MyEvent != null)
            {
                //Begin the action
                m_MyEvent.Invoke();
            }
        }

        void MyAction()
        {
            //Output message to the console
            //Debug.Log("Do Stuff");
        }
    }
}