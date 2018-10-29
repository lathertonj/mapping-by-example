using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class RapidMixRegression : MonoBehaviour
{
    private System.UInt32 myTrainingID, myRegressionID, myOutputLength, myInputLength;
    private bool haveTrained = false;

    public enum RegressionType { NeuralNetworkRegression, GaussianMixtureRegression, LinearRegression };

    public RegressionType regressionType;

    private List<double[]> myLinearRegressionInputs;
    private List<double> myLinearRegressionOutputs;

    // TODO: GMRegression crashes on training
    void Awake()
    {
        myTrainingID = createEmptyTrainingData();
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                myRegressionID = createNewStaticRegression();
                break;
            case RegressionType.GaussianMixtureRegression:
                myRegressionID = createNewStaticGMRegression();
                break;
            case RegressionType.LinearRegression:
                myLinearRegressionInputs = new List<double[]>();
                myLinearRegressionOutputs = new List<double>();
                break;
        }
        myOutputLength = 0;
        myInputLength = 0;
    }

    public void RecordDataPoint( double[] input, double[] output )
    {
        // remember expected output length
        if( myOutputLength == 0 )
        {
            myOutputLength = (System.UInt32)output.Length;
            myInputLength = (System.UInt32)input.Length;
        }

        // show error if we get something that isn't the expected length
        if( myOutputLength != output.Length )
        {
            Debug.LogError( string.Format( "Received output of dimension {0} which was different than the expected / originally recieved output dimension {1}", output.Length, myOutputLength ) );
        }

        // record
        if( regressionType == RegressionType.LinearRegression )
        {
            RecordDataPointLinearRegression( input, output[0] );
        }
        else
        {
            recordSingleTrainingElement(
                myTrainingID,
                input, (System.UInt32)input.Length,
                output, (System.UInt32)output.Length
            );
        }

    }


    private void RecordDataPointLinearRegression( double[] input, double output )
    {
        myLinearRegressionInputs.Add( input );
        myLinearRegressionOutputs.Add( output );
    }

    public void Train()
    {
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                trainStaticRegression( myRegressionID, myTrainingID );
                break;
            case RegressionType.GaussianMixtureRegression:
                trainStaticGMRegression( myRegressionID, myTrainingID );
                break;
            case RegressionType.LinearRegression:
                TrainLinearRegression();
                break;
        }

        haveTrained = true;
    }

    private double[] myWeights;
    private double[] myWeightsGradient;
    private void TrainLinearRegression()
    {
        myWeights = new double[myInputLength];
        myWeightsGradient = new double[myInputLength];
        double eta = 0.01;
        int numSteps = 100;
        for( int i = 0; i < numSteps; i++ )
        {
            ComputeGradient();
            for( int j = 0; j < myWeights.Length; j++ )
            {
                myWeights[j] -= eta * myWeightsGradient[j];
            }
            // Debug.Log( string.Format( string.Format( "iteration {0}, error = {1}", i, ComputeError() ) ) );
        }
    }

    private double DotWeights( double[] input )
    {
        double ret = 0;
        for( int i = 0; i < input.Length; i++ )
        {
            ret += input[i] * myWeights[i];
        }
        return ret;
    }

    private void ComputeGradient()
    {
        for( int j = 0; j < myWeightsGradient.Length; j++ ) { myWeightsGradient[j] = 0; }
        for( int i = 0; i < myLinearRegressionInputs.Count; i++ )
        {
            double dot = DotWeights( myLinearRegressionInputs[i] );
            double multiplier = dot - myLinearRegressionOutputs[i];
            for( int j = 0; j < myWeightsGradient.Length; j++ )
            {
                myWeightsGradient[j] += multiplier * myLinearRegressionInputs[i][j];
            }
        }
    }

    private double ComputeError()
    {
        double sum = 0;
        for( int i = 0; i < myLinearRegressionInputs.Count; i++ )
        {
            sum += System.Math.Pow( DotWeights( myLinearRegressionInputs[i] ) - myLinearRegressionOutputs[i], 2 );
        }
        return sum;
    }

    public double[] Run( double[] input )
    {
        if( !haveTrained )
        {
            Debug.LogError( "Regression can't Run() without having Train()ed first!" );
            return new double[] { };
        }
        double[] output = new double[myOutputLength];
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                runStaticRegression(
                    myRegressionID,
                    input, (System.UInt32)input.Length,
                    output, (System.UInt32)output.Length
                );
                break;
            case RegressionType.GaussianMixtureRegression:
                runStaticGMRegression(
                    myRegressionID,
                    input, (System.UInt32)input.Length,
                    output, (System.UInt32)output.Length
                );
                break;
            case RegressionType.LinearRegression:
                RunLinearRegression( input, output );
                break;
        }

        return output;
    }

    void RunLinearRegression( double[] input, double[] output )
    {
        output[0] = DotWeights( input );
    }

    public void ResetRegression()
    {
        switch( regressionType )
        {
            case RegressionType.NeuralNetworkRegression:
                resetStaticRegression( myRegressionID );
                break;
            case RegressionType.GaussianMixtureRegression:
                resetStaticGMRegression( myRegressionID );
                break;
            case RegressionType.LinearRegression:
                // nothing to reset. weights are recreated on Train().
                break;
        }

        haveTrained = false;
        // reset training data
        if( regressionType == RegressionType.LinearRegression )
        {
            ResetLinearRegressionData();
        }
        else
        {
            resetTrainingData( myTrainingID ); // deletes and recreates the dataset
        }
        myOutputLength = 0; // to enable if the dataset can also be reset
        myInputLength = 0;
    }

    void ResetLinearRegressionData()
    {
        myLinearRegressionInputs.Clear();
        myLinearRegressionOutputs.Clear();
    }

    const string PLUGIN_NAME = "RapidMixAPI";

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createEmptyTrainingData();

    [DllImport( PLUGIN_NAME )]
    private static extern void resetTrainingData( System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createNewStaticRegression();

    [DllImport( PLUGIN_NAME )]
    private static extern System.UInt32 createNewStaticGMRegression();

    [DllImport( PLUGIN_NAME )]
    private static extern bool recordSingleTrainingElement(
        System.UInt32 trainingID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_ouput
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool trainStaticRegression( System.UInt32 regressionID, System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool trainStaticGMRegression( System.UInt32 GMRegressionID, System.UInt32 trainingID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool runStaticRegression(
        System.UInt32 regressionID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_output
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool runStaticGMRegression(
        System.UInt32 GMRegressionID,
        double[] input, System.UInt32 n_input,
        double[] output, System.UInt32 n_output
    );

    [DllImport( PLUGIN_NAME )]
    private static extern bool resetStaticRegression( System.UInt32 regressionID );

    [DllImport( PLUGIN_NAME )]
    private static extern bool resetStaticGMRegression( System.UInt32 GMRegressionID );

    // TODO: call cleanupRapidMixApi()

}
