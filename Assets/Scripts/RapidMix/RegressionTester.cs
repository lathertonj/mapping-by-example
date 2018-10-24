using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionTester : MonoBehaviour
{
    RapidMixRegression myRegression;
    public RectTransform myImage;


    void Start()
    {
        myRegression = GetComponent<RapidMixRegression>();
    }

    void Update()
    {
        // record x --> y while R is pressed
        if( Input.GetKey( KeyCode.R ) )
        {
            float mouseX = Input.mousePosition.x;
            float mouseY = Input.mousePosition.y;
            myRegression.RecordDataPoint( new double[] { mouseX }, new double[] { mouseY } );
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
            float mouseX = Input.mousePosition.x;
            float predictedY = (float) myRegression.Run( new double[] { mouseX } )[0];
            SetImageLocation( mouseX, predictedY );
        }
    }

    private void SetImageLocation( float x, float y )
    {
        // centered on (0,0) = (-10, -10, 0)
        myImage.position = new Vector3( x - 10, y - 10, 0 );
    }
}
