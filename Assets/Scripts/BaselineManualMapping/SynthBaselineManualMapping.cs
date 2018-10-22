using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthBaselineManualMapping : MonoBehaviour
{

    public Transform myController;

    private ParamVectorToSound mySynth;

    void Start()
    {
        mySynth = GetComponent<ParamVectorToSound>();
    }

    // Update is called once per frame
    void Update()
    {
		mySynth.SetParamsDirectly( new float[] {
			// freq: 40 to 2000 (pow)
			myController.position.y.PowMapClamp( 0f, 2f, 40f, 2000f, 2f ),
			// delayMS
			0.05f,
			// timbre LFO: higher = more pitch spread. 0 to 0.9????
			myController.position.z.MapClamp( -1f, 1f, 0.05f, 0.9f ),
			// pitch LFO rate = 0 to 10 or 0 to 400 (pow)
			myController.position.y.PowMapClamp( 0f, 2f, 0.01f, 10f, 1f ),
			// percent depth of pitch to waver: 0 to 0.8 (high exponent -- it sounds good 0.01 to 0.05 and then it becomes something else)
			myController.position.x.PowMapClamp( 0f, 1f, 0.01f, 0.8f, 8f ),
			// HPF cutoff: 20 to 20000 (pow)
			myController.position.y.PowMapClamp( 0.5f, 2f, 20f, 800f, 2f ),
			// LPF cutoff: 20 to 20000 (pow)
			myController.position.y.PowMapClamp( 0f, 1.5f, 800f, 20000f, 2f ),
			// ampMod frequency: 0 to 20 or 0 to 400 (pow)
			myController.position.z.PowMapClamp( -1f, 1f, 0.01f, 10f, 1f ),
			// ampMod depth: 0 to 0.4 (linear)
			myController.position.x.MapClamp( -1f, 1f, 0, 0.4f )
		} );
    }
}
