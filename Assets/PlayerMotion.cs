using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotion : MonoBehaviour {
    [SerializeField]
    private GameObject OVRPlayerControllerGameObject = null;
    [SerializeField]
    private Transform LeftHandAnchorTransform = null;
    [SerializeField]
    private Transform RightHandAnchorTransform = null;
    private OVRPlayerController OVRPlayerControllerComponent;

    //identical to fields of OVRPlayerController class
    private CharacterController Controller;
    private Vector3 MoveThrottle = Vector3.zero;
    private float MoveScale = 1.0f;
    private float MoveScaleMultiplier = 1.0f;
    private float SimulationRate = 60f;
    private float FallSpeed = 0.0f;
    private float Acceleration;
    private float Damping;
    private float GravityModifier;
    private float JumpForce;

    // original fields for this script
    private Vector3 touchVelocityL;
    private Vector3 touchVelocityR;
    private Vector3 touchAccelerationL;
    private Vector3 touchAccelerationR;
    private bool motionInertia = false;
    private float motionInertiaDuration = 1.0f;

    const float WALK_THRESHOLD = 0.1f;
    const float RUN_THRESHOLD = 0.1f;
    const float JUMP_THRESHOLD = 1.5f;

    //Start()関数より先に呼び出される
    private void Awake( ) {
        Controller = OVRPlayerControllerGameObject.GetComponent<CharacterController>( );
        OVRPlayerControllerComponent = OVRPlayerControllerGameObject.GetComponent<OVRPlayerController>( );
    }
    // Start is called before the first frame update
    private void Start( ) {
        // store public fields of OVRPlayerController-class to local private fileds
        Acceleration = OVRPlayerControllerComponent.Acceleration;
        Damping = OVRPlayerControllerComponent.Damping;
        GravityModifier = OVRPlayerControllerComponent.GravityModifier;
        JumpForce = OVRPlayerControllerComponent.JumpForce;

        // pre-setting for overriding character-control
        OVRPlayerControllerComponent.PreCharacterMove += ( ) => CharacterMoveByHandShake( );
        OVRPlayerControllerComponent.EnableLinearMovement = false;
        // necessary for initial grounded-evaluation
        Controller.Move( Vector3.zero * Time.deltaTime );
    }

    // Update is called once per frame
    void Update( ) {
      
    }

    private void CharacterMoveByHandShake( ) {
        HandShakeController( );
        UpdateController( );

        // display for development purpose
        Debug.Log( "L-touch velocity: " + touchVelocityL );
        Debug.Log( "R-touch velocity: " + touchVelocityR );
    }

    private void HandShakeController( ) {
        //OVRInput.GetLocalControllerVelocity=手を上に3上げた時は、Y方向に3進んだ値が取得される
        //速度に関わらず動かした分だけ進む( 例えば壁を手を使ってよじ登る等 )のに使う事が出来る
        touchVelocityL = OVRInput.GetLocalControllerVelocity( OVRInput.Controller.LTouch );
        touchVelocityR = OVRInput.GetLocalControllerVelocity( OVRInput.Controller.RTouch );
        //OVRInput.GetLocalControllerAcceleration=例えば手を上に3上げた時、ゆっくり上げればY方向に0～1と小さな加速値が取得されるが勢い付けて早く上げると10以上の大きな加速値が取得できる
        //加速量を使用すると歩いているのか手を動かしているだけなのかを使い分ける事が出来る
        touchAccelerationL = OVRInput.GetLocalControllerAcceleration( OVRInput.Controller.LTouch );
        touchAccelerationR = OVRInput.GetLocalControllerAcceleration( OVRInput.Controller.RTouch );

        if( !IsGrounded( ) )
            MoveScale = 0.0f;
        else
            MoveScale = 1.0f;

        MoveScale *= SimulationRate * Time.deltaTime;

        float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

        Transform activeHand;
        Vector3 handShakeVel;
        Vector3 handShakeAcc;

        if( Math.Abs( touchVelocityL.y ) > Math.Abs( touchVelocityR.y ) ) {
            activeHand = LeftHandAnchorTransform;
            handShakeVel = touchVelocityL;
            handShakeAcc = touchAccelerationL;
        } else {
            activeHand = RightHandAnchorTransform;
            handShakeVel = touchVelocityR;
            handShakeAcc = touchAccelerationR;
        }

        Quaternion ort = activeHand.rotation;
        Vector3 ortEuler = ort.eulerAngles;
        ortEuler.z = ortEuler.x = 0f;
        ort = Quaternion.Euler( ortEuler );



        MoveThrottle += CalculateMoveEffect( moveInfluence, ort, handShakeVel, handShakeAcc );
    }

    private Vector3 CalculateMoveEffect( float moveInsfluence, Quaternion ort, Vector3 handShakeVel, Vector3 handShakeAcc ) {
        Vector3 tmpMoveThrottle = Vector3.zero;

        bool isWalk = DetectHandShakeWalk( Math.Abs( handShakeVel.y ) ) || motionInertia;
        if( isWalk ) {
            if( !motionInertia )
                SetMotionInertia( );

            tmpMoveThrottle += ort * ( OVRPlayerControllerGameObject.transform.lossyScale.z * moveInsfluence * Vector3.forward ) * 0.2f;

            bool isRun = DetectHandShakeRun( Math.Abs( handShakeVel.y ) );
            if( isRun )
                tmpMoveThrottle *= 2.0f;
        }
        bool isJump = DetectHandShakeJump( );
        if( isJump )
            tmpMoveThrottle += new Vector3( 0.0f, JumpForce, 0.0f );

        return tmpMoveThrottle;
    }

    //IEnumerator は反復処理をサポートするインターフェース
    IEnumerator SetMotionInertia( ) {
        motionInertia = true;
        //スケール化されていない時間を使用して、指定した秒数の間コルーチンの実行を一時停止させる
        yield return new WaitForSecondsRealtime( motionInertiaDuration );
        motionInertia = false;
    }

    private bool DetectHandShakeWalk( float speed ) {
        if( !IsGrounded( ) )
            return false;
        if( speed > WALK_THRESHOLD )
            return true;
        return false;
    }

    private bool DetectHandShakeRun( float speed ) {
        if( !IsGrounded( ) )
            return false;
        if( speed > RUN_THRESHOLD )
            return true;
        return false;
    }

    private bool DetectHandShakeJump( ) {
        if( !IsGrounded( ) )
            return false;
        if( touchVelocityL.y > JUMP_THRESHOLD && touchVelocityR.y > JUMP_THRESHOLD )
            return true;
        return false;
    }

    private bool IsGrounded( ) {
        if( Controller.isGrounded )
            return true;

        var pos = OVRPlayerControllerGameObject.transform.position;
        var ray = new Ray( pos + Vector3.up * 0.1f, Vector3.down );
        var tolerance = 0.3f;
        return Physics.Raycast( ray, tolerance );
    }

    private void UpdateController(){ 
            Vector3 moveDirection= Vector3.zero;
        float motorDamp=1.0f+(Damping*SimulationRate*Time.deltaTime);

        MoveThrottle.x/=motorDamp;
        MoveThrottle.y=(MoveThrottle.y>0.0f)?
            (MoveThrottle.y/motorDamp):MoveThrottle.y;
        MoveThrottle.z/=motorDamp;

        moveDirection+=MoveThrottle*SimulationRate*Time.deltaTime;

        // calculate gravity influence
        if( Controller.isGrounded&&FallSpeed<=0 ) 
            FallSpeed=Physics.gravity.y*(GravityModifier*0.002f);
        else
            FallSpeed+=Physics.gravity.y*(GravityModifier*0.002f)*SimulationRate*Time.deltaTime;

        moveDirection.y+=FallSpeed*SimulationRate*Time.deltaTime;

        if( Controller.isGrounded&&MoveThrottle.y<=OVRPlayerControllerGameObject.transform.lossyScale.y*0.001f ) {
            // offset correction for uneven ground
            float bumpUpOffset=Mathf.Max(Controller.stepOffset,new Vector3(moveDirection.x,0,moveDirection.z).magnitude);
            moveDirection-=bumpUpOffset*Vector3.up;
        }
        Vector3 predictedXZ=Vector3.Scale(Controller.transform.localPosition+moveDirection,new Vector3(1,0,1));

        // update character position
        Controller.Move(moveDirection);

        Vector3 actualXZ=Vector3.Scale(Controller.transform.localPosition,new Vector3(1,0,1));

        if(predictedXZ!=actualXZ)
            MoveThrottle+=(actualXZ-predictedXZ)/(SimulationRate*Time.deltaTime);
    }
}

