//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.UI;




// аккер барахлит (непонятно вычисляет угол поворота внутреннего колеса (там явно не ackermanCoef*...))


/*
 * переписать DecelerateCar(): найти, с каким ускорением замедляются авто в ИРЛ
 * добавить рассчет угла Аккера
 * дать игроку возможность настраивать параметры brakeForce, maxSteeringAngle, bodyMassCenter, ackermanAngle
 * добавить алгоритм нахождения текущей скорости carSpeed
 */


public class CarController : MonoBehaviour
{
	#region Variables
	//CAR SETUP

	[Header("CAR SETUP")]
    [Space(8)]
    [Range(70, 280)]
    public int maxSpeed = 100;
    [Range(30, 100)]
    public int maxReverseSpeed = 45;
    [Range(1, 10)]
    public int accelerationMultiplier = 3;
    [Range(50, 300)]
    public int brakeForce = 100;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
    //[Range(?, ?)]
    public float steeringSpeed = 0.5f;
    [Range(30, 60)]
    public int maxSteeringAngle = 38;
    /*[Range(0.9f, 1.85f)]
    public float ackermanCoef = 1.1f;*/
    [Space(10)]
    public Vector3 bodyMassCenter; // This is a vector that contains the center of mass of the car. I recommend to set this value
                                   // in the points x = 0 and z = 0 of your car. You can select the value that you want in the y axis,
                                   // however, you must notice that the higher this value is, the more unstable the car becomes.
                                   // Usually the y value goes from 0 to 1.5.

    //WHEELS

    [Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(5)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(5)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(5)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;


    //CAR DATA

    [HideInInspector]
    public float carSpeed; // Used to store the speed of the car.
    [HideInInspector]
    public bool isDrifting; // Used to know whether the car is drifting or not.
    [HideInInspector]
    public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.


    //CONTROLS

    [Space(20)]
    [Header("CONTROLS")]
    [Space(10)]
    //The following variables lets you to set up touch controls for mobile devices.
    /*public bool useTouchControls = true;*/
    [SerializeField] GameObject goForwardButton;
    ButtonsTouchManager throttle_BTM;
    [SerializeField] GameObject goBackButton;
    ButtonsTouchManager reverse_BTM;
    [SerializeField] GameObject turnRightButton;
    ButtonsTouchManager turnRight_BTM;
    [SerializeField] GameObject turnLeftButton;
    ButtonsTouchManager turnLeft_BTM;
    [SerializeField] GameObject handbrakeButton;
    ButtonsTouchManager handbrake_BTM;



    //PRIVATE VARIABLES

    Rigidbody carRigidbody; // Stores the car's rigidbody.
    [SerializeField] float localVelocityZ;
    [SerializeField] float localVelocityX;
    [SerializeField] float driftingAxis;
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    //float deceleratingAxis;
    bool deceleratingCar;
    //bool touchControlsSetup = false;
    /*
        The following variables are used to store information about sideways friction of the wheels (such as
        extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
        make the car to start drifting.
    */
    WheelFrictionCurve FLWheelFriction;
    float FLWExtremumSlip;
    WheelFrictionCurve FRWheelFriction;
    float FRWExtremumSlip;
    WheelFrictionCurve RLWheelFriction;
    float RLWExtremumSlip;
    WheelFrictionCurve RRWheelFriction;
    float RRWExtremumSlip;
	#endregion



	void Start()
    {
        //In this part, we set the 'carRigidbody' value with the Rigidbody attached to this
        //gameObject. Also, we define the center of mass of the car with the Vector3 given
        //in the inspector.
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        //Initial setup to calculate the drift value of the car. This part could look a bit
        //complicated, but do not be afraid, the only thing we're doing here is to save the default
        //friction values of the car wheels so we can set an appropiate drifting value later.
        if (true)
        {
            FLWheelFriction = new WheelFrictionCurve();
            FLWExtremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
            FLWheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
            FLWheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
            FLWheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
            FLWheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
            //FLWheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
            FRWheelFriction = new WheelFrictionCurve();
            FRWExtremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
            FRWheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
            FRWheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
            FRWheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
            FRWheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
            //FRWheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
            RLWheelFriction = new WheelFrictionCurve();
            RLWExtremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
            RLWheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
            RLWheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
            RLWheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
            RLWheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
            //RLWheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
            RRWheelFriction = new WheelFrictionCurve();
            RRWExtremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
            RRWheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
            RRWheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
            RRWheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
            RRWheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
            //RRWheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;
        }


		if (/*useTouchControls*/ true) // безусловное использование кнопок для управления (вместе с клавишами)
		{
			if (goForwardButton != null && goBackButton != null && turnRightButton != null && 
                turnLeftButton != null && handbrakeButton != null)
			{
				throttle_BTM = goForwardButton.GetComponent<ButtonsTouchManager>();
				reverse_BTM = goBackButton.GetComponent<ButtonsTouchManager>();
				turnLeft_BTM = turnLeftButton.GetComponent<ButtonsTouchManager>();
				turnRight_BTM = turnRightButton.GetComponent<ButtonsTouchManager>();
				handbrake_BTM = handbrakeButton.GetComponent<ButtonsTouchManager>();
				//touchControlsSetup = true;
			}
			else
			{
				String ex = "Touch controls are not completely set up. You must drag and drop your scene buttons " +
                    "in the relevant fields of CarController component.";
				Debug.LogWarning(ex);
			}
		}
	}


	void Update()
    {

        //CAR DATA

        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;


		//CAR PHYSICS

		/*
        The next part is regarding to the car controller. First, it checks if the user wants to use touch controls (for
        mobile devices) or analog input controls (WASD + Space).

        The following methods are called whenever a certain key is pressed. For example, in the first 'if' we call the
        method GoForward() if the user has pressed W.

        In this part of the code we specify what the car needs to do if the user presses W (throttle), S (reverse),
        A (turn left), D (turn right) or Space bar (handbrake).
        */

		#region Car movement code
		if (true)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || throttle_BTM.buttonPressFlag)
            {
                //Debug.Log("------GoForward()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoForward();
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || reverse_BTM.buttonPressFlag)
            {
                //Debug.Log("------GoReverse()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoReverse();
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || turnLeft_BTM.buttonPressFlag)
            {
                //Debug.Log("------TurnLeft()------");
                TurnLeft();
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || turnRight_BTM.buttonPressFlag)
            {
                //Debug.Log("------TurnRight()------");
                TurnRight();
            }
            if (Input.GetKey(KeyCode.Space) || handbrake_BTM.buttonPressFlag)
            {
                //Debug.Log("------Handbrake()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                //Debug.Log("------RecoverTraction()------");
                RecoverTraction();
            }
            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.DownArrow) || throttle_BTM.buttonPressFlag || reverse_BTM.buttonPressFlag))
            {
                //Debug.Log("------ThrottleOff()------");
                ThrottleOff();
            }
            if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.Space) || throttle_BTM.buttonPressFlag ||
                reverse_BTM.buttonPressFlag || handbrake_BTM.buttonPressFlag || deceleratingCar))
            {
                //Debug.Log("------DecelerateCar()------");
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }
            if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || turnLeft_BTM.buttonPressFlag ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || turnRight_BTM.buttonPressFlag ||
                steeringAxis == 0f))
            {
                //Debug.Log("------ResetSteeringAngle()------");
                ResetSteeringAngle();
            }
        }
		#endregion

		// We call the method AnimateWheelMeshes() in order to match the wheel collider movements with the 3D meshes of the wheels.
		AnimateWheelMeshes();
    }


    //STEERING METHODS
    
    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

	// This method matches both the position and rotation of the WheelColliders with the WheelMeshes.
	void AnimateWheelMeshes()
	{
		try
		{
			Quaternion FLWRotation;
			Vector3 FLWPosition;
			frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
			frontLeftMesh.transform.position = FLWPosition;
			frontLeftMesh.transform.rotation = FLWRotation;

			Quaternion FRWRotation;
			Vector3 FRWPosition;
			frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
			frontRightMesh.transform.position = FRWPosition;
			frontRightMesh.transform.rotation = FRWRotation;

			Quaternion RLWRotation;
			Vector3 RLWPosition;
			rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
			rearLeftMesh.transform.position = RLWPosition;
			rearLeftMesh.transform.rotation = RLWRotation;

			Quaternion RRWRotation;
			Vector3 RRWPosition;
			rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
			rearRightMesh.transform.position = RRWPosition;
			rearRightMesh.transform.rotation = RRWRotation;
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex);
		}
	}


	//ENGINE AND BRAKING METHODS

	public void GoForward()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            ///DriftCarPS();
        }
        else
        {
            isDrifting = false;
            //DriftCarPS();
        }
        // The following part sets the throttle power to 1 smoothly.
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        //If the car is going backwards, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
        //is safe to apply positive torque to go forward.
        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                // If the maxSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            //DriftCarPS();
        }
        else
        {
            isDrifting = false;
            //DriftCarPS();
        }
        // The following part sets the throttle power to -1 smoothly.
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        
        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                //Apply negative torque in all wheels to go in reverse if maxReverseSpeed has not been reached.
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                //If the maxReverseSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxReverseSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            //DriftCarPS();
        }
        else
        {
            isDrifting = false;
            //DriftCarPS();
        }
        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.velocity = carRigidbody.velocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
        // also cancel the invoke of this method.
        if (carRigidbody.velocity.magnitude < 0.25f)
        {
            carRigidbody.velocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        // We are going to start losing traction smoothly, there is were our 'driftingAxis' variable takes
        // place. This variable will start from 0 and will reach a top value of 1, which means that the maximum
        // drifting value has been reached. It will increase smoothly by using the variable Time.deltaTime.
        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * FLWExtremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWExtremumSlip)
        {
            driftingAxis = FLWExtremumSlip / (FLWExtremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car lost its traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        //If the 'driftingAxis' value is not 1f, it means that the wheels have not reach their maximum drifting
        //value, so, we are going to continue increasing the sideways friction of the wheels until driftingAxis
        // = 1f.
        if (driftingAxis < 1f)
        {
            FLWheelFriction.extremumSlip = FLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLWheelFriction;

            FRWheelFriction.extremumSlip = FRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRWheelFriction;
        }

        // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
        // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
        isTractionLocked = true;
        //DriftCarPS();

    }

    // This function is used to emit both the particle systems of the tires' smoke and the trail renderers of the tire skids
    // depending on the value of the bool variables 'isDrifting' and 'isTractionLocked'.
    /*public void DriftCarPS()
    {

        if (useEffects)
        {
            try
            {
                if (isDrifting)
                {
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }
                else if (!isDrifting)
                {
                    RLWParticleSystem.Stop();
                    RRWParticleSystem.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useEffects)
        {
            if (RLWParticleSystem != null)
            {
                RLWParticleSystem.Stop();
            }
            if (RRWParticleSystem != null)
            {
                RRWParticleSystem.Stop();
            }
            if (RLWTireSkid != null)
            {
                RLWTireSkid.emitting = false;
            }
            if (RRWTireSkid != null)
            {
                RRWTireSkid.emitting = false;
            }
        }

    }*/

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        //If the 'driftingAxis' value is not 0f, it means that the wheels have not recovered their traction.
        //We are going to continue decreasing the sideways friction of the wheels until we reach the initial
        // car's grip.
        if (FLWheelFriction.extremumSlip > FLWExtremumSlip)
        {
            FLWheelFriction.extremumSlip = FLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLWheelFriction;

            FRWheelFriction.extremumSlip = FRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRWheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (FLWheelFriction.extremumSlip < FLWExtremumSlip)
        {
            FLWheelFriction.extremumSlip = FLWExtremumSlip;
            frontLeftCollider.sidewaysFriction = FLWheelFriction;

            FRWheelFriction.extremumSlip = FRWExtremumSlip;
            frontRightCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip;
            rearLeftCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip;
            rearRightCollider.sidewaysFriction = RRWheelFriction;

            driftingAxis = 0f;
        }
    }
}