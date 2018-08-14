﻿using SigStat.Common.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SigStat.Common.Transforms
{
    public class Multiply : PipelineBase, IEnumerable, ITransformation
    {
        private readonly double byConst;

        public Multiply(double byConst)
        {
            this.byConst = byConst;
        }

        public IEnumerator GetEnumerator()
        {
            return InputFeatures.GetEnumerator();
        }

        public void Add(FeatureDescriptor newItem)
        {
            InputFeatures.Add(newItem);
        }

        public void Transform(Signature signature)
        {
            //default output is the input
            if (OutputFeatures == null)
                OutputFeatures = InputFeatures;

            if (InputFeatures[0].IsCollection)
            {
                var values = signature.GetFeature<List<double>>(InputFeatures[0]);
                for (int i = 0; i < values.Count; i++)
                    values[i] = values[i] * byConst;
                signature.SetFeature(OutputFeatures[0], values);
            }
            else
            {
                var values = signature.GetFeature<double>(InputFeatures[0]);
                values = values * byConst;
                signature.SetFeature(OutputFeatures[0], values);
            }

            Progress = 100;
        }
    }
}
