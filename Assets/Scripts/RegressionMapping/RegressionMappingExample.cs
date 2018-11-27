using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionMappingExample : MonoBehaviour
{
	private static List<RegressionMappingExample> allExamples;
	private MeshRenderer myRenderer;
	private Color myOriginalColor;
	private void Awake()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingExample>();
		}
		allExamples.Add( this );

		myRenderer = GetComponentInChildren<MeshRenderer>();
		myOriginalColor = myRenderer.material.color;
	}

	public static List<RegressionMappingExample> GetAllExamples()
	{
		if( allExamples == null )
		{
			allExamples = new List<RegressionMappingExample>();
		}
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
