using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsProject3_4
{
    // The implementation is highly influenced by this source: https://www.geeksforgeeks.org/line-clipping-set-2-cyrus-beck-algorithm/
    class CyrusBeckClipping
    {

        static int Dot(Point p0, Point p1)
        {
            return p0.X * p1.X + p0.Y * p1.Y;
        }

        static float Max(List<float> t)
        {
            return t.Max();
        }

        static float Min(List<float> t)
        {
            return t.Min();
        }

        private static Shape clippingPolygon;

        public static List<Point> ClipPolygon(Shape clippingPolygon_, Shape clippedPolygon)
        {
            List<Point> points = new List<Point>();
            clippingPolygon = clippingPolygon_;
            foreach(Line line in clippedPolygon.Edges)
            {
                List<Point> linePoints = ClipLine(line);
                if (linePoints[0].X == -1 || linePoints[1].X == -1)
                    continue;
                points.AddRange(linePoints);
            }
            return points;
        }

        private static List<Point> ClipLine(Line clippedLine)
        {
            List<Point> vertices = clippingPolygon.GetPoints();
            vertices = vertices.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            List<Point> line = new List<Point>
            {
                clippedLine.startPoint,
                clippedLine.endPoint,
            };

            List<Point> newPair = new List<Point>();

            List<Point> normal = new List<Point>();

            for (int i = 0; i < vertices.Count; i++)
            {
                Point point = new Point(vertices[i].Y - vertices[(i + 1) % vertices.Count].Y,
                    vertices[(i + 1) % vertices.Count].X - vertices[i].X, clippedLine.PixelColor);
                normal.Add(point);
            }

            Point P1_P0 = new Point(line[1].X - line[0].X, line[1].Y - line[0].Y, clippedLine.PixelColor);

            List<Point> P0_PEi = new List<Point>();

            for (int i = 0; i < vertices.Count; i++)
            {

                Point point = new Point(vertices[i].X - line[0].X,
                    vertices[i].Y - line[0].Y, clippedLine.PixelColor);
                P0_PEi.Add(point);
            }

            int[] numerator = new int[vertices.Count], denominator = new int[vertices.Count];

            // Calculating the numerator and denominators
            // using the dot function
            for (int i = 0; i < vertices.Count; i++)
            {
                numerator[i] = Dot(normal[i], P0_PEi[i]);
                denominator[i] = Dot(normal[i], P1_P0);
            }

            float[] t = new float[vertices.Count];

            // Making two vectors called 't entering'
            // and 't leaving' to group the 't's
            List<float> tE = new List<float>(), tL = new List<float>();

            // Calculating 't' and grouping them accordingly
            for (int i = 0; i < vertices.Count; i++)
            {
                t[i] = (float)numerator[i] / (float)denominator[i];

                if (denominator[i] > 0)
                    tE.Add(t[i]);
                else
                    tL.Add(t[i]);
            }

            // Initializing the final two values of 't'
            float[] temp = new float[2];

            // Taking the max of all 'tE' and 0, so pushing 0
            tE.Add(0f);
            temp[0] = Max(tE);

            // Taking the min of all 'tL' and 1, so pushing 1
            tL.Add(1f);
            temp[1] = Min(tL);

            if (temp[0] > temp[1])
            {
                newPair.Add(new Point(-1, -1, clippedLine.PixelColor));
                newPair.Add(new Point(-1, -1, clippedLine.PixelColor));
                return newPair;
            }

            // Calculating the coordinates in terms of x and y
            newPair.Add(new Point((int)(line[0].X + P1_P0.X * temp[0]), (int)(line[0].Y + P1_P0.Y * temp[0]), clippedLine.PixelColor));
            newPair.Add(new Point((int)(line[0].X + P1_P0.X * temp[1]), (int)(line[0].Y + P1_P0.Y * temp[1]), clippedLine.PixelColor));

            return newPair;
        }

    }
}
