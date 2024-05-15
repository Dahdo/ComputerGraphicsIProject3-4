using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsProject3_4
{
    public class CyrusBeckClipping
    {
        public static List<Point> ClipPolygon(Shape clippedPolygon, Shape clippingPolygon)
        {
            List<Point> outputPoints = new List<Point>();

            foreach (Line edge in clippingPolygon.Edges)
            {
                // Calculate the normal vector (N) and direction vector (D) of the current edge
                Point N = new Point(edge.endPoint.Y - edge.startPoint.Y, edge.startPoint.X - edge.endPoint.X, clippingPolygon.PixelColor);
                Point D = new Point(edge.startPoint.X - edge.endPoint.X, edge.startPoint.Y - edge.endPoint.Y, clippingPolygon.PixelColor);

                double tE = 0.0;
                double tL = 1.0;

                foreach (Line subjectEdge in clippedPolygon.Edges) // Iterate through each edge of the subject clippedPolygon
                {
                    // Calculate the vector from the starting point of the subject edge to the starting point of the clipping edge (PE)
                    Point PE = new Point(subjectEdge.startPoint.X - edge.startPoint.X, subjectEdge.startPoint.Y - edge.startPoint.Y, clippedPolygon.PixelColor);

                    // Calculate dot products between normal vector and direction vector, and normal vector and PE
                    double NdotD = N.X * D.X + N.Y * D.Y;
                    double NdotPE = N.X * PE.X + N.Y * PE.Y;

                    if (NdotD == 0) // If the dot product between normal vector and direction vector is 0, the line is parallel to the edge
                    {
                        if (NdotPE < 0)
                        {
                            // If NdotPE is negative, the line is outside, no intersection, continue to the next subject edge
                            continue;
                        }
                    }
                    else // If the dot product is not 0, the line is not parallel to the edge
                    {
                        double t = -NdotPE / NdotD; // Calculate parameter t
                        if (NdotD < 0)
                        {
                            tE = Math.Max(tE, t); // Update tE if t is greater
                        }
                        else
                        {
                            tL = Math.Min(tL, t); // Update tL if t is smaller
                        }
                    }
                }

                if (tE <= tL) // If tE is less than or equal to tL, there is an intersection
                {
                    // Calculate the intersection point using parameter tE
                    double intersectionX = edge.startPoint.X + tE * D.X;
                    double intersectionY = edge.startPoint.Y + tE * D.Y;
                    outputPoints.Add(new Point((int)intersectionX, (int)intersectionY, clippedPolygon.PixelColor)); // Add the intersection point to the output list
                }
            }

            return outputPoints; // Return the list of clipped clippedPolygon points
        }
    }

}
