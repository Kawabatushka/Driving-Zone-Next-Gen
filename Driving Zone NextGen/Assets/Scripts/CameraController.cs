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

public class CameraController : MonoBehaviour
{

	[SerializeField] GameObject target;
	public MonoBehaviour cameraMonoBehaviour;
	Camera camera;
	int cameraNumber = 4;
	[SerializeField] int currentCameraNumber;
	[SerializeField] CameraParametres[] cameraParametres;

	[System.Serializable]
	public class CameraParametres
	{
		[SerializeField] Vector3 cameraPosition;
		[SerializeField] Vector3 cameraRotation;
		[SerializeField] int cameraViewAngle;
		[SerializeField] Vector3 cameraOffsetPosition;
		[SerializeField] Vector3 cameraOffsetRotation; // поле для продвинутых эффектов камеры

		// конструктор по умолчанию
		public CameraParametres() { }
		// конструктор для полей, инициализируемых в Start()
		public CameraParametres(Vector3 _position, int _cameraViewAngle)
		{
			cameraPosition = _position;
			cameraViewAngle = _cameraViewAngle;
		}
		// конструктор для полей, инициализируемых в Update()
		public CameraParametres(Vector3 _rotation, in Transform _target, in Transform _transform)
		{
			cameraRotation = _rotation;
		}

		public Vector3 CameraPosition
		{
			get { return cameraPosition; }
			set { cameraPosition = value; }
		}
		public Vector3 CameraRotation
		{
			get { return cameraRotation; }
			set { cameraRotation = value; }
		}
		public int CameraViewAngle
		{
			get { return cameraViewAngle; }
			set { cameraViewAngle = value; }
		}
		public Vector3 CameraOffsetPosition
		{
			get { return cameraOffsetPosition; }
			set { cameraOffsetPosition = value; }
		}
		public Vector3 CameraOffsetRotation
		{
			get { return cameraOffsetRotation; }
			set { cameraOffsetRotation = value; }
		}
	}

	Vector3 _offsetPosition;


	private void Awake()
	{
		// в Volvo Demo так показано
		camera = transform.GetComponent<Camera>();
	}

	void Start()
	{

		// при запуске игры выбран первый сетап настроек камеры (пока не добавил PlayerPrefs)
		currentCameraNumber = 0;

		// меняем длину массива сетапов настроек камеры
		Array.Resize(ref cameraParametres, cameraNumber);
		for (int i = 0; i < cameraNumber; i++)
		{
			cameraParametres[i] = new CameraParametres();
		}

		#region Сохраняем сетапы для камеры: позицию, повороты, угол обзора, дистанцию от автомобиля
		cameraParametres[0] = new CameraParametres(new(0f, 3f, -17.5f), 60);
		cameraParametres[1] = new CameraParametres(new(0f, 3.7f, -19f), 55);
		cameraParametres[2] = new CameraParametres(new(0f, 1.5f, -12.7f), 65);
		cameraParametres[3] = new CameraParametres(new(0f, 2f, -17.5f), 60);
		
		_offsetPosition.x = _offsetPosition.z = (float)Math.Sqrt(Math.Pow(target.transform.position.x - cameraParametres[0].CameraPosition.x, 2) + Math.Pow(target.transform.position.z - cameraParametres[0].CameraPosition.z, 2));
		_offsetPosition.y = target.transform.position.y - cameraParametres[0].CameraPosition.y;
		cameraParametres[0].CameraOffsetPosition = _offsetPosition;
		_offsetPosition.x = _offsetPosition.z = (float)Math.Sqrt(Math.Pow(target.transform.position.x - cameraParametres[1].CameraPosition.x, 2) + Math.Pow(target.transform.position.z - cameraParametres[1].CameraPosition.z, 2));
		_offsetPosition.y = target.transform.position.y - cameraParametres[1].CameraPosition.y;
		cameraParametres[1].CameraOffsetPosition = _offsetPosition;
		// брать отрицательную дистанцию, т к камера находится спереди автомобиля
		_offsetPosition.x = _offsetPosition.z = -(float)Math.Sqrt(Math.Pow(target.transform.position.x - cameraParametres[2].CameraPosition.x, 2) + Math.Pow(target.transform.position.z - cameraParametres[2].CameraPosition.z, 2));
		_offsetPosition.y = target.transform.position.y - cameraParametres[2].CameraPosition.y;
		cameraParametres[2].CameraOffsetPosition = _offsetPosition;
		_offsetPosition.x = _offsetPosition.z = (float)Math.Sqrt(Math.Pow(target.transform.position.x - cameraParametres[3].CameraPosition.x, 2) + Math.Pow(target.transform.position.z - cameraParametres[3].CameraPosition.z, 2));
		_offsetPosition.y = target.transform.position.y - cameraParametres[3].CameraPosition.y;
		cameraParametres[3].CameraOffsetPosition = _offsetPosition;

		#endregion
	}

	void Update()
	{
		// переключение камеры с клавиатуры
		if (Input.GetKeyDown(KeyCode.C))
		{
			cameraMonoBehaviour.enabled = false;
			CameraChange();
			cameraMonoBehaviour.enabled = true;
		}
	}

	void LateUpdate()
	{

		#region Сохраняем сетапы для камеры: позицию, повороты, угол обзора, дистанцию от автомобиля

		cameraParametres[0].CameraRotation = new(30f, target.transform.eulerAngles.y, 0f);
		cameraParametres[1].CameraRotation = new(30f, target.transform.eulerAngles.y, 0f);
		cameraParametres[2].CameraRotation = new(15f, target.transform.eulerAngles.y, 0f);
		cameraParametres[3].CameraRotation = new(15f, target.transform.eulerAngles.y, 0f);

		#endregion


		transform.position = new(target.transform.position.x - cameraParametres[currentCameraNumber].CameraOffsetPosition.x * Mathf.Sin(target.transform.eulerAngles.y * Mathf.Deg2Rad), 
			target.transform.position.y - cameraParametres[currentCameraNumber].CameraOffsetPosition.y, 
			target.transform.position.z - cameraParametres[currentCameraNumber].CameraOffsetPosition.z * Mathf.Cos(target.transform.eulerAngles.y * Mathf.Deg2Rad));
		transform.eulerAngles = cameraParametres[currentCameraNumber].CameraRotation;
		camera.fieldOfView = cameraParametres[currentCameraNumber].CameraViewAngle;

	}

	// переключение камеры посредством UI
	public void CameraChange()
	{
		currentCameraNumber++;
		if (currentCameraNumber >= cameraNumber)
		{
			currentCameraNumber = 0;
		}
	}
}
