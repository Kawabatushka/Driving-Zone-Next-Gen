using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
	public void SceneChange(int sceneID)
	{
		SceneManager.LoadScene(sceneID);
	}

	public void SceneChange(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void CloseApplication()
	{

#if UNITY_EDITOR
        	UnityEditor.EditorApplication.isPlaying = false;
#endif

		Application.Quit();
	}
}