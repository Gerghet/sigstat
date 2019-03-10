﻿using SigStat.Common.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SigStat.Common.PipelineItems.Transforms.Preprocessing
{
    public class NormalizeRotation : PipelineBase, ITransformation
    {
        //[Output] 
        public List<FeatureDescriptor> OutputFeatures;


        public void Transform(Signature signature)
        {
            //TODO: ellenőrizni h meg van-e adva output

            var linePoints = GenerateLinearBestFit(signature, out double a, out double b);

            var xValues = signature.GetFeature(Features.X);
            var yValues = signature.GetFeature(Features.Y);

            var time = signature.GetFeature(Features.T);

            var angle = CalculateAngleBetweenLines(
                time.Min(), time.Max(), linePoints.Min(), linePoints.Max(),
                time.Min(), time.Max(), linePoints.Min(), linePoints.Min());
            Console.WriteLine($"angle:{angle}");

            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            //double cosa = (time.Max() - time.Min()) / (linePoints.Max() - linePoints.Min());
            //double sina = (linePoints.Max() - linePoints.Min()) / (time.Max() - time.Min());
            Console.WriteLine($"COS={cosa}, SIN={sina}");

            for (int i = 0; i < xValues.Count; i++)
            {
                double x = xValues[i];
                double y = yValues[i];
                xValues[i] = x * cosa - y * sina;
                yValues[i] = x * sina + y * cosa;
            }

            signature.SetFeature(Features.X, xValues);
            signature.SetFeature(Features.Y, yValues);
        }


        private List<double> GenerateLinearBestFit(Signature sig, out double a, out double b)
        {
            var tValues = new List<double>(sig.GetFeature(Features.T));
            var yValues = new List<double>(sig.GetFeature(Features.Y));

            int numPoints = yValues.Count;
            double meanT = tValues.Average();
            double meanY = yValues.Average();

            double sumTSquared = tValues.Sum(t => t * t);

            double sumTY = 0;
            for (int i = 0; i < numPoints; i++)
            {
                sumTY += tValues[i] * yValues[i];
            }

            a = (sumTY / numPoints - meanT * meanY) / (sumTSquared / numPoints - meanT * meanT);
            b = (a * meanT - meanY);

            double a1 = a;
            double b1 = b;

            var newYValues = new List<double>(numPoints);
            for (int i = 0; i < numPoints; i++)
            {
                newYValues.Add(a1 * tValues[i] - b1);
            }

            return newYValues;

        }

        private double CalculateAngleBetweenLines(
            double p1x0, double p1x1, double p1y0, double p1y1, 
            double p2x0, double p2x1, double p2y0, double p2y1)
        {
            //Calculate the angles
            var thetaP1 = Math.Atan2(p1y0 - p1y1, p1x0 - p1x1);
            var thetaP2 = Math.Atan2(p2y0 - p2y1, p2x0 - p2x1);

            //Calculate the angle between the lines
            var diff = Math.Abs(thetaP1 - thetaP2);

            return diff;
        }
    }
}
