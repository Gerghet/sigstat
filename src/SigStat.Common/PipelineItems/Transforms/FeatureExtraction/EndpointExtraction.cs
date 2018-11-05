﻿using SigStat.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SigStat.Common.Transforms
{
    /// <summary>
    /// Extracts EndPoints and CrossingPoints from Skeleton.
    /// <para>Default Pipeline Input: (bool[,]) Skeleton</para>
    /// <para>Default Pipeline Output: (List{Point}) EndPoints, (List{Point}) CrossingPoints </para>
    /// </summary>
    public class EndpointExtraction : PipelineBase, ITransformation
    {
        /// <summary> Initializes a new instance of the <see cref="EndpointExtraction"/> class. </summary>
        public EndpointExtraction()
        {
            this.Output(
                FeatureDescriptor.Get<List<Point>>("EndPoints"),
                FeatureDescriptor.Get<List<Point>>("CrossingPoints")
            );
        }

        /// <inheritdoc/>
        public void Transform(Signature signature)
        {
            //bool[,] b = signature.GetFeature(FeatureDescriptor.Get<bool[,]>(InputFeatures[0].Key));

            bool[,] b = signature.GetFeature(FeatureDescriptor.Get<bool[,]>("Skeleton"));
             
            var endPoints = new List<Point>();
            var crossingPoints = new List<Point/*, int ncnt*/>();
            int w = b.GetLength(0);
            int h = b.GetLength(1);

            for (int i = 1; i < w - 1; i++)
            {
                for (int j = 1; j < h - 1; j++)
                {
                    if (b[i, j])
                    {
                        bool[] ns = {//8 szomszed sorrendben
                            b[i - 1,j - 1],
                            b[i,j - 1],
                            b[i + 1,j - 1],
                            b[i + 1,j],
                            b[i + 1,j + 1],
                            b[i,j + 1],
                            b[i - 1,j + 1],
                            b[i - 1,j]
                        };
                        int ncnt = 0;//count neighbours
                        for (int ni = 0; ni < 8; ni++)
                        {
                            ncnt += ns[ni] ? 1 : 0;
                        }
                        if (ncnt == 1)  //1 szomszed -> endpoint
                        {
                            endPoints.Add(new Point(i, j));
                        }
                        else if (ncnt > 2)  //tobb mint 2 szomszed -> crossing point
                        {
                            crossingPoints.Add(new Point(i, j/*, ncnt*/));
                        }
                    }
                }
                Progress = (int)(i / (double)(w - 1) * 100);
            }

            signature.SetFeature(OutputFeatures[0], endPoints);
            signature.SetFeature(OutputFeatures[1], crossingPoints);

            Progress = 100;
            Log(LogLevel.Info, $"Endpoint extraction done. {endPoints.Count} endpoints and {crossingPoints.Count} crossingpoints found.");
        }
    }
}
