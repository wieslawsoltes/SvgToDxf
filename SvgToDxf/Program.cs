using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using SkiaSharp;
using System.Collections.Generic;

public class SvgToDxfConverter
{
    public static DxfDocument Convert(string svgPathData)
    {
        // Create a new DXF document.
        DxfDocument dxf = new DxfDocument();

        // Parse the SVG path data into an SKPath object.
        SKPath skPath = SKPath.ParseSvgPathData(svgPathData);

        // Create a list to store Hatch boundary path edges.
        List<HatchBoundaryPath.Edge> edges = new List<HatchBoundaryPath.Edge>();

        // Use SKPath.CreateRawIterator() to iterate over path segments.
        using var iterator = skPath.CreateRawIterator();
        var points = new SKPoint[4];
        SKPathVerb pathVerb;

        while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done)
        {
            switch (pathVerb)
            {
                case SKPathVerb.Move:
                    edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.Line, points[0].X, points[0].Y, 0, 0, 0));
                    break;
                case SKPathVerb.Line:
                    edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.Line, points[1].X, points[1].Y, 0, 0, 0));
                    break;
                case SKPathVerb.Cubic:
                    edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.CubicSpline, points[0].X, points[0].Y, points[1].X, points[1].Y, points[2].X, points[2].Y, points[3].X, points[3].Y, 0));
                    break;
                case SKPathVerb.Quad:
                    // Approximate the quadratic Bézier curve with line segments.
                    for (int i = 1; i <= 10; i++) // Adjust steps for accuracy
                    {
                        float t = (float)i / 10;
                        float x = (float)(Math.Pow(1 - t, 2) * points[0].X + 2 * t * (1 - t) * points[1].X + Math.Pow(t, 2) * points[2].X);
                        float y = (float)(Math.Pow(1 - t, 2) * points[0].Y + 2 * t * (1 - t) * points[1].Y + Math.Pow(t, 2) * points[2].Y);
                        edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.Line, x, y, 0, 0, 0));
                    }
                    break;
                case SKPathVerb.Conic:
                    // Convert conic to quadratic Bézier curves using SKPath.ConvertConicToQuads().
                    var quads = SKPath.ConvertConicToQuads(points[0], points[1], points[2], iterator.ConicWeight(), 1);
                    for (int i = 1; i < quads.Length; i += 2)
                    {
                        // Approximate each quadratic Bézier curve with line segments.
                        for (int j = 1; j <= 10; j++) // Adjust steps for accuracy
                        {
                            float t = (float)j / 10;
                            float x = (float)(Math.Pow(1 - t, 2) * quads[i - 1].X + 2 * t * (1 - t) * quads[i].X + Math.Pow(t, 2) * quads[i + 1].X);
                            float y = (float)(Math.Pow(1 - t, 2) * quads[i - 1].Y + 2 * t * (1 - t) * quads[i].Y + Math.Pow(t, 2) * quads[i + 1].Y);
                            edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.Line, x, y, 0, 0, 0));
                        }
                    }
                    break;
                case SKPathVerb.Close:
                    // Close the path by adding an edge from the last point to the first point.
                    edges.Add(new HatchBoundaryPath.Edge(HatchBoundaryPath.EdgeType.Line, edges[0].StartPoint.X, edges[0].StartPoint.Y, 0, 0, 0));
                    break;
            }
        }

        // Create the Hatch boundary path.
        HatchBoundaryPath boundary = new HatchBoundaryPath(edges)
        {
            FillMode = skPath.FillType == SKPathFillType.EvenOdd ? FillMode.Alternate : FillMode.Winding
        };

        // Create the Hatch entity.
        Hatch hatch = new Hatch(HatchPattern.Solid, new List<HatchBoundaryPath>() { boundary }, false)
        {
            ShowBoundary = true // Set to true if you want to see the boundary lines
        };

        // Add the hatch to the DXF document.
        dxf.AddEntity(hatch);

        return dxf;
    }
}