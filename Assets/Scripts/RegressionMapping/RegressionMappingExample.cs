using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingExample : MonoBehaviour
{
	private static List<RegressionMappingExample> allExamples;
	private MeshRenderer myRenderer;
	private void Awake()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingExample>();
		}
		allExamples.Add( this );

		myRenderer = GetComponentInChildren<MeshRenderer>();
	}

	public static List<RegressionMappingExample> GetAllExamples()
	{
		return allExamples;
	}

	float[] myOut;
	public void Init( float[] outVec )
	{
        myOut = outVec;
	}
    
	public double[] GetIn()
	{
		return new double[] { transform.position.x, transform.position.y, transform.position.z };;
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
		myRenderer.material.color = Color.white;
	}

	void OnDestroy()
	{
		allExamples.Remove( this );
	}

}
