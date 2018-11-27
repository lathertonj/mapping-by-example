using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingVelocityExample : MonoBehaviour
{
	private static List<RegressionMappingVelocityExample> allExamples;
	private MeshRenderer myRenderer;
	private Color myOriginalColor;
	private List<double[]> myInputs;
	private List<Vector3> myPositions;
	private void Awake()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingVelocityExample>();
		}
		allExamples.Add( this );

		myRenderer = GetComponentInChildren<MeshRenderer>();
		myOriginalColor = myRenderer.material.color;
		myInputs = new List<double[]>();
		myPositions = new List<Vector3>();
	}

	public static List<RegressionMappingVelocityExample> GetAllExamples()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingVelocityExample>();
		}
		return allExamples;
	}

	float[] myOut;
	public void Init( float[] outVec )
	{
        myOut = outVec;
	}

	public void RecordIn( Vector3 position, Vector3 velocity, Vector3 angularVelocity )
	{
		myInputs.Add( GetInput( position, velocity, angularVelocity ) );
		myPositions.Add( position );
	}

	// TODO: which of these to actually use?
	public static double[] GetInput( Vector3 position, Vector3 velocity, Vector3 angularVelocity )
	{
		return new double[] { 
			// position.x, position.y, position.z, 
			velocity.x, velocity.y, velocity.z, velocity.magnitude, velocity.sqrMagnitude,
			// angularVelocity.x, angularVelocity.y, angularVelocity.z, angularVelocity.magnitude, angularVelocity.sqrMagnitude
		};
	}

	public void FinishRecording()
	{
		// called when we've stopped recording input points. now, we should start animating
		StartCoroutine( "AnimateSelf" );
	}

	// animate self
	IEnumerator AnimateSelf()
	{
		while( true )
		{
			// animate, one position per frame
			for( int i = 0; i < myPositions.Count; i++ )
			{
				transform.position = myPositions[i];
				yield return null;
			}

			// spend 0.5 seconds in last position
			yield return new WaitForSeconds( 0.5f );

			// spend 0.1 seconds in first position
			transform.position = myPositions[0];

			yield return new WaitForSeconds( 0.1f );
		}
	} 

	public List<double[]> GetInputs()
	{
		return myInputs;
	}

	public float[] GetOut()
	{
		return myOut;
	}

	public void Highlight()
	{
		myRenderer.material.color = Color.red;
	}

	public void Highlight( Color c )
	{
		myRenderer.material.color = c;
	}

	public void ResetHighlight()
	{
		myRenderer.material.color = myOriginalColor;
	}

	public void SetActiveness( float activeness )
	{
		activeness = Mathf.Clamp01( activeness );
		Color c = myRenderer.material.color;
		c.a = activeness;
		myRenderer.material.color = c;
	}

	void OnDestroy()
	{
		allExamples.Remove( this );
	}

}
