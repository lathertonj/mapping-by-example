using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthBaselineVolumeMappingController : MonoBehaviour
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
	private float[] myCurrentExample = null;
	public SynthBaselineVolumeMapping mappingObjectPrefab;

    // Use this for initialization
    void Start()
    {
		mySynth = GetComponent<ParamVectorToSound>();
		myCurrentExample = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
				SynthBaselineVolumeMapping.TurnOff();
			}
			else
			{
				// switch to runtime mode
				SynthBaselineVolumeMapping.TurnOn();
				GetComponent<ChuckSubInstance>().SetRunning( false );
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
		mySynth.SetParams( myCurrentExample );
	}

	void SpawnCurrentExample()
	{
		SynthBaselineVolumeMapping newObject = Instantiate( mappingObjectPrefab, transform.position, Quaternion.identity );
		newObject.Init( myCurrentExample );

		// turn off my sound when I place something
		GetComponent<ChuckSubInstance>().SetRunning( false );
	}
}
