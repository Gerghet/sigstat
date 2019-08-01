﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SigStat.Common.Helpers;
using Microsoft.Extensions.Logging;
using SigStat.Common.Pipeline;
using Newtonsoft.Json;

namespace SigStat.Common.Transforms
{
    /// <summary>
    /// Resizes the image to a specified width and height
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Resize : PipelineBase, ITransformation
    {
        /// <summary>
        /// Input <see cref="FeatureDescriptor"/> describing the image of the signature
        /// </summary>
        [Input]
        public FeatureDescriptor<Image<Rgba32>> InputImage { get; set; }

        /// <summary>
        /// Output <see cref="FeatureDescriptor"/> describing the resized image of the signature
        /// </summary>
        [Output("Resized")]
        public FeatureDescriptor<Image<Rgba32>> OutputImage { get; set; }

        /// <summary>
        /// The new width. Leave it as null, if you do not want to explicitly specify a given width
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// The new height. Leave it as null, if you do not want to explicitly specify a given height
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// Set a resize function if you want to dynamically calculate the new width and height of the image
        /// </summary>
        public Func<Image<Rgba32>, Size> ResizeFunction { get; set; }

        /// <inheritdoc/>
        public void Transform(Signature signature)
        {
            Image<Rgba32> image = signature.GetFeature(InputImage);

            Size newSize = CalculateSize(image);
            var newImage = image.Clone(ctx => ctx.Resize(newSize, KnownResamplers.Bicubic, true));

            // Dispose old image if not needed anymore
            if (InputImage == OutputImage)
            {
                image.Dispose();
            }

            signature.SetFeature(OutputImage, newImage);
            this.LogInformation( "Resizing done.");
            Progress = 100;
        }

        private Size CalculateSize(Image<Rgba32> image)
        {
            // Check exclusive arguments for invalid combinations
            if (Width == null && Height == null && ResizeFunction == null)
            {
                throw new ArgumentNullException($"{Width} and {Height} and {ResizeFunction} can not be all null.");
            }
            if ((Width != null || Height != null) && ResizeFunction != null)
            {
                throw new ArgumentNullException($"Do not set {Width} or {Height} if you want to use {ResizeFunction}.");
            }

            // Use the ResizeFunction
            if (Width == null && Height == null && ResizeFunction != null)
            {
                return ResizeFunction(image);
            }

            // Resize and keep aspect ratio
            var originalSize = image.Size();
            if (Width != null && Height == null)
            {
                Height = Width * originalSize.Height / originalSize.Width;
            }
            if (Height != null && Width == null)
            {
                Width = Height * originalSize.Width / originalSize.Height;
            }

            // Resize
            return new Size(Width.Value, Height.Value);
        }
    }
}
