using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPauseMenu : MonoBehaviour
{

	[SerializeField] GameObject pauseButton;
	ButtonsTouchManager pauseGame_BTM;
	[SerializeField] GameObject continueGameButton;
	ButtonsTouchManager continueGame_BTM;
	[SerializeField] GameObject pausePanelUI;
	bool gameIsPaused = false;

	void Start()
	{
		pauseGame_BTM = pauseButton.GetComponent<ButtonsTouchManager>();
		continueGame_BTM = continueGameButton.GetComponent<ButtonsTouchManager>();
		pausePanelUI.SetActive(false);
	}

	void Update()
	{
		// UI control
		if (pauseGame_BTM.buttonPressFlag)
		{
			PauseUI();
		}
		if (continueGame_BTM.buttonPressFlag)
		{
			ResumeUI();
		}

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

	void ResumeUI()
	{
		pausePanelUI.SetActive(false);
		Time.timeScale = 1f;
		continueGame_BTM.buttonPressFlag = false;
	}
	void PauseUI()
	{
		pausePanelUI.SetActive(true);
		Time.timeScale = 0f;
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
}
