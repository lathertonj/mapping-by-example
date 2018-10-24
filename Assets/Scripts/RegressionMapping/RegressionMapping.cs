using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMapping : MonoBehaviour
{
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input( (int)trackedObj.index ); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private ParamVectorToSound mySynth;
    private bool inPlaceExamplesMode = true;
    private bool regressionIsRunning = false;
    private double[] myCurrentExample = null;
    private double[] myCurrentInput = null;
    private double[] myCurrentOutput = null;
    public RegressionMappingExample mappingObjectPrefab;
    private RapidMixRegression[] myRegressions;

    // TODO: save the examples you currently have placed and enable you to arrow back through them
    // TODO: enable you to capture new examples from runtime mode
    // TODO: enable you to delete examples from the world
    // TODO: visualize spheres better (I wonder if they could glow the closer you are to them?)
    // TODO: try other models besides the specific regression model I'm using?

    // Use this for initialization
    void Start()
    {
        mySynth = GetComponent<ParamVectorToSound>();
        myCurrentExample = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        myCurrentInput = new double[] { 0, 0, 0 };
        myCurrentOutput = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        myRegressions = GetComponentsInChildren<RapidMixRegression>();
    }

    // Update is called once per frame
    void Update()
    {
        // grip 
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Grip ) )
        {
            inPlaceExamplesMode = !inPlaceExamplesMode;
            if( inPlaceExamplesMode )
            {
                // switch to placing examples mode
                EnterExamplesMode();

            }
            else
            {
                // switch to runtime mode
                EnterRuntimeMode();
            }
        }

        // trigger
        if( Controller.GetPressUp( SteamVR_Controller.ButtonMask.Trigger ) )
        {
            if( inPlaceExamplesMode )
            {
                SpawnCurrentExample();
            }
        }

        // touchpad
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad ) )
        {
            if( inPlaceExamplesMode )
            {
                MakeNewExample();
            }
        }

        if( regressionIsRunning )
        {
            RunRegression();
        }
    }

    void RunRegression()
    {
        // query regressions for new sample
        myCurrentInput[0] = transform.position.x;
        myCurrentInput[1] = transform.position.y;
        myCurrentInput[2] = transform.position.z;

        // get output
        for( int i = 0; i < myRegressions.Length; i++ )
        {
            myCurrentOutput[i] = myRegressions[i].Run( myCurrentInput )[0];
        }

        // play it
        mySynth.SetParams( DoubleToFloat( myCurrentOutput ) );
    }

    void EnterRuntimeMode()
    {
        // TODO: reset data given to each algorithm. need to make a new empty training data? or empty the one we have.

        // for each regression:
        for( int i = 0; i < myCurrentExample.Length; i++ )
        {
            // give it all the input, output pairs [3 -> 1]
            foreach( RegressionMappingExample example in RegressionMappingExample.GetAllExamples() )
            {
                myRegressions[i].RecordDataPoint( example.GetIn(), new double[] { example.GetOut()[i] } );
            }

            // tell it to train
            myRegressions[i].Train();
        }

        // start sonifying
        regressionIsRunning = true;

        // turn back on sound
        GetComponent<ChuckSubInstance>().SetRunning( true );
    }

    void EnterExamplesMode()
    {
        // stop sonifying
        regressionIsRunning = false;

        // momentarily turn off sound
        GetComponent<ChuckSubInstance>().SetRunning( false );

        // reset all regressions
        foreach( RapidMixRegression regression in myRegressions )
        {
            regression.ResetRegression();
        }
    }

    void MakeNewExample()
    {
        for( int i = 0; i < myCurrentExample.Length; i++ )
        {
            myCurrentExample[i] = Random.Range( 0f, 1f );
        }

        // start chuck running
        GetComponent<ChuckSubInstance>().SetRunning( true );

        // set params
        mySynth.SetParams( DoubleToFloat( myCurrentExample ) );
    }

    void SpawnCurrentExample()
    {
        RegressionMappingExample newObject = Instantiate( mappingObjectPrefab, transform.position, Quaternion.identity );
        newObject.Init( DoubleToFloat( myCurrentExample ) );

        // turn off my sound when I place something
        GetComponent<ChuckSubInstance>().SetRunning( false );
    }

    float[] DoubleToFloat( double[] input )
    {
        float[] output = new float[input.Length];
        for( int i = 0; i < input.Length; i++ ) { output[i] = (float)input[i]; }
        return output;
    }
}
