using UnityEngine;
using System;

public class carController_copy : MonoBehaviour{

    internal enum driveType{
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField]private driveType drive;

    //components:
    [HideInInspector]public inputs input;
    [HideInInspector]public Rigidbody rigidbody;

    public float torque = 100 , maxRPM , minRPM;
    public AnimationCurve enginePower;
    public float[] gears;
    public float finalDrive = 3.7f ;
    public int gearNum;
    [Range(0.2f,0.8f)]public float EngineSmoothTime;
    [HideInInspector]public float engineRPM , engineLoad ,DownForceValue  ,dragAmount , KPH;
    public WheelCollider[] wheelColliders;
    [HideInInspector]public float ForwardStifness;
    [HideInInspector]public float SidewaysStifness;
    public Transform[] wheelTransforms;

    private Vector3 wheelPosition;
    private Quaternion wheelRotation;

    //junk
    [HideInInspector]public bool vehicleChecked = false;
    private float vertical , horizontal;
    private float finalTurnAngle , radius;
    private float wheelsRPM , acceleration , totalPower , gearChangeRate;
    private float engineLerpValue , brakePower ;
	private WheelFrictionCurve  forwardFriction,sidewaysFriction;
    private float[] wheelSlip;
    private bool engineLerp;
    private bool reverse , grounded;
    
    public float currentVelocity , lastFrameVelocity , Gforce;

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

    
    void Start(){
        findValues();
        
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
  
    void Update(){

        currentVelocity=rigidbody.velocity.magnitude;


        moveCar();
        updateWheels();

    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        currentVelocity = rigidbody.velocity.magnitude;
        Gforce = (currentVelocity - lastFrameVelocity) / (Time.deltaTime * Physics.gravity.magnitude);
        lastFrameVelocity = currentVelocity;
    }


    void findValues(){
        print("tt");

        //get components:
        input = GetComponent<inputs>();
        rigidbody = GetComponent<Rigidbody>();

        wheelSlip = new float[wheelColliders.Length];
        vehicleChecked = true;

    }

    void moveCar(){

        runEngine();
        steerVehicle();

    }

    void runEngine(){
        lerpEngine();
        wheelRPM();
        manual();

        acceleration = vertical > 0 ?  vertical : wheelsRPM <= 1 ? vertical  : 0 ;
        
        if(!isGrounded()){
            acceleration = engineRPM > 1000 ? acceleration / 2 : acceleration; 
        }


        if(engineRPM >= maxRPM){
            setEngineLerp(maxRPM - 1000);
        }
        if(!engineLerp){
            engineRPM = Mathf.Lerp(engineRPM,1000f + Mathf.Abs(wheelsRPM) *  finalDrive *  (gears[gearNum]) , (EngineSmoothTime * 10) * Time.deltaTime);
            totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum] * finalDrive ) * acceleration  ;
        }
        
        engineLoad = Mathf.Lerp(engineLoad,vertical - ((engineRPM - 1000) / maxRPM ),(EngineSmoothTime * 10) * Time.deltaTime);
        runCar();
    }

    void runCar(){
        if(drive == driveType.rearWheelDrive){
            for (int i = 2; i < wheelColliders.Length; i++){
                wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2) ;
            }
        }
        else if(drive == driveType.frontWheelDrive){
            for (int i = 0; i < wheelColliders.Length - 2; i++){
                wheelColliders[i].motorTorque =  (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2) ;
            }
        }
        else{
            for (int i = 0; i < wheelColliders.Length; i++){
                wheelColliders[i].motorTorque =  (vertical == 0) ? 0 : totalPower / wheelColliders.Length;
            }
        }


        for (int i = 0; i < wheelColliders.Length; i++){
            if(KPH <= 1 && KPH >= -1 && vertical == 0){
                brakePower = 5;
            } else{
                if(vertical < 0 && KPH > 1 && !reverse)
                    brakePower =  (wheelSlip[i] <= 0.3f) ? brakePower + -vertical * 100 : brakePower > 0 ? brakePower  + vertical * 50 : 0 ;
                else 
                    brakePower = 0;
            }
            wheelColliders[i].brakeTorque = brakePower;
        }

        if (Input.GetKey(KeyCode.Space) || handbrake_BTM.isPressed)
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = Mathf.Infinity;
        else
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = 0;

        rigidbody.angularDrag = (KPH > 100)? KPH / 100 : 0;
        rigidbody.drag = dragAmount + (KPH / 40000) ;

        KPH = rigidbody.velocity.magnitude * 3.6f;
        friction();
    }

    void steerVehicle(){

        vertical = input.vertical;
        horizontal = Mathf.Lerp(horizontal , input.horizontal , (input.horizontal != 0) ? 2 * Time.deltaTime : 3 * 2 * Time.deltaTime);

        finalTurnAngle = (radius > 5 ) ? radius : 5  ;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || turnRight_BTM.isPressed)
        {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || turnLeft_BTM.isPressed)
        {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
        }
        /*if (horizontal > 0 ) {
				//rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
        } else if (horizontal < 0 ) {                                                          
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
			//transform.Rotate(Vector3.up * steerHelping);

        } else {
            wheelColliders[0].steerAngle =0;
            wheelColliders[1].steerAngle =0;
        }*/

    }

    void updateWheels(){

        for (int i = 0; i < wheelColliders.Length; i++) {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelTransforms[i].transform.rotation = wheelRotation;
            wheelTransforms[i].transform.position = wheelPosition;
        }
    }

    void wheelRPM(){
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheelColliders[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
 
        if(wheelsRPM < 0 && !reverse ){
            reverse = true;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
        else if(wheelsRPM > 0 && reverse){
            reverse = false;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
    }

    void setEngineLerp(float num){
        engineLerp = true;
        engineLerpValue = num;
    }

    void lerpEngine(){
        if(engineLerp){
            totalPower = 0;
            engineRPM = Mathf.Lerp(engineRPM,engineLerpValue,20 * Time.deltaTime );
            engineLerp = engineRPM <= engineLerpValue + 100 ? false : true;
        }
    }   

    bool isGrounded(){
        if(wheelColliders[0].isGrounded &&wheelColliders[1].isGrounded &&wheelColliders[2].isGrounded &&wheelColliders[3].isGrounded )
            return true;
        else
            return false;
    }

    void manual(){

        if((Input.GetAxis("Fire2") == 1  ) && gearNum <= gears.Length && Time.time >= gearChangeRate ){
            gearNum  = gearNum +1;
            gearChangeRate = Time.time + 1f/3f ;
            setEngineLerp(engineRPM - ( engineRPM > 1500 ? 2000 : 700));
            //audio.DownShift();

        }
        if((Input.GetAxis("Fire3") == 1 ) && gearNum >= 1  && Time.time >= gearChangeRate){
            gearChangeRate = Time.time + 1f/3f ;
            gearNum --;
            setEngineLerp(engineRPM - ( engineRPM > 1500 ? 1500 : 700));
            //audio.DownShift();
        }
    
    }

    void friction(){
    
        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheelColliders.Length];
        for (int i = 0; i < wheelColliders.Length ; i++){
            if(wheelColliders[i].GetGroundHit(out hit) && i >= 2 ){
                //forwardFriction = wheelColliders[i].forwardFriction;
                //forwardFriction.stiffness = (input.handbrake)?  .55f : ForwardStifness; 
                //wheelColliders[i].forwardFriction = forwardFriction;

                //sidewaysFriction = wheelColliders[i].sidewaysFriction;
                //sidewaysFriction.stiffness = (input.handbrake)? .55f : SidewaysStifness;
                //wheelColliders[i].sidewaysFriction = sidewaysFriction;
                
                grounded = true;

                sum += Mathf.Abs(hit.sidewaysSlip);

            }
            else grounded = false;

            wheelSlip[i] = Mathf.Abs( hit.forwardSlip ) + Mathf.Abs(hit.sidewaysSlip) ;
            sidewaysSlip[i] = Mathf.Abs(hit.sidewaysSlip);


        }

        sum /= wheelColliders.Length - 2 ;
        radius = (KPH > 60) ?  4 + (sum * -25) + KPH / 8 : 4;
        
    }

    void OnGUI(){

        float pos = 50;

        GUI.Label(new Rect(20, pos, 200, 20),"currentGear: " + gearNum.ToString("0"));
        pos+=25f;
        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineRPM,0,maxRPM);
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"Torque: " + totalPower.ToString("0"));
        pos+=25f;
        GUI.Label(new Rect(20, pos, 200, 20),"KPH: " + KPH.ToString("0"));
        pos+=25f;
        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineLoad, 0.0F, 1.0F);
        pos+=25f;
        GUI.Label(new Rect(20, pos, 200, 20),"brakes: " + brakePower.ToString("0"));
        pos+=25f;
        GUI.Label(new Rect(20, pos, 200, 20),"currentVelocity: " + currentVelocity.ToString("0"));
        pos+=25f;
        GUI.Label(new Rect(20, pos, 200, 20),"lastFrameVelocity: " + lastFrameVelocity.ToString("0"));
        pos+=25f;
        GUI.Label(new Rect(20, pos, 200, 20),"Gforce: " + Gforce.ToString("0"));
        pos+=25f;
        
    }

}
