using UnityEngine;

/*
 *
 * удаление камеры назад при ускорении (acceleration - 1 из параметров)
 * приближение камеры при замедлении (deceleration - 1 из параметров)
 * отклонение камеры по оси X в зависимости от угла поворота колес, направления движения автомобиля (в т.ч. при дрифте). 
					Можно изменять не позицию, а угол поворота камеры по оси Y, чтобы offsetPos оставался постоянным 
					(т.е. камера вращалась всегда по траектории-коружности)
 *
 */

public class CameraFollower : MonoBehaviour
{

	[SerializeField] GameObject target;
	[SerializeField] GameObject[] cameras;
	static public int cameraNumber;
	[Space(5)]
	public GameObject cameraChangerButton;
	ButtonsTouchManager cameraChanger_BTM;

	[Space(5)]
	[SerializeField] Vector3 offsetPos/* = new Vector3(0f, 0f, 0f)*/; // default values (4.5, -3, 4.5)

	void Start()
	{
		// параметры каждой камеры записать в массив
		transform.position = new Vector3(0f, 3f, -17.5f);
		transform.eulerAngles = new Vector3(30f, target.transform.eulerAngles.y, 0f);

		cameraChanger_BTM = cameraChangerButton.GetComponent<ButtonsTouchManager>();

		offsetPos.x = offsetPos.z = target.transform.position.z - transform.position.z; 
		offsetPos.y = target.transform.position.y - transform.position.y;
		cameraNumber = 0;

		for (int i = 0; i < cameras.Length; i++)
		{
			if(i==cameraNumber)
			{
				cameras[i].SetActive(true);
			}
			else
			{
				cameras[i].SetActive(false);
			}
		}
	}

	private void Update()
	{
		if(cameraChanger_BTM.buttonPressFlag)
		{
			if (cameraNumber + 1 < cameras.Length)
			{
				cameras[cameraNumber].SetActive(false);
				cameraNumber++;
				cameras[cameraNumber].SetActive(true);
			}
			else
			{
				cameras[cameraNumber].SetActive(false);
				cameraNumber = 0;
				cameras[cameraNumber].SetActive(true);
			}
			
		}
	}

	private void LateUpdate()
	{
		transform.position = new Vector3(target.transform.position.x - offsetPos.x * Mathf.Sin(target.transform.eulerAngles.y * Mathf.Deg2Rad), target.transform.position.y - offsetPos.y, target.transform.position.z - offsetPos.z * Mathf.Cos(target.transform.eulerAngles.y * Mathf.Deg2Rad));
		
	}
}
