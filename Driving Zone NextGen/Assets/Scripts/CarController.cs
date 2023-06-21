using UnityEngine;
using System;

public class CarController : MonoBehaviour
{
    internal enum driveType{
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    
	#region Variables
	//CAR SETUP
    [Header("CAR SETUP")] [Space(8)] [Range(20, 280)]
    public short maxSpeed = 100;
    public float minSpeed { get; private set; }
    [Range(30, 100)] public int maxReverseSpeed = 45;
    [RangeEx(50, 500, 50)] public int accelerationMultiplier = 150;
    [Range(1, 10)] public int decelerationMultiplier = 2;
    [RangeEx(500, 4000, 250)] public int brakeForce = 2000;
    [Range(0f, 1f)] public float distributionBrakingForceOnFrontAxis = 0.5f;
    [Range(1, 10)] public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
    //[Range(?, ?)]
    public float steeringSpeed = 0.5f;
    [Range(30, 60)] public int maxSteeringAngle = 38;
    /*[Range(0.9f, 1.85f)]
    public float ackermanCoef = 1.1f;*/
    [SerializeField]private driveType drive;
    
    [SerializeField] 
    [Space(10)] public Vector3 bodyMassCenter; // This is a vector that contains the center of mass of the car. I recommend to set this value
                                   // in the points x = 0 and z = 0 of your car. You can select the value that you want in the y axis,
                                   // however, you must notice that the higher this value is, the more unstable the car becomes.
                                   // Usually the y value goes from 0 to 1.5.
    
    //WHEELS

    [Header("WHEELS")] 
    public WheelCollider FLCollider;
    public WheelCollider FRCollider;
    public WheelCollider RLCollider;
    public WheelCollider RRCollider;
    
    //CAR DATA

    [HideInInspector] public float carSpeed { get; private set; }
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;
    [HideInInspector] public bool isBraking;

    //CONTROLS

    [Space(20)] [Header("CONTROLS")] [Space(10)] [SerializeField]
    private GameObject goForwardButton;
    ButtonsTouchManager throttle_BTM;
    [SerializeField] private GameObject goBackButton;
    private ButtonsTouchManager reverse_BTM;
    [SerializeField] private GameObject turnRightButton;
    private ButtonsTouchManager turnRight_BTM;
    [SerializeField] private GameObject turnLeftButton;
    private ButtonsTouchManager turnLeft_BTM;
    [SerializeField] private GameObject handbrakeButton;
    private ButtonsTouchManager handbrake_BTM;
    

    //PRIVATE VARIABLES

    Rigidbody carRigidbody; // Stores the car's rigidbody.
    [SerializeField] private float localVelocityZ;
    [SerializeField] private float localVelocityX;
    [SerializeField] private float driftingAxis;
    private float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.

    private float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    //float deceleratingAxis;
    private bool deceleratingCar;
    /*
        The following variables are used to store information about sideways friction of the wheels (such as
        extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
        make the car to start drifting.
    */
    private WheelFrictionCurve FLWheelFriction;
    private float FLWExtremumSlip;
    private WheelFrictionCurve FRWheelFriction;
    private float FRWExtremumSlip;
    private WheelFrictionCurve RLWheelFriction;
    private float RLWExtremumSlip;
    private WheelFrictionCurve RRWheelFriction;
    private float RRWExtremumSlip;
	#endregion

    public float extremumSlip;
    public float extremumValue;
    public float asymptoteSlip;
    public float asymptoteValue;
    public float stiffness;
    
	void Start()
    {
        //In this part, we set the 'carRigidbody' value with the Rigidbody attached to this
        //gameObject. Also, we define the center of mass of the car with the Vector3 given
        //in the inspector.
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        //minSpeed = 0.25f;

        //Initial setup to calculate the drift value of the car. This part could look a bit
        //complicated, but do not be afraid, the only thing we're doing here is to save the default
        //friction values of the car wheels so we can set an appropiate drifting value later.
        if (true)
        {
            FLWheelFriction = new WheelFrictionCurve();
            FLWExtremumSlip = FLCollider.sidewaysFriction.extremumSlip;
            FLWheelFriction.extremumSlip = FLCollider.sidewaysFriction.extremumSlip;
            FLWheelFriction.extremumValue = FLCollider.sidewaysFriction.extremumValue;
            FLWheelFriction.asymptoteSlip = FLCollider.sidewaysFriction.asymptoteSlip;
            FLWheelFriction.asymptoteValue = FLCollider.sidewaysFriction.asymptoteValue;
            FLWheelFriction.stiffness = FLCollider.sidewaysFriction.stiffness;
            FRWheelFriction = new WheelFrictionCurve();
            FRWExtremumSlip = FRCollider.sidewaysFriction.extremumSlip;
            FRWheelFriction.extremumSlip = FRCollider.sidewaysFriction.extremumSlip;
            FRWheelFriction.extremumValue = FRCollider.sidewaysFriction.extremumValue;
            FRWheelFriction.asymptoteSlip = FRCollider.sidewaysFriction.asymptoteSlip;
            FRWheelFriction.asymptoteValue = FRCollider.sidewaysFriction.asymptoteValue;
            FRWheelFriction.stiffness = FRCollider.sidewaysFriction.stiffness;
            RLWheelFriction = new WheelFrictionCurve();
            RLWExtremumSlip = RLCollider.sidewaysFriction.extremumSlip;
            RLWheelFriction.extremumSlip = RLCollider.sidewaysFriction.extremumSlip;
            RLWheelFriction.extremumValue = RLCollider.sidewaysFriction.extremumValue;
            RLWheelFriction.asymptoteSlip = RLCollider.sidewaysFriction.asymptoteSlip;
            RLWheelFriction.asymptoteValue = RLCollider.sidewaysFriction.asymptoteValue;
            RLWheelFriction.stiffness = RLCollider.sidewaysFriction.stiffness;
            RRWheelFriction = new WheelFrictionCurve();
            RRWExtremumSlip = RRCollider.sidewaysFriction.extremumSlip;
            RRWheelFriction.extremumSlip = RRCollider.sidewaysFriction.extremumSlip;
            RRWheelFriction.extremumValue = RRCollider.sidewaysFriction.extremumValue;
            RRWheelFriction.asymptoteSlip = RRCollider.sidewaysFriction.asymptoteSlip;
            RRWheelFriction.asymptoteValue = RRCollider.sidewaysFriction.asymptoteValue;
            RRWheelFriction.stiffness = RRCollider.sidewaysFriction.stiffness;
        }


		if (true)
		{
			if (goForwardButton != null && goBackButton != null && turnRightButton != null && 
                turnLeftButton != null && handbrakeButton != null)
			{
				throttle_BTM = goForwardButton.GetComponent<ButtonsTouchManager>();
				reverse_BTM = goBackButton.GetComponent<ButtonsTouchManager>();
				turnLeft_BTM = turnLeftButton.GetComponent<ButtonsTouchManager>();
				turnRight_BTM = turnRightButton.GetComponent<ButtonsTouchManager>();
				handbrake_BTM = handbrakeButton.GetComponent<ButtonsTouchManager>();
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
        extremumSlip = RLCollider.sidewaysFriction.extremumSlip;
        extremumValue = RLCollider.sidewaysFriction.extremumValue;
        asymptoteSlip = RLCollider.sidewaysFriction.asymptoteSlip;
        asymptoteValue = RLCollider.sidewaysFriction.asymptoteValue; 
        stiffness = RLCollider.sidewaysFriction.stiffness;
    }

    void FixedUpdate()
    {
        //CAR DATA

        carSpeed = (2 * Mathf.PI * FLCollider.radius * FLCollider.rpm * 60) / 1000;
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

		#region Car controling code
		if (true)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || throttle_BTM.isPressed)
            {
                //Debug.Log("------GoForward()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoForward();
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || reverse_BTM.isPressed)
            {
                //Debug.Log("------GoReverse()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoReverse();
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || turnLeft_BTM.isPressed)
            {
                //Debug.Log("------TurnLeft()------");
                TurnLeft();
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || turnRight_BTM.isPressed)
            {
                //Debug.Log("------TurnRight()------");
                TurnRight();
            }
            if (Input.GetKey(KeyCode.Space) || handbrake_BTM.isPressed)
            {
                //Debug.Log("------Handbrake()------");
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
            }
            if (!(Input.GetKey(KeyCode.Space) || handbrake_BTM.isPressed))
            {
                //Debug.Log("------RecoverTraction()------");
                //RecoverTraction();
            }
            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.DownArrow) || throttle_BTM.isPressed || reverse_BTM.isPressed))
            {
                //Debug.Log("------ThrottleOff()------");
                ThrottleOff(); 
                //DecelerateCar();
            }
            if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.Space) || throttle_BTM.isPressed ||
                reverse_BTM.isPressed || handbrake_BTM.isPressed || deceleratingCar))
            {
                //Debug.Log("------DecelerateCar()------");
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }
            if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || turnLeft_BTM.isPressed ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || turnRight_BTM.isPressed ||
                steeringAxis == 0f))
            {
                //Debug.Log("------ResetSteeringAngle()------");
                ResetSteeringAngle();
            }
        }
		#endregion
    }


    //STEERING METHODS
    
    public void TurnLeft()
    {
        //Debug.Log("llllllllllllllll");
        //Debug.Log("Time.deltaTime = " + Time.deltaTime);
        steeringAxis -= Time.deltaTime * 10f * steeringSpeed;
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        FLCollider.steerAngle = Mathf.Lerp(FLCollider.steerAngle, steeringAngle, steeringSpeed);
        FRCollider.steerAngle = Mathf.Lerp(FRCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        //Debug.Log("RRRRRRRRRRRRRR");
        //Debug.Log("Time.deltaTime = " + Time.deltaTime);
        steeringAxis += Time.deltaTime * 10f * steeringSpeed;
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        FLCollider.steerAngle = Mathf.Lerp(FLCollider.steerAngle, steeringAngle, steeringSpeed);
        FRCollider.steerAngle = Mathf.Lerp(FRCollider.steerAngle, steeringAngle, steeringSpeed);
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
        if (Mathf.Abs(FLCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        FLCollider.steerAngle = Mathf.Lerp(FLCollider.steerAngle, steeringAngle, steeringSpeed);
        FRCollider.steerAngle = Mathf.Lerp(FRCollider.steerAngle, steeringAngle, steeringSpeed);
    }

	//ENGINE AND BRAKING METHODS

	public void GoForward()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
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
                switch (drive)
                {
                    case driveType.frontWheelDrive:
                        FLCollider.brakeTorque = 0;
                        FLCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        FRCollider.brakeTorque = 0;
                        FRCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        break;
                    case driveType.rearWheelDrive:
                        RLCollider.brakeTorque = 0;
                        RLCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        RRCollider.brakeTorque = 0;
                        RRCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        break;
                    case driveType.allWheelDrive:
                        FLCollider.brakeTorque = 0;
                        FLCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        FRCollider.brakeTorque = 0;
                        FRCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        RLCollider.brakeTorque = 0;
                        RLCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        RRCollider.brakeTorque = 0;
                        RRCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        break;
                    default:
                        Debug.Log("Error case is achieved.");
                        break;
                }
            }
            else
            {
                // If the maxSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                FLCollider.motorTorque = 0;
                FRCollider.motorTorque = 0;
                RLCollider.motorTorque = 0;
                RRCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to -1 smoothly.
        throttleAxis -= (Time.deltaTime * 3f);
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
                // Apply negative torque in all wheels to go in reverse if maxReverseSpeed has not been reached.
                switch (drive)
                {
                    case driveType.frontWheelDrive:
                        FLCollider.brakeTorque = 0;
                        FLCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        FRCollider.brakeTorque = 0;
                        FRCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        break;
                    case driveType.rearWheelDrive:
                        RLCollider.brakeTorque = 0;
                        RLCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        RRCollider.brakeTorque = 0;
                        RRCollider.motorTorque = accelerationMultiplier * 2 * throttleAxis;
                        break;
                    case driveType.allWheelDrive:
                        FLCollider.brakeTorque = 0;
                        FLCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        FRCollider.brakeTorque = 0;
                        FRCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        RLCollider.brakeTorque = 0;
                        RLCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        RRCollider.brakeTorque = 0;
                        RRCollider.motorTorque = accelerationMultiplier * throttleAxis;
                        break;
                    default:
                        Debug.Log("Error case is achieved.");
                        break;
                }
            }
            else
            {
                // If the maxReverseSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxReverseSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                FLCollider.motorTorque = 0;
                FRCollider.motorTorque = 0;
                RLCollider.motorTorque = 0;
                RRCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        switch (drive)
        {
            case driveType.frontWheelDrive:
                FLCollider.motorTorque = 0;
                FRCollider.motorTorque = 0;
                break;
            case driveType.rearWheelDrive:
                RLCollider.motorTorque = 0;
                RRCollider.motorTorque = 0;
                break;
            case driveType.allWheelDrive:
                FLCollider.motorTorque = 0;
                FRCollider.motorTorque = 0;
                RLCollider.motorTorque = 0;
                RRCollider.motorTorque = 0;
                break;
            default:
                Debug.Log("Error case is achieved.");
                break;
        }
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis -= (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis += (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.velocity *= (1f / (1f + (0.025f * decelerationMultiplier)));
        // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
        ThrottleOff();
        // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
        // also cancel the invoke of this method.
        if (carRigidbody.velocity.magnitude < minSpeed)
        {
            carRigidbody.velocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        FLCollider.brakeTorque = brakeForce * distributionBrakingForceOnFrontAxis;
        FRCollider.brakeTorque = brakeForce * distributionBrakingForceOnFrontAxis;
        RLCollider.brakeTorque = brakeForce * (1 - distributionBrakingForceOnFrontAxis);
        RRCollider.brakeTorque = brakeForce * (1 - distributionBrakingForceOnFrontAxis);
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        
        //RLCollider.
        
        // Set brake torque to maximum to lock the wheels
        RLCollider.brakeTorque = Mathf.Infinity;
        RRCollider.brakeTorque = Mathf.Infinity;

        // Set motor torque to 0 to stop the wheels from rotating
        //FLCollider.motorTorque = 0f;
        //FRCollider.motorTorque = 0f;
        RLCollider.motorTorque = 0f;
        RRCollider.motorTorque = 0f;
        
        DriftCarPS();
    }
    // oroginal Handbrake()
    public void Handbrake2()
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
            //Debug.Log("4");
            FLWheelFriction.extremumSlip = FLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            FLCollider.sidewaysFriction = FLWheelFriction;
            //Debug.Log(" - - - - " + FLWheelFriction.extremumSlip);
            FRWheelFriction.extremumSlip = FRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            FRCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RLCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RRCollider.sidewaysFriction = RRWheelFriction;
        }

        // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
        // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
        isTractionLocked = true;
        DriftCarPS();

    }

    // This function is used to emit both the particle systems of the tires' smoke and the trail renderers of the tire skids
    // depending on the value of the bool variables 'isDrifting' and 'isTractionLocked'.
    public void DriftCarPS()
    {

        /*if (useEffects)
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
        }*/

    }

    public void RecoverTraction2()
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
            FLCollider.sidewaysFriction = FLWheelFriction;

            FRWheelFriction.extremumSlip = FRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            FRCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RLCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RRCollider.sidewaysFriction = RRWheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (FLWheelFriction.extremumSlip < FLWExtremumSlip)
        {
            FLWheelFriction.extremumSlip = FLWExtremumSlip;
            FLCollider.sidewaysFriction = FLWheelFriction;

            FRWheelFriction.extremumSlip = FRWExtremumSlip;
            FRCollider.sidewaysFriction = FRWheelFriction;

            RLWheelFriction.extremumSlip = RLWExtremumSlip;
            RLCollider.sidewaysFriction = RLWheelFriction;

            RRWheelFriction.extremumSlip = RRWExtremumSlip;
            RRCollider.sidewaysFriction = RRWheelFriction;

            driftingAxis = 0f;
        }
    }
}