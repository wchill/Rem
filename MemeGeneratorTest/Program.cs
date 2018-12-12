using MemeGenerator;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace MemeGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
            /*
            var img = Image.Load<Rgba32>("test2.png");
            //var mask = Image.Load<Rgba32>("mask.png");
            var canvas = new Image<Rgba32>(600, 600);
            //var output = new Image<Rgba32>(mask.Width, mask.Height);

            //var m1 = ImageProjectionHelper.CalculateProjectiveTransformationMatrix(img.Width, img.Height,
                //new Point(52, 165), new Point(358, 109), new Point(115, 327), new Point(423, 239));
                //new Point(52, 165), new Point(358, 109), new Point(115, 327), new Point(600, 600));

            var m1 = new Matrix4x4(0.260987f, -0.434909f, 0, -0.0022184f, 0.373196f, 0.949882f, 0, -0.000312129f, 0, 0, 1, 0, 52, 165, 0, 1);

            canvas.Mutate(ctx =>
            {
                ctx.DrawImage(img, 1);
                //ImageProjectionHelper.ProjectLayerOntoSurface(ctx, m1);
                ctx.Transform(m1, KnownResamplers.Lanczos3);
            });
            canvas.Save("canvas.png");
            canvas.Mutate(ctx =>
            {
                ctx.DrawImage(mask, 1);
                ctx.DrawImage(mask, PixelBlenderMode.Xor, 1);
            });
            canvas.Save("canvas2.png");
            output.Mutate(ctx =>
            {
                ctx.Fill(Rgba32.Azure);
                ctx.DrawImage(canvas, 1);
            });
            canvas.Save("output.png");
            */
        }

        static void Test()
        {
            var img = Image.Load<Rgba32>("test.png");
            img.Mutate(ctx => { ctx.Resize(290, 154); });
            var canvas = new Image<Rgba32>(290, 154);

            var m1 = new Matrix4x4(0.260987f, -0.434909f, 0, -0.0022184f, 0.373196f, 0.949882f, 0, -0.000312129f, 0, 0, 1, 0, 52, 165, 0, 1);

            canvas.Mutate(ctx =>
            {
                ctx.DrawImage(img, 1);
                ctx.Transform(new ProjectiveTransformBuilder().AppendMatrix(m1), KnownResamplers.Lanczos3);
            });
            canvas.Save("canvas.png");
        }
    }
}
