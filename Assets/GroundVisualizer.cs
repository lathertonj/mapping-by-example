using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundVisualizer : MonoBehaviour
{
	TextMesh myText;
	MeshRenderer myPlane;
    // Use this for initialization
    void Start()
    {	
		myText = GetComponentInChildren<TextMesh>();
		myPlane = GetComponentInChildren<MeshRenderer>();
    }

    public void EnterExamplesMode()
	{
		myText.text = "Examples Mode";
		myText.color = Color.black;
		myPlane.material.color = Color.white;
	}

	public void EnterRuntimeMode()
	{
		myText.text = "Runtime Mode";
		myText.color = Color.white;
		myPlane.material.color = Color.black;
	}
}
