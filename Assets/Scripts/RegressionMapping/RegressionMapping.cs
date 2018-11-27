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
    public bool useCrossInputs;
    private RapidMixRegression[] myRegressions;

    private List<double[]> myPreviousPresets;
    private int myCurrentPresetIndex;
    private TextMesh myText;
    private RegressionMappingExample intersectingExample = null;

    // TODO: visualize spheres better (I wonder if they could glow the closer you are to them?)
    // TODO: make the deletion process more smooth in appearance

    // Use this for initialization
    void Start()
    {
        mySynth = GetComponent<ParamVectorToSound>();
        myCurrentExample = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        myCurrentInput = new double[] { 0, 0, 0 };

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
        Vector2 touchpadPos = Controller.GetAxis();
        if( Controller.GetPressDown( SteamVR_Controller.ButtonMask.Touchpad ) )
        {
            // first check if we are intersecting
            if( intersectingExample != null && inPlaceExamplesMode )
            {
                if( touchpadPos.y < -0.3f )
                {
                    // delete the example
                    Destroy( intersectingExample.gameObject );
                    GetComponent<ChuckSubInstance>().SetRunning( false );
                    intersectingExample = null;
                }
                else
                {
                    // do nothing
                }
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
            if( intersectingExample != null && inPlaceExamplesMode )
            {
                if( touchpadPos.y < -0.3f )
                {
                    myText.text = "Delete current example";
                    intersectingExample.Highlight( Color.red );
                }
                else
                {
                    // only show the text and do the highlight in examples mode
                    myText.text = "Preview current example";
                    intersectingExample.Highlight( Color.yellow );
                }
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
        else if( intersectingExample != null && inPlaceExamplesMode )
        {
            // only show the text and do the highlight in examples mode
            myText.text = "Preview current example";
            intersectingExample.Highlight( Color.yellow );
        }
        else
        {
            myText.text = "";
        }

        // undo highlight
        if( intersectingExample != null && !inPlaceExamplesMode )
        {
            intersectingExample.ResetHighlight();
        }

        // alpha values
        foreach( RegressionMappingExample example in RegressionMappingExample.GetAllExamples() )
        {
            float sqrDist = ( transform.position - example.transform.position ).sqrMagnitude;
            Debug.Log( sqrDist );
            example.SetActiveness( sqrDist.MapClamp( 0, 2, 1, 0 ) );
        }
    }

    void RunRegression()
    {
        // query regressions for new sample
        myCurrentInput[0] = transform.position.x;
        myCurrentInput[2] = transform.position.z;
        myCurrentInput[1] = transform.position.y;

        // get output
        for( int i = 0; i < myRegressions.Length; i++ )
        {
            if( useCrossInputs )
            {
                myCurrentOutput[i] = myRegressions[i].Run( CrossInput( myCurrentInput ) )[0];
            }
            else
            {
                myCurrentOutput[i] = myRegressions[i].Run( myCurrentInput )[0];
            }
        }

        // play it
        mySynth.SetParams( DoubleToFloat( myCurrentOutput ) );
    }

    void EnterRuntimeMode()
    {
        // for each regression:
        for( int i = 0; i < myCurrentExample.Length; i++ )
        {
            // give it all the input, output pairs [3 -> 1]
            foreach( RegressionMappingExample example in RegressionMappingExample.GetAllExamples() )
            {
                if( useCrossInputs )
                {
                    myRegressions[i].RecordDataPoint( CrossInput( example.GetIn() ), new double[] { example.GetOut()[i] } );
                }
                else
                {
                    myRegressions[i].RecordDataPoint( example.GetIn(), new double[] { example.GetOut()[i] } );
                }
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
        RegressionMappingExample newObject = Instantiate( mappingObjectPrefab, transform.position, Quaternion.identity );
        newObject.Init( DoubleToFloat( myCurrentExample ) );

        // turn off my sound when I place something
        GetComponent<ChuckSubInstance>().SetRunning( false );

        // when an example is spawned, add it to my preset list
        AddPreset( myCurrentExample );
    }

    void AddPreset( double[] preset )
    {
        double[] storedExample = new double[preset.Length];
        for( int i = 0; i < preset.Length; i++ ) { storedExample[i] = preset[i]; }
        myPreviousPresets.Add( storedExample );
        myCurrentPresetIndex = myPreviousPresets.Count;
    }

    double[] CrossInput( double[] input )
    {
        double[] ret = new double[9];
        ret[0] = input[0];
        ret[1] = input[1];
        ret[2] = input[2];
        ret[3] = input[0] * input[0];
        ret[4] = input[0] * input[1];
        ret[5] = input[0] * input[2];
        ret[6] = input[1] * input[1];
        ret[7] = input[1] * input[2];
        ret[8] = input[2] * input[2];
        return ret;
    }

    float[] DoubleToFloat( double[] input )
    {
        float[] output = new float[input.Length];
        for( int i = 0; i < input.Length; i++ ) { output[i] = (float)input[i]; }
        return output;
    }

    bool stillPlayingIntersectingExample = false;
    void OnTriggerEnter( Collider other )
    {
        if( intersectingExample != null ) { return; }
        RegressionMappingExample maybeExample = other.GetComponentInParent<RegressionMappingExample>();
        if( maybeExample != null )
        {
            intersectingExample = maybeExample;
            if( inPlaceExamplesMode )
            {
                mySynth.SetParams( intersectingExample.GetOut() );
                GetComponent<ChuckSubInstance>().SetRunning( true );
            }
        }
    }

    void OnTriggerStay( Collider other )
    {
        if( intersectingExample != null ) { return; }
        RegressionMappingExample maybeExample = other.GetComponentInParent<RegressionMappingExample>();
        if( maybeExample != null )
        {
            intersectingExample = maybeExample;
            if( inPlaceExamplesMode )
            {
                mySynth.SetParams( intersectingExample.GetOut() );
                GetComponent<ChuckSubInstance>().SetRunning( true );
            }
        }
    }

    void OnTriggerExit( Collider other )
    {
        if( intersectingExample == null ) { return; }
        RegressionMappingExample maybeExample = other.GetComponentInParent<RegressionMappingExample>();
        if( maybeExample == intersectingExample )
        {
            // color it back to white
            intersectingExample.ResetHighlight();

            if( inPlaceExamplesMode )
            {
                GetComponent<ChuckSubInstance>().SetRunning( false );
            }

            // forget it
            intersectingExample = null;
        }
    }
}
