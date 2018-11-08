using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingWithVelocity : MonoBehaviour
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
    private double[] myCurrentOutput = null;
    public RegressionMappingVelocityExample mappingObjectPrefab;
    private RapidMixRegression[] myRegressions;

    private List<double[]> myPreviousPresets;
    private int myCurrentPresetIndex;
    private TextMesh myText;
    private RegressionMappingVelocityExample intersectingExample = null;
    private RegressionMappingVelocityExample currentlyPlacingExample = null;

    // TODO: visualize spheres better (I wonder if they could glow the closer you are to them?)
    // TODO: make the deletion process more smooth in appearance?

    // Use this for initialization
    void Start()
    {
        mySynth = GetComponent<ParamVectorToSound>();
        myCurrentExample = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        myCurrentOutput = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        myRegressions = GetComponentsInChildren<RapidMixRegression>();


        myPreviousPresets = new List<double[]>();
        myText = GetComponentInChildren<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        // compute
        if( regressionIsRunning )
        {
            RunRegression();
        }

        // record
        if( currentlyPlacingExample != null )
        {
            currentlyPlacingExample.RecordIn( transform.position, Controller.velocity, Controller.angularVelocity );
        }

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
                // EnterRuntimeMode();
                // but do it slowly because training takes a long time
                StartCoroutine( "EnterRuntimeModeSlowly" );
            }
        }

        // trigger
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Trigger ) )
        {
            if( inPlaceExamplesMode )
            {
                SpawnCurrentExample();
            }
        }
        if( Controller.GetPressUp( SteamVR_Controller.ButtonMask.Trigger ) ) 
        {
            if( currentlyPlacingExample != null )
            {
                FinishSpawningCurrentExample();
            }
        }

        // touchpad
        Vector2 touchpadPos = Controller.GetAxis();
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad ) )
        {
            // first check if we are intersecting
            if( intersectingExample != null && touchpadPos.y < -0.3f )
            {
                // delete the example
                Destroy( intersectingExample.gameObject );
                intersectingExample = null;
            }
            else if( inPlaceExamplesMode )
            {
                if( touchpadPos.x < 0 )
                {
                    // cycle through presets
                    PlayNextPreset();
                }
                else
                {
                    // play something new
                    MakeNewExample();
                }
            }
            else
            {
                // in runtime mode, capture the current output as a preset
                AddPreset( myCurrentOutput );
            }
        }

        // touchpad text
        if( touchpadPos != Vector2.zero )
        {
            // first check if we are intersecting
            if( intersectingExample != null && touchpadPos.y < -0.3f )
            {
                myText.text = "Delete current example";
                intersectingExample.Highlight();
            }
            else if( inPlaceExamplesMode )
            {
                if( touchpadPos.x < 0 )
                {
                    // cycle through presets
                    myText.text = "Cycle through presets";
                }
                else
                {
                    // place new random example
                    myText.text = "Generate new random example";
                }
            }
            else
            {
                // runtime mode
                myText.text = "Capture current sound as preset";
            }
        }
        else
        {
            myText.text = "";
        }

        // undo highlight
        if( intersectingExample != null && touchpadPos.y >= -0.3f )
        {
            intersectingExample.ResetHighlight();
        }

        // alpha values
        foreach( RegressionMappingVelocityExample example in RegressionMappingVelocityExample.GetAllExamples() )
        {
            float sqrDist = ( transform.position - example.transform.position ).sqrMagnitude;
            example.SetActiveness( sqrDist.MapClamp( 0, 2, 1, 0 ) );
        }
    }

    void RunRegression()
    {
        // query regressions for new sample
        // get input vector
        double[] input = RegressionMappingVelocityExample.GetInput(
            transform.position, Controller.velocity, Controller.angularVelocity
        );
        // compute output vectors
        for( int i = 0; i < myRegressions.Length; i++ )
        {
            myCurrentOutput[i] = myRegressions[i].Run( input )[0];
        }

        // play it
        mySynth.SetParams( DoubleToFloat( myCurrentOutput ) );
    }

    void EnterRuntimeMode()
    {
        // for each regression:
        for( int i = 0; i < myCurrentExample.Length; i++ )
        {
            // give it all the input, output pairs
            SendTrainingExamples( i );

            // tell it to train
            myRegressions[i].Train();
        }

        // start sonifying
        regressionIsRunning = true;

        // turn back on sound
        GetComponent<ChuckSubInstance>().SetRunning( true );
    }

    IEnumerator EnterRuntimeModeSlowly()
    {
        // for each regression:
        for( int i = 0; i < myCurrentExample.Length; i++ )
        {
            // give it all the input, output pairs
            SendTrainingExamples( i );

            // tell it to train
            myRegressions[i].Train();

            // wait another frame for the next regression
            yield return null;
        }

        // start sonifying
        regressionIsRunning = true;

        // turn back on sound
        GetComponent<ChuckSubInstance>().SetRunning( true );
    }

    void SendTrainingExamples( int which_regression )
    {
        int i = which_regression;

        // give it all the input, output pairs
        foreach( RegressionMappingVelocityExample example in RegressionMappingVelocityExample.GetAllExamples() )
        {
            foreach( double[] input in example.GetInputs() )
            {
                myRegressions[i].RecordDataPoint( input, new double[] { example.GetOut()[i] } );
            }
        }
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

        PlayMyCurrentExample();
    }

    void PlayNextPreset()
    {
        if( myPreviousPresets.Count == 0 )
        {
            return;
        }

        myCurrentPresetIndex--;
        if( myCurrentPresetIndex < 0 )
        {
            myCurrentPresetIndex = myPreviousPresets.Count - 1;
        }

        myCurrentExample = myPreviousPresets[myCurrentPresetIndex];

        PlayMyCurrentExample();
    }

    void PlayMyCurrentExample()
    {
        // start chuck running
        GetComponent<ChuckSubInstance>().SetRunning( true );

        // set params
        mySynth.SetParams( DoubleToFloat( myCurrentExample ) );
    }

    void SpawnCurrentExample()
    {
        currentlyPlacingExample = Instantiate( mappingObjectPrefab, transform.position, Quaternion.identity );
        currentlyPlacingExample.Init( DoubleToFloat( myCurrentExample ) );

        // parent it to me
        currentlyPlacingExample.transform.parent = transform;

        // turn off my sound when I place something
        GetComponent<ChuckSubInstance>().SetRunning( false );

        // when an example is spawned, add it to my preset list
        AddPreset( myCurrentExample );
    }

    void FinishSpawningCurrentExample()
    {
        currentlyPlacingExample.FinishRecording();
        currentlyPlacingExample.transform.parent = null;
        currentlyPlacingExample = null;
    }

    void AddPreset( double[] preset )
    {
        double[] storedExample = new double[preset.Length];
        for( int i = 0; i < preset.Length; i++ ) { storedExample[i] = preset[i]; }
        myPreviousPresets.Add( storedExample );
        myCurrentPresetIndex = myPreviousPresets.Count;
    }

    float[] DoubleToFloat( double[] input )
    {
        float[] output = new float[input.Length];
        for( int i = 0; i < input.Length; i++ ) { output[i] = (float)input[i]; }
        return output;
    }

    void OnTriggerEnter( Collider other )
    {
        if( intersectingExample != null ) { return; }
        RegressionMappingVelocityExample maybeExample = other.GetComponentInParent<RegressionMappingVelocityExample>();
        if( maybeExample != null )
        {
            intersectingExample = maybeExample;
        }
    }

    void OnTriggerStay( Collider other )
    {
        if( intersectingExample != null ) { return; }
        RegressionMappingVelocityExample maybeExample = other.GetComponentInParent<RegressionMappingVelocityExample>();
        if( maybeExample != null )
        {
            intersectingExample = maybeExample;
        }
    }

    void OnTriggerExit( Collider other )
    {
        if( intersectingExample == null ) { return; }
        RegressionMappingVelocityExample maybeExample = other.GetComponentInParent<RegressionMappingVelocityExample>();
        if( maybeExample == intersectingExample )
        {
            // color it back to white
            intersectingExample.ResetHighlight();

            // forget it
            intersectingExample = null;
        }
    }
}
