﻿using SigStat.Common.Helpers;
using SigStat.Common.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace SigStat.Common.Transforms
{
    /// <summary>
    /// Extracts tangent values of the standard X, Y <see cref="Features"/>
    /// <para>Default Pipeline Input: X, Y <see cref="Features"/></para>
    /// <para>Default Pipeline Output: (List{double})  Tangent </para>
    /// </summary>
    public class TangentExtraction : PipelineBase, ITransformation
    {
        [Input]
        public FeatureDescriptor<List<double>> X = Features.X;

        [Input]
        public FeatureDescriptor<List<double>> Y = Features.Y;

        [Output("Tangent")]
        public FeatureDescriptor<List<double>> OutputTangent;

        /// <inheritdoc/>
        public void Transform(Signature signature)
        {
            var xs = signature.GetFeature(X);
            var ys = signature.GetFeature(Y);

            List<double> res = new List<double>();
            for (int i = 1; i < xs.Count - 2; i++)
            {
                double dx = xs[i + 1] - xs[i - 1];
                double dy = ys[i + 1] - ys[i - 1];
                res.Add(Math.Atan2(dy, dx));
                Progress += 100 / xs.Count-2;
            }
            res.Insert(0, res[0]);//elso
            res.Add(res[res.Count - 1]);//utolso
            res.Add(res[res.Count - 1]);//utolso
            signature.SetFeature(OutputTangent, res);
            Progress = 100;
        }
    }
}
