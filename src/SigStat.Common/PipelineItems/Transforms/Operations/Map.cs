﻿using SigStat.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SigStat.Common.Transforms
{
    /// <summary>
    /// Maps values of a feature to a specified range.
    /// <para>Pipeline Input type: List{double}</para>
    /// <para>Default Pipeline Output: (List{double}) MapResult</para>
    /// </summary>
    public class Map : PipelineBase, ITransformation
    {
        private readonly double v0;
        private readonly double v1;

        /// <summary> Initializes a new instance of the <see cref="Map"/> class with specified settings. </summary>
        /// <param name="minVal">New minimum value.</param>
        /// <param name="maxVal">New maximum value.</param>
        public Map(double minVal, double maxVal)
        {
            this.v0 = minVal;
            this.v1 = maxVal;

            this.Output(FeatureDescriptor<List<double>>.Descriptor("MapResult"));
        }

        /// <inheritdoc/>
        public void Transform(Signature signature)
        {
            List<double> values = signature.GetFeature<List<double>>(InputFeatures[0]);

            //find min and max values
            double min = values.Min();
            double max = values.Max();

            //min lesz v0, max lesz v1
            for (int i = 0; i < values.Count; i++)
            { 
                double t = (values[i] - min) / (max - min);//0-1
                values[i] = (1.0 - t) * v0 + t * v1;//lerp
                Progress += 100 / values.Count;
            }

            signature.SetFeature(OutputFeatures[0], values);

        }

    }
}
