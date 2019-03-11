﻿using SigStat.Common.Helpers;
using SigStat.Common.Pipeline;
using SigStat.Common.PipelineItems.Transforms.Preprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SigStat.Common.Loaders
{
    public static class BenchmarkBuilder
    {
        public static VerifierBenchmark Build(BenchmarkConfig config)
        {
            VerifierBenchmark b = new VerifierBenchmark();
            switch (config.Sampling)
            {
                case "S1":
                    b.Sampler = new SVC2004Sampler();
                    break;
                default:
                    break;
            }
            switch (config.Database)
            {
                case "SVC2004":
                    b.Loader = new Svc2004Loader(@"Databases\Online\SVC2004\Task2.zip", true);
                    break;
                case "MCYT100":
                    b.Loader = new Svc2004Loader(@"Databases\Online\MCYT100\MCYT100.zip", true);
                    break;
                default:
                    break;
            }
            switch (config.Filter)//TODO: ez a Loader Signer filtere, vagy valami transform lesz
            {
                case "P":
                    
                    break;
                default:
                    break;
            }

            var pipeline = new SequentialTransformPipeline();

            switch (config.Rotation)
            {
                case true:
                    pipeline.Add(new NormalizeRotation() { InputX = Features.X, InputY = Features.Y, InputT = Features.T, OutputX = Features.X, OutputY = Features.Y });
                    break;
                default:
                    break;
            }

            switch (config.Translation)
            {
                case "CogToOriginX":
                    pipeline.Add(new TranslatePreproc(OriginType.CenterOfGravity) { InputFeature = Features.X, OutputFeature = Features.X });
                    break;
                case "CogToOriginY":
                    pipeline.Add(new TranslatePreproc(OriginType.CenterOfGravity) { InputFeature = Features.Y, OutputFeature = Features.Y });
                    break;
                case "CogToOriginXY":
                    pipeline.Add(new TranslatePreproc(OriginType.CenterOfGravity) { InputFeature = Features.X, OutputFeature = Features.X });
                    pipeline.Add(new TranslatePreproc(OriginType.CenterOfGravity) { InputFeature = Features.Y, OutputFeature = Features.Y });
                    break;
                case "BottomLeftToOrigin":
                    pipeline.Add(new TranslatePreproc(OriginType.Minimum) { InputFeature = Features.X, OutputFeature = Features.X });
                    pipeline.Add(new TranslatePreproc(OriginType.Minimum) { InputFeature = Features.Y, OutputFeature = Features.Y });
                    break;
                default:
                    break;
            }


            switch (config.UniformScaling)
            {
                case "X01Y0prop":
                    pipeline.Add(new UniformScale() { BaseDimension=Features.X, BaseDimensionOutput=Features.X, ProportionalDimension=Features.Y, ProportionalDimensionOutput=Features.Y});
                    break;
                case "Y01X0prop":
                    pipeline.Add(new UniformScale() { BaseDimension = Features.Y, BaseDimensionOutput = Features.Y, ProportionalDimension = Features.X, ProportionalDimensionOutput = Features.X });
                    break;
                default:
                    break;
            }

            switch (config.Scaling)
            {
                case "X01":
                    pipeline.Add(new Scale() { InputFeature = Features.X, OutputFeature = Features.X });
                    break;
                case "Y01":
                    pipeline.Add(new Scale() { InputFeature = Features.Y, OutputFeature = Features.Y });
                    break;
                case "X01Y01":
                    pipeline.Add(new Scale() { InputFeature = Features.X, OutputFeature = Features.X });
                    pipeline.Add(new Scale() { InputFeature = Features.Y, OutputFeature = Features.Y });
                    break;
                default:
                    break;
            }

            var featurelist = new List<FeatureDescriptor<List<double>>>()
            {
                Features.X, Features.Y, Features.Pressure, Features.Azimuth, Features.Altitude
            };

            IInterpolation ip = new LinearInterpolation();
            switch (config.Interpolation)
            {
                case "Cubic":
                    //ip = new CubicInterpolation();
                    break;
                case "Linear":
                default:
                    ip = new LinearInterpolation();
                    break;
            }

            switch (config.ResamplingType)
            {
                case "TimeSlot":
                    pipeline.Add(new ResampleTimeBased() {
                        InputFeatures = featurelist,
                        OutputFeatures = featurelist,
                        Interpolation = ip
                    });
                    break;
                case "SampleCount":
                    pipeline.Add(new ResampleSamplesCountBased()
                    {
                        InputFeatures = featurelist,
                        OutputFeatures = featurelist,
                        NumOfSamples = 100,
                        Interpolation = ip
                    });
                    break;
                case "FillPenUp":
                    pipeline.Add(new FillPenUpDurations()
                    { 
                        InputFeatures = featurelist,
                        OutputFeatures = featurelist,
                        FillUpTimeSlot = 10,
                        Interpolation = ip
                    });
                    break;
                default:
                    break;
            }



            b.Verifier = new Model.Verifier()
            {
                Pipeline = pipeline,
                Classifier = null//
            };

            b.Logger = new SimpleConsoleLogger();//TODO: fajlba loggoljon
            return b;

        }
    }
}
