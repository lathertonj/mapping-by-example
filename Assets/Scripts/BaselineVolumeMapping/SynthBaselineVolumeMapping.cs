using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthBaselineVolumeMapping : MonoBehaviour
{

	private static List<SynthBaselineVolumeMapping> allSounds;

	bool needToUpdateParams = false;
	float[] paramsToUpdate = new float[9];

	private void Awake()
	{
		if( allSounds == null )
		{
			allSounds = new List<SynthBaselineVolumeMapping>();
		}
		allSounds.Add( this );
	}
    public void Init( float[] zeroOneParams )
    {
		TurnOffIndividual();
        for( int i = 0; i < zeroOneParams.Length; i++ ) { paramsToUpdate[i] = zeroOneParams[i]; }
		needToUpdateParams = true;
    }

	private void Update()
	{
		// do this in Update so it happens after Start() for sure.
		if( needToUpdateParams )
		{
			needToUpdateParams = false;
			GetComponent<ParamVectorToSound>().SetParams( paramsToUpdate );
		}
	}

	private void TurnOnIndividual()
	{
		GetComponent<ChuckSubInstance>().SetRunning( true );
	}

	private void TurnOffIndividual()
	{
		GetComponent<ChuckSubInstance>().SetRunning( false );
	}

    public static void TurnOn()
    {
		foreach( SynthBaselineVolumeMapping sound in allSounds )
		{
			sound.TurnOnIndividual();
		}
    }

	public static void TurnOff()
    {
		foreach( SynthBaselineVolumeMapping sound in allSounds )
		{
			sound.TurnOffIndividual();
		}
    }
}
