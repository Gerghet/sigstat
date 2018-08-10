﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.Primitives;
using SigStat.Common.Helpers;
using SixLabors.Shapes;

namespace SigStat.Common.PipelineItems.Transforms
{
    public class BinaryRasterizer : PipelineBase, ITransformation
    {
        private readonly int w;
        private readonly int h;
        private readonly float penwidth;
        Byte4 fg = new Byte4(0, 0, 0, 255);
        Byte4 bg = new Byte4(255, 255, 255, 255);
        GraphicsOptions noAA = new GraphicsOptions(false);//aa kikapcs, mert binarisan dolgozunk es ne legyenek szakadasok
        Pen<Byte4> pen;

        public BinaryRasterizer(int resolutionX, int resolutionY, float penwidth)
        {
            this.w = resolutionX;
            this.h = resolutionY;
            this.penwidth = penwidth;
            pen = new Pen<Byte4>(fg, penwidth);
        }

        public void Transform(Signature signature)
        {
            List<double> xs = signature.GetFeature(Features.X);
            List<double> ys = signature.GetFeature(Features.Y);
            List<bool> pendowns = signature.GetFeature(Features.Button);
            List<double> ps = signature.GetFeature(Features.Pressure);
            List<int> alts = signature.GetFeature(Features.Altitude);
            List<int> azs = signature.GetFeature(Features.Azimuth);
            //+ egyeb ami kellhet

            //TODO: X,Y legyen normalizalva, normalizaljuk ha nincs, ahhoz kell az Extrema, ..

            Image<Byte4> img = new Image<Byte4>(w, h);
            img.Mutate(ctx => ctx.Fill(bg));

            int len = xs.Count;
            List<IPath> paths = new List<IPath>();
            List<PointF> points = new List<PointF>();
            for (int i=0;i<len;i++)
            {
                if (pendowns[i])
                {
                    points.Add(ToImageCoords(xs[i], ys[i]));
                }
                else
                {
                    if(points.Count>0)
                        DrawLines(img, points);
                    points = new List<PointF>();
                    points.Add(ToImageCoords(xs[i], ys[i]));
                }
                Progress = (int)(i / (double)len * 90);
            }
            DrawLines(img, points);

            bool[,] b = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    b[x, y] = (img[x, y] == fg);

            signature.SetFeature(FeatureDescriptor<bool[,]>.Descriptor("Binarized"), b);
            Progress = 100;
            Log(LogLevel.Info, "Rasterization done.");
        }

        private PointF ToImageCoords(double x, double y)
        {
            //ha x-et w-vel, y-t pedig h-val szoroznank, akkor torzulna
            //megtartjuk az aranyokat ugy, hogy w es h bol a kisebbiket valasztjuk (igy biztos belefer a kepbe minden)
            int m = Math.Min(w, h);

            int frame = m / 20;//keretet hagyunk, hogy ne a kep legszelerol induljon

            //betesszuk a kep kozepere is
            return new PointF(
                (float)(frame + x * (m-frame*2) + (w-m)/2),
                (float)(frame + y * (m-frame*2) + (h-m)/2)
            );
        }

        private void DrawLines(Image<Byte4> img, List<PointF> ps)
        {
            img.Mutate(ctx => {
                ctx.DrawLines(noAA, pen, ps.ToArray());
                ctx.DrawLines(noAA, pen, ps.ToArray());// 2x kell meghivni hogy mukodjon??
            });
        }
    }
}
