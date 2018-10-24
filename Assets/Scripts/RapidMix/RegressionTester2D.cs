using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionTester2D : MonoBehaviour
{
    RapidMixRegression myRegression;
    public RectTransform myImage;


    void Start()
    {
        myRegression = GetComponent<RapidMixRegression>();
    }

    float rPressTime;
    float spacePressTime;

    void Update()
    {
        // store time of presses
        if( Input.GetKeyDown( KeyCode.R ) )
        {
            rPressTime = Time.time;
        }
        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            spacePressTime = Time.time;
        }

        // record time --> (x, y) while R is pressed
        if( Input.GetKey( KeyCode.R ) )
        {
            float mouseX = Input.mousePosition.x;
            float mouseY = Input.mousePosition.y;
            float elapsedTime = Time.time - rPressTime;
            myRegression.RecordDataPoint( new double[] { elapsedTime }, new double[] { mouseX, mouseY } );
            SetImageLocation( mouseX, mouseY );
        }

        // when r is released, train model
        if( Input.GetKeyUp( KeyCode.R ) )
        {
            myRegression.Train();
        }

        // when space is pressed, use model
        if( Input.GetKey( KeyCode.Space ) )
        {
            float elapsedTime = Time.time - spacePressTime;
            double[] predictedXY = myRegression.Run( new double[] { elapsedTime } );
            SetImageLocation( (float) predictedXY[0], (float) predictedXY[1] );
        }
    }

    private void SetImageLocation( float x, float y )
    {
        // centered on (0,0) = (-10, -10, 0)
        myImage.position = new Vector3( x - 10, y - 10, 0 );
    }
}
