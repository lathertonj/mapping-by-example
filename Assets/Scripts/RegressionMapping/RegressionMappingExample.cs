using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingExample : MonoBehaviour
{
	private static List<RegressionMappingExample> allExamples;
	private void Awake()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingExample>();
		}
		allExamples.Add( this );
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
}
