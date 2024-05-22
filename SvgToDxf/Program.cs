using SkiaSharp;

var svgPath =
    "M11.75 3a.75.75 0 0 1 .743.648l.007.102.001 7.25h7.253a.75.75 0 0 1 .102 1.493l-.102.007h-7.253l.002 7.25a.75.75 0 0 1-1.493.101l-.007-.102-.002-7.249H3.752a.75.75 0 0 1-.102-1.493L3.752 11h7.25L11 3.75a.75.75 0 0 1 .75-.75Z";

var skPath = SKPath.ParseSvgPathData(svgPath);

Parse(skPath);

static void Parse(SKPath path)
{
    using var iterator = path.CreateRawIterator();
    var points = new SKPoint[4];
    SKPathVerb pathVerb;

    // path.FillType

    while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done)
    {
        switch (pathVerb)
        {
            case SKPathVerb.Move:
            {
                // points[0].X, points[0].Y
                Console.WriteLine("Move");
                break;
            }
            case SKPathVerb.Line:
            {
                // LineTo
                // points[1].X, points[1].Y
                Console.WriteLine("LineTo");
                break;
            }
            case SKPathVerb.Cubic:
            {
                // CubicBezierTo
                // points[1].X, points[1].Y
                // points[2].X, points[2].Y
                // points[3].X, points[3].Y
                Console.WriteLine("LineTo");
                break;
            }
            case SKPathVerb.Quad:
            {
                // QuadraticBezierTo
                // points[1].X, points[1].Y
                // points[2].X, points[2].Y
                Console.WriteLine("LineTo");
                break;
            }
            case SKPathVerb.Conic:
            {
                var quads = SKPath.ConvertConicToQuads(points[0], points[1], points[2], iterator.ConicWeight(), 1);
                // QuadraticBezierTo
                // quads[1].X, quads[1].Y
                // quads[2].X, quads[2].Y
                // QuadraticBezierTo
                // quads[3].X, quads[3].Y
                // quads[4].X, quads[4].Y
                Console.WriteLine("Conic");
                break;
            }
            case SKPathVerb.Close:
            {
                // SetClosedState(true)
                Console.WriteLine("Close");
                break;
            }
        }
    }
}
