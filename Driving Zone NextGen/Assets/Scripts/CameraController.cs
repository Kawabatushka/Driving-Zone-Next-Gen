using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private CinemachineVirtualCamera[] virtualCameras;
	private int currentCameraIndex = 0;


	void Start()
	{
		for (var i = 0; i < virtualCameras.Length; i++)
		{
			if (i == currentCameraIndex)
			{
				virtualCameras[i].Priority = 1;
			}
			else
			{ 
				virtualCameras[i].Priority = 0;
			}
		}
		//virtualCameras[currentCameraIndex].Priority = 1;
	}

	void Update()
	{
		// change Camera View by keyboard
		if (Input.GetKeyDown(KeyCode.C))
		{
			CameraChange();
		}
	}

	// change Camera View by UI
	public void CameraChange()
	{
		virtualCameras[currentCameraIndex].Priority = 0;
		currentCameraIndex++;
		if (currentCameraIndex >= virtualCameras.Length)
		{
			currentCameraIndex = 0;
		}
		virtualCameras[currentCameraIndex].Priority = 1;
	}
}