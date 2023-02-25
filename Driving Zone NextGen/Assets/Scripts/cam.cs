//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;

/*
 *
 * удаление камеры назад при ускорении (acceleration - 1 из параметров)
 * приближение камеры при замедлении (deceleration - 1 из параметров)
 * отклонение камеры по оси X в зависимости от угла поворота колес, направления движения автомобиля (в т.ч. при дрифте). 
					Можно изменять не позицию, а угол поворота камеры по оси Y, чтобы offsetPos оставался постоянным 
					(т.е. камера вращалась всегда по траектории-коружности)
 * номер камеры сохранять в PlayerPrefs
 *
 */

public class cam : MonoBehaviour
{

	[SerializeField] GameObject target;
	Camera camera;
	public const int cameraNumber = 3;
	int currenCameraNumber;
	[SerializeField] CameraParametres[] cameraParametres;

	[System.Serializable]
	public class CameraParametres
	{
		[SerializeField] Vector3 cameraPosition;
		[SerializeField] Vector3 cameraRotation;
		[SerializeField] int cameraViewAngle;
		[SerializeField] Vector3 cameraOffsetPosition;
		[SerializeField] Vector3 cameraOffsetRotation;

		public CameraParametres(Vector3 _position, Vector3 _rotation, int _cameraViewAngle, in Transform _target, in Transform _transform)
		{
			cameraPosition = _position;
			cameraRotation = _rotation;
			cameraViewAngle = _cameraViewAngle;
			//cameraOffsetPosition = new(_target.transform.position.x - offsetPos.x * Mathf.Sin(_target.transform.eulerAngles.y * Mathf.Deg2Rad), _target.transform.position.y - _transform.position.y, _target.transform.position.z - offsetPos.z * Mathf.Cos(_target.transform.eulerAngles.y * Mathf.Deg2Rad));
		}

		public Vector3 CameraPosition
		{
			get { return cameraPosition; }
		}
		public Vector3 CameraRotation
		{
			get { return cameraRotation; }
		}
		public int CameraViewAngle
		{
			get { return cameraViewAngle; }
		}
		public Vector3 CameraOffsetPosition
		{
			get { return cameraOffsetPosition; }
		}
	}

	[Space(5)]
	[SerializeField] Vector3 offsetPos/* = new Vector3(0f, 0f, 0f)*/; // default values (4.5, -3, 4.5)

	void Awake()
	{
		camera = transform.GetComponent<Camera>();

		// созраняем сетапы для камеры: позицию, повороты, угол обзора, дистанцию от автомобиля
		Array.Resize(ref cameraParametres, cameraNumber);
		cameraParametres[0] = new CameraParametres(new(0f, 3f, -17.5f), new(30f, target.transform.eulerAngles.y, 0f), 60, target.transform, transform);
		cameraParametres[1] = new CameraParametres(new(0f, 3.7f, -19f), new(30f, target.transform.eulerAngles.y, 0f), 60, target.transform, transform);
		cameraParametres[2] = new CameraParametres(new(0f, 1.5f, -12.7f), new(15f, target.transform.eulerAngles.y, 0f), 65, target.transform, transform);

		// при запуске игры выбран первый сетап настроек камеры
		currenCameraNumber = 0;
		transform.position = cameraParametres[currenCameraNumber].CameraPosition;
		transform.eulerAngles = cameraParametres[currenCameraNumber].CameraRotation;
		camera.fieldOfView = cameraParametres[currenCameraNumber].CameraViewAngle;
	}

	void Start()
	{
		// вычисляем дистанцию камеры от автомобиля
		float horizontalOffset = (float)Math.Sqrt(Math.Pow(target.transform.position.x - transform.position.x, 2) + Math.Pow(target.transform.position.z - transform.position.z, 2));
		offsetPos = new(horizontalOffset, target.transform.position.y - transform.position.y, horizontalOffset);
		//cameraParametres[currenCameraNumber].CameraOffsetPosition = new(0f,0f,0f);
		Debug.Log(horizontalOffset);


	}

	void Update()
	{

	}

	void LateUpdate()
	{
		//transform.position = cameraParametres[currenCameraNumber].CameraPosition;
		transform.position = new(target.transform.position.x - offsetPos.x * Mathf.Sin(target.transform.eulerAngles.y * Mathf.Deg2Rad), target.transform.position.y - offsetPos.y, target.transform.position.z - offsetPos.z * Mathf.Cos(target.transform.eulerAngles.y * Mathf.Deg2Rad));
		transform.eulerAngles = cameraParametres[currenCameraNumber].CameraRotation;
		camera.fieldOfView = cameraParametres[currenCameraNumber].CameraViewAngle;

		//transform.position = new Vector3(target.transform.position.x - offsetPos.x * Mathf.Sin(target.transform.eulerAngles.y * Mathf.Deg2Rad), target.transform.position.y - offsetPos.y, target.transform.position.z - offsetPos.z * Mathf.Cos(target.transform.eulerAngles.y * Mathf.Deg2Rad));
		//transform.eulerAngles = new Vector3(30f, target.transform.eulerAngles.y, 0f);
	}

	public void CameraChange()
	{
		currenCameraNumber++;
		transform.position = cameraParametres[currenCameraNumber].CameraPosition;
		transform.eulerAngles = cameraParametres[currenCameraNumber].CameraRotation;
		camera.fieldOfView = cameraParametres[currenCameraNumber].CameraViewAngle;

	}
}
