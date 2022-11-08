# Mapping By Example

This [set of prototypes](https://ccrma.stanford.edu/~lja/vr/MappingByExample/) explores the use of interactive machine learning 
to create a mapping from hand movements to synthesizer parameters, forming a kind of gestural musical instrument.
The examples are coordinates in virtual reality that tie the position or movement of the user's hand to a specific sound.

I found that the experience of creating a mapping in a virtual space that enabled a greater spatial understanding of the training data
was vastly more satisfying than other simpler methods of creating a mapping by demonstrating examples or writing code. For the input training data, both hand position
and hand velocity were useful for making different kinds of mappings. When using these sets of training data together, however, it became
more difficult to translate user intent into reasonably surprising outputs. These findings suggested the importance of exploring interfaces for
users to control the feature vector.

## Running the Prototypes

Each Unity scene represents a different prototype. 

- [BaselineScene1](Assets/Scenes/BaselineScene1.unity), [BaselineScene2](Assets/Scenes/BaselineScene2.unity): baseline comparisons 
to simpler methods for creating mappings, including writing [mapping code](Assets/Scripts/BaselineManualMapping) and placing examples of specific sounds
and [crossfading volume](Assets/Scripts/BaselineVolumeMapping) between them.
- [Position-Based Regression](Assets/Scenes/MLRegression.unity): comparisons between [linear regression and neural network regression](Assets/Scripts/RapidMix/RapidMixRegression.cs)
for mappings that translate example positions to [synthesizer parameters](Assets/Scripts/ParamVectorToSound.cs).
- [Velocity-Based Regression](Assets/Scenes/MLRegressionWithVelocity.unity): comparisons between using [different combinations](Assets/Scripts/RegressionMapping/RegressionMappingVelocityExample.cs#L48) of hand position, 
vlocity, and angular velocity to [map](Assets/Scripts/RegressionMapping/RegressionMappingWithVelocity.cs) synthesizer parameters.

## RapidMix API Embedding

This project also served to test my bespoke [Unity embedding](Assets/Scripts/RapidMix/RapidMixRegression.cs)
 of the [RapidMix API](http://www.rapidmixapi.com) for interactive machine learning. The embedding also involved some work compiling a C++ DLL
(repository is private). Tests were written for [2D](Assets/Scripts/RapidMix/RegressionTester2D.cs) and 
[3D](Assets/Scripts/RapidMix/RegressionTester.cs) cases.
