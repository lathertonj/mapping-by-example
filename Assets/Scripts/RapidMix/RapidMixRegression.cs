using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class RapidMixRegression : MonoBehaviour
{
    private System.UInt32 myTrainingID, myRegressionID, myOutputLength;
    private bool haveTrained = false;

    public enum RegressionType { NeuralNetworkRegression, GaussianMixtureRegression, LinearRegression };

    public RegressionType regressionType;

    // TODO: try xmmStaticRegression
    void Awake()
    {
        myTrainingID = createEmptyTrainingData();
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                myRegressionID = createNewStaticRegression();
                break;
            case RegressionType.GaussianMixtureRegression:
                myRegressionID = createNewStaticXMMRegression();
                break;
            case RegressionType.LinearRegression:
                Debug.Log( "linear regression not implemented yet" );
                break;
        }
        myOutputLength = 0;
    }

    public void RecordDataPoint( double[] input, double[] output )
    {
        // remember expected output length
        if( myOutputLength == 0 )
        {
            myOutputLength = (System.UInt32) output.Length;
        }

        // show error if we get something that isn't the expected length
        if( myOutputLength != output.Length )
        {
            Debug.LogError( string.Format( "Received output of dimension {0} which was different than the expected / originally recieved output dimension {1}", output.Length, myOutputLength ) );
        }

        recordSingleTrainingElement(
            myTrainingID,
            input, (System.UInt32) input.Length,
            output, (System.UInt32) output.Length
        );
    }

    public void Train()
    {
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                trainStaticRegression( myRegressionID, myTrainingID );
                break;
            case RegressionType.GaussianMixtureRegression:
                trainStaticXMMRegression( myRegressionID, myTrainingID );
                break;
            case RegressionType.LinearRegression:
                Debug.Log( "linear regression not implemented yet" );
                break;
        }
        
        haveTrained = true;
    }

    public double[] Run( double[] input )
    {
        if( !haveTrained )  
        {
            Debug.LogError( "Regression can't Run() without having Train()ed first!" );
            return new double[]{ };
        }
        double [] output = new double[myOutputLength];
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                runStaticRegression(
                    myRegressionID,
                    input, (System.UInt32) input.Length,
                    output, (System.UInt32) output.Length
                );
                break;
            case RegressionType.GaussianMixtureRegression:
                runStaticXMMRegression(
                    myRegressionID,
                    input, (System.UInt32) input.Length,
                    output, (System.UInt32) output.Length
                );
                break;
            case RegressionType.LinearRegression:
                Debug.Log( "linear regression not implemented yet" );
                break;
        }
        
        return output;
    }

    public void ResetRegression()
    {
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                resetStaticRegression( myRegressionID );
                break;
            case RegressionType.GaussianMixtureRegression:
                resetStaticXMMRegression( myRegressionID );
                break;
            case RegressionType.LinearRegression:
                Debug.Log( "linear regression not implemented yet" );
                break;
        }
        
        haveTrained = false;
        resetTrainingData( myTrainingID ); // deletes and recreates the dataset
        myOutputLength = 0; // to enable if the dataset can also be reset
    }

    const string PLUGIN_NAME = "RapidMixAPI";

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createEmptyTrainingData();

    [DllImport( PLUGIN_NAME )]
    private static extern void resetTrainingData( System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createNewStaticRegression();

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createNewStaticXMMRegression();

    [DllImport( PLUGIN_NAME )]
    private static extern bool recordSingleTrainingElement(
        System.UInt32 trainingID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_ouput
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool trainStaticRegression( System.UInt32 regressionID, System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool trainStaticXMMRegression( System.UInt32 XMMRegressionID, System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool runStaticRegression(
        System.UInt32 regressionID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_output
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool runStaticXMMRegression(
        System.UInt32 XMMRegressionID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_output
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool resetStaticRegression( System.UInt32 regressionID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool resetStaticXMMRegression( System.UInt32 XMMRegressionID );

    // TODO: call cleanupRapidMixApi()

}
