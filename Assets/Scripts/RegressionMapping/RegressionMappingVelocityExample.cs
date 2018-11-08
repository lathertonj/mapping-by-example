using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingVelocityExample : MonoBehaviour
{
	private static List<RegressionMappingVelocityExample> allExamples;
	private MeshRenderer myRenderer;
	private Color myOriginalColor;
	private List<double[]> myInputs;
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
	}

	// TODO: use angular velocity or not?
	public static double[] GetInput( Vector3 position, Vector3 velocity, Vector3 angularVelocity )
	{
		return new double[] { 
			position.x, position.y, position.z, 
			velocity.x, velocity.y, velocity.z, velocity.magnitude, velocity.sqrMagnitude,
			angularVelocity.x, angularVelocity.y, angularVelocity.z, angularVelocity.magnitude, angularVelocity.sqrMagnitude
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
			for( int i = 0; i < myInputs.Count; i++ )
			{
				transform.position = new Vector3(
					(float) myInputs[i][0], 
					(float) myInputs[i][1],
					(float) myInputs[i][2]
				);
				yield return null;
			}

			// spend 0.5 seconds in last position
			yield return new WaitForSeconds( 0.5f );

			// spend 0.1 seconds in first position
			transform.position = new Vector3(
				(float) myInputs[0][0],
				(float) myInputs[0][1],
				(float) myInputs[0][2]
			);

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
