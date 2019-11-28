﻿using Newtonsoft.Json;
using SigStat.Common.Helpers;
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
    [JsonObject(MemberSerialization.OptOut)]
    public class TangentExtraction : PipelineBase, ITransformation
    {
        /// <summary>
        /// Gets or sets the input feature representing the X coordinates of an online signature
        /// </summary>
        [Input]
        public FeatureDescriptor<List<double>> X { get; set; } = Features.X;

        /// <summary>
        /// Gets or sets the input feature representing the Y coordinates of an online signature
        /// </summary>
        [Input]
        public FeatureDescriptor<List<double>> Y { get; set; } = Features.Y;

        /// <summary>
        /// Gets or sets the output feature representing the tangent angles of an online signature
        /// </summary>
        [Output("Tangent")]
        public FeatureDescriptor<List<double>> OutputTangent { get; set; }

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
