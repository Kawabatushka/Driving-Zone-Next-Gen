using UnityEngine;

public class ShowPauseMenu : MonoBehaviour
{

	[SerializeField] GameObject gameplayUI;
	[SerializeField] GameObject pausePanelUI;
	bool gameIsPaused = false;

	void Start()
	{
		//gameplayUI.SetActive(true);
		pausePanelUI.SetActive(false);
	}

	void Update()
	{
		// Keyboard control
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (gameIsPaused)
			{
				ResumeKeyboard();
			}
			else
			{
				PauseKeyboard();
			}
		}

	}
	void ResumeKeyboard()
	{
		pausePanelUI.SetActive(false);
		Time.timeScale = 1f;
		gameIsPaused = false;
	}
	void PauseKeyboard()
	{
		pausePanelUI.SetActive(true);
		Time.timeScale = 0f;
		gameIsPaused = true;
	}


	// UI control

	public void ResumeUI()
	{
		pausePanelUI.SetActive(false);
		gameplayUI.SetActive(true);
		Time.timeScale = 1f;
	}
	public void PauseUI()
	{
		gameplayUI.SetActive(false);
		pausePanelUI.SetActive(true);
		Time.timeScale = 0f;
	}
	public void GarageUI()
	{
		pausePanelUI.SetActive(false);
		gameplayUI.SetActive(true);
		Time.timeScale = 1f;
	}
	public void SettingsUI()
	{
		Debug.Log("settings pamel is activated");
		//pausePanelUI.SetActive(false);
		gameplayUI.SetActive(false);
		// активировать окно/панель с настройками
		//Time.timeScale = 1f;
	}
}
