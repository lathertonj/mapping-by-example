using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamVectorToSound : MonoBehaviour {

	private ChuckSubInstance myChuck;
	private string myFreq, myDelaySeconds, myTimbreLFO, myPitchLFORate,
	 	myPitchLFODepthMultiplier, myHPFFreq, myLPFFreq,
		myAmpModLFOFreq, myAmpModDepth;
	private string myTurnOffEvent, myTurnOnEvent;

	// Use this for initialization
	void Start () {
		myChuck = GetComponent<ChuckSubInstance>();
		myFreq = myChuck.GetUniqueVariableName();
		myDelaySeconds = myChuck.GetUniqueVariableName();
		myTimbreLFO = myChuck.GetUniqueVariableName();
		myPitchLFORate = myChuck.GetUniqueVariableName();
	 	myPitchLFODepthMultiplier = myChuck.GetUniqueVariableName();
		myHPFFreq = myChuck.GetUniqueVariableName();
		myLPFFreq = myChuck.GetUniqueVariableName();
		myAmpModLFOFreq = myChuck.GetUniqueVariableName();
		myAmpModDepth = myChuck.GetUniqueVariableName();
        myTurnOnEvent = myChuck.GetUniqueVariableName();
        myTurnOffEvent = myChuck.GetUniqueVariableName();

		myChuck.RunCode( string.Format( @"
            // using a carrier wave (saw oscillator for example,) 
            // and modulating its signal using a comb filter 
            // where the filter cutoff frequency is usually 
            // modulated with an LFO, which the LFO's depth (or amplitude) 
            // is equal to the saw oscillator's current frequency. 
            // It can also be done by using a copied signal and 
            // have the copy run throught a delay which the 
            // delay's time is modulated again by an LFO where the 
            // LFO's depth is equal to the saw oscillator's current frequency.
            class Supersaw extends Chubgraph
            {{
                SawOsc osc => Gain out => outlet;
                5 => int numDelays;
                1.0 / (1 + numDelays) => out.gain;
                DelayA theDelays[numDelays];
                SinOsc lfos[numDelays];
                dur baseDelays[numDelays];
                float baseFreqs[numDelays];
                for( int i; i < numDelays; i++ )
                {{
                    osc => theDelays[i] => out;
                    0.15::second => theDelays[i].max;
                    // crucial to modify!
                    Math.random2f( 0.001, 0.002 )::second => baseDelays[i];
                    lfos[i] => blackhole;
            //        Math.random2f( -0.1, 0.1) => baseFreqs[i];
                    Math.random2f( 0, pi ) => lfos[i].phase;
        
                }}
                0.05::second => dur baseDelay;
                0.333 => float baseFreq;
                1 => float lfoGain;
    
                fun void AttachLFOs()
                {{
                    while( true )
                    {{
                        for( int i; i < numDelays; i++ )
                        {{
                            baseFreq + baseFreqs[i] => lfos[i].freq;
                            lfoGain * lfos[i].last()::second + baseDelay + baseDelays[i] 
                                => theDelays[i].delay;
                        }}
                        1::ms => now;
                    }}
                }}
                spork ~ AttachLFOs();
    
    
                SinOsc pitchLFO => blackhole;
                0.77 => pitchLFO.freq;
                1.0 / 300 => float pitchLFODepth;
                440 => float basePitch;
    
                fun void FreqMod()
                {{
                    while( true )
                    {{
                        // calc freq
                        basePitch + ( basePitch * pitchLFODepth ) 
                            * pitchLFO.last() => float f;
                        // set
                        f => osc.freq;
                        1.0 / f => lfoGain; // seconds per cycle == gain amount
                        // wait
                        1::ms => now;
                    }}
                }}
                spork ~ FreqMod();
    
                fun void freq( float f )
                {{
                    f => basePitch;
                }}
    
                fun void delay( dur d )
                {{
                    d => baseDelay;
                }}
    
                fun void timbreLFO( float f )
                {{
                    f => baseFreq;
                }}
    
                fun void pitchLFORate( float f )
                {{
                    f => pitchLFO.freq;
                }}
    
                fun void pitchLFODepthMultiplier( float r )
                {{
                    r => pitchLFODepth;
                }}
            }}

			// freq: 40 to 2000 (pow)
			global float {0};
			// delay == ???. 0 to ???
			global float {1};
			// timbre LFO: higher = more pitch spread. 0 to ????
			global float {2};
			// pitch LFO rate = 0 to 10 or 0 to 400 (pow)
			global float {3};
			// percent depth of pitch to waver: 0 to 0.8 (linear)
			global float {4};
			// HPF cutoff: 20 to 20000 (pow)
			global float {5};
			// LPF cutoff: 20 to 20000 (pow)
			global float {6};
			// ampMod frequency: 0 to 20 or 0 to 400 (pow)
			global float {7};
			// ampMod depth: 0 to 0.4 (linear)
			global float {8};
            HPF hpf => LPF lpf => Gain ampMod => JCRev reverb => dac;
			SinOsc ampModLFO => blackhole;
			0.13 => ampModLFO.freq;
            1800 => hpf.freq;
            1000 => lpf.freq; // orig 6000
            0.05 => reverb.mix;

			Supersaw mySaw;
            fun void AmpMod()
            {{
                while( true )
                {{
                     (1 - {8}) + {8} * ampModLFO.last() => ampMod.gain;
                     1::ms => now;
                }}
            }}
            spork ~ AmpMod();

			fun void SetParams()
			{{
				{0} => mySaw.freq;
				{1}::second => mySaw.delay;
				{2} => mySaw.timbreLFO;
				{3} => mySaw.pitchLFORate;
				{4} => mySaw.pitchLFODepthMultiplier;
				{5} => hpf.freq;
				{6} => lpf.freq;
				{7} => ampModLFO.freq;
				1::ms => now;
			}}

			global Event {9}, {10};

            {9} => now;
            // spork ~ SetParams();
            mySaw => hpf;

            while( true ) 
            {{ 
                {0} => mySaw.freq;
				{1}::second => mySaw.delay;
				{2} => mySaw.timbreLFO;
				{3} => mySaw.pitchLFORate;
				{4} => mySaw.pitchLFODepthMultiplier;
				{5} => hpf.freq;
				{6} => lpf.freq;
				{7} => ampModLFO.freq;
				1::ms => now;
            }} 
        ", myFreq, myDelaySeconds, myTimbreLFO, myPitchLFORate, 
		myPitchLFODepthMultiplier, myHPFFreq, myLPFFreq,
		myAmpModLFOFreq, myAmpModDepth, myTurnOnEvent, myTurnOffEvent ) );
	}
	
	// Update is called once per frame
	void Update () {
		// if( Input.GetKeyDown( "space" ) )
        // {
        //     float[] newParams = new float[9];
        //     for( int i = 0; i < newParams.Length; i++ )
        //     {
        //         newParams[i] = Random.Range( 0f, 1f );
        //     }
        //     SetParams( newParams );
        // }
	}

	public void SetParams( float[] zeroOneParams )
	{
        // turn sound off 
		myChuck.BroadcastEvent( myTurnOffEvent );

        // check vector length
		if( zeroOneParams.Length != 9 ) { Debug.Log( "wrong number of params..." ); return; }

		// freq: 40 to 2000 (pow)
		myChuck.SetFloat( myFreq, zeroOneParams[0].PowMapClamp( 0f, 1f, 40f, 2000f, 2f ) );
		// delayMS == ???. 0 to ???
		myChuck.SetFloat( myDelaySeconds, 0.05f );// zeroOneParams[1].MapClamp( 0f, 1f, 0.01f, 0.07f ) );
		// timbre LFO: higher = more pitch spread. 0 to 0.9????
		myChuck.SetFloat( myTimbreLFO, zeroOneParams[2].MapClamp( 0f, 1f, 0.05f, 0.9f ) );
		// pitch LFO rate = 0 to 10 or 0 to 400 (pow)
		myChuck.SetFloat( myPitchLFORate, zeroOneParams[3].PowMapClamp( 0f, 1f, 0.01f, 10f, 1f ) );
		// percent depth of pitch to waver: 0 to ??? (linear??? it sounds good 0.01 to 0.05 and then it becomes something else)
		myChuck.SetFloat( myPitchLFODepthMultiplier, zeroOneParams[4].PowMapClamp( 0f, 1f, 0.01f, 0.8f, 8f ) );
		// HPF cutoff: 20 to 20000 (pow)
		myChuck.SetFloat( myHPFFreq, zeroOneParams[5].PowMapClamp( 0f, 1f, 20f, 800f, 2f ) );
		// LPF cutoff: 20 to 20000 (pow)
		myChuck.SetFloat( myLPFFreq, zeroOneParams[6].PowMapClamp( 0f, 1f, 800f, 20000f, 2f ) );
		// ampMod frequency: 0 to 20 or 0 to 400 (pow)
		myChuck.SetFloat( myAmpModLFOFreq, zeroOneParams[7].PowMapClamp( 0f, 1f, 0.01f, 10f, 1f ) );
		// ampMod depth: 0 to 0.4 (linear)
		myChuck.SetFloat( myAmpModDepth, zeroOneParams[8].MapClamp( 0f, 1f, 0, 0.4f ) );

        // turn sound on
		myChuck.BroadcastEvent( myTurnOnEvent );
	}

    public void SetParamsDirectly( float[] parameters )
    {
        // turn sound off 
		myChuck.BroadcastEvent( myTurnOffEvent );

        // check vector length
		if( parameters.Length != 9 ) { Debug.Log( "wrong number of params..." ); return; }

		myChuck.SetFloat( myFreq, parameters[0] );
		myChuck.SetFloat( myDelaySeconds, parameters[1] );
		myChuck.SetFloat( myTimbreLFO, parameters[2] );
		myChuck.SetFloat( myPitchLFORate, parameters[3] );
		myChuck.SetFloat( myPitchLFODepthMultiplier, parameters[4] );
		myChuck.SetFloat( myHPFFreq, parameters[5] );
		myChuck.SetFloat( myLPFFreq, parameters[6] );
		myChuck.SetFloat( myAmpModLFOFreq, parameters[7] );
		myChuck.SetFloat( myAmpModDepth, parameters[8] );

        // turn sound on
		myChuck.BroadcastEvent( myTurnOnEvent );
    }
}
