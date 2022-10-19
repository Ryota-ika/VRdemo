using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class player_Controller : MonoBehaviour {
    // Start is called before the first frame update
    void Start( ) {

    }

    // Update is called once per frame
    void Update( ) {
        //右アナログ値取得
        var right_Stick = OVRInput.Get( OVRInput.Axis2D.SecondaryThumbstick ).x;
        transform.Rotate( 0, right_Stick * 2f, 0 );
        //左アナログ値取得
        var left_Stick = OVRInput.Get( OVRInput.Axis2D.PrimaryThumbstick ).x;
        transform.Rotate( 0, left_Stick * 2f, 0 );
    }
}
