using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComputerGraphicsProject3_4
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Drawing;

    public class ScanLineFill
    {
        private class Edge
        {
            public int x1, y1, x2, y2, dx, dy;

            public Edge(int x1, int y1, int x2, int y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;

                this.dx = x2 - x1;
                this.dy = y2 - y1;
            }
        }

        private static int getYmin(List<Edge> edgeTable)
        {
            int yMin = edgeTable[0].y1;
            foreach (Edge edge in edgeTable)
            {
                if (edge.y1 < yMin)
                    yMin = edge.y1;
            }
            return yMin;
        }

        private static int getYmax(List<Edge> edgeTable)
        {
            int yMax = edgeTable[0].y1;
            foreach (Edge edge in edgeTable)
            {
                if (edge.y1 > yMax)
                    yMax = edge.y1;
            }
            return yMax;
        }

        private static bool isActive(Edge edge, int currentY)
        {
            return (edge.y1 < currentY && currentY <= edge.y2) || (edge.y1 >= currentY && currentY > edge.y2);
        }

        private static List<Edge> updateActiveEdges(List<Edge> activeEdges, List<Edge> edgeTable, int y)
        {
            foreach (Edge edge in edgeTable)
            {
                if (isActive(edge, y))
                {
                    if (!activeEdges.Contains(edge))
                    {
                        activeEdges.Add(edge);
                    }
                }
                else
                {
                    if (activeEdges.Contains(edge))
                    {
                        activeEdges.Remove(edge);
                    }
                }
            }

            return activeEdges;
        }

        private static List<int> findIntersections(List<int> intersections, List<Edge> activeEdges, int currentY)
        {
            intersections.Clear();

            foreach (Edge edge in activeEdges)
            {
                int x = edge.x1 + ((currentY - edge.y1) * edge.dx) / edge.dy;
                intersections.Add(x);
            }

            return intersections;
        }

        public static void Fill(List<Point> vertices, WriteableBitmap? imageCanvasBitmap, bool isImageFill, System.Windows.Media.Color fillColor , Bitmap fillImage = null)
        {
            List<Edge> edgeTable = new List<Edge>();

            //Graphics sourceGraphics = Graphics.FromImage(fillImage);
            //Bitmap fillImage = new Bitmap("C:\\Users\\Dahdo\\Desktop\\ComputerGraphicsI\\a.png");

            // Populate edge table
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                int x1 = vertices[i].X;
                int y1 = vertices[i].Y;
                int x2 = vertices[i + 1].X;
                int y2 = vertices[i + 1].Y;
                edgeTable.Add(new Edge(x1, y1, x2, y2));
            }

            // Connect last vertex to first vertex
            int lastVertexIndex = vertices.Count - 1;
            int x1_last = vertices[lastVertexIndex].X;
            int y1_last = vertices[lastVertexIndex].Y;
            int x2_first = vertices[0].X;
            int y2_first = vertices[0].Y;
            edgeTable.Add(new Edge(x1_last, y1_last, x2_first, y2_first));


            List<Edge> activeEdges = new List<Edge>();
            List<int> intersections = new List<int>();

            int yMin = getYmin(edgeTable);
            int yMax = getYmax(edgeTable);

            for (int y = yMin; y < yMax; y++)
            {
                activeEdges = updateActiveEdges(activeEdges, edgeTable, y);
                intersections = findIntersections(intersections, activeEdges, y);

                intersections.Sort();

                for (int i = 0; i < intersections.Count; i += 2)
                {
                    int x1 = intersections[i];

                    if (i + 1 >= intersections.Count)
                        break;

                    int x2 = intersections[i + 1];

                    for(int  x = x1; x < x2; x++)
                    {
                        if(isImageFill)
                        {
                            int adjustedX = x % fillImage.Width;

                            int adjustedY = y % fillImage.Height;

                            // Get the System.Drawing.Color
                            System.Drawing.Color drawingColor = fillImage.GetPixel(adjustedX, adjustedY);

                            // Convert System.Drawing.Color to System.Windows.Media.Color
                            System.Windows.Media.Color pixelColor = System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);


                            Point point = new Point(x, y, pixelColor);

                            // Put the pixel on the canvas
                            PutSinglePixel(point, imageCanvasBitmap);
                        }
                        else
                        {
                            Point point = new Point(x, y, fillColor);
                            PutSinglePixel(point, imageCanvasBitmap);
                        }

                    }
                }
            }
        }

        private static void PutSinglePixel(Point point, WriteableBitmap? imageCanvasBitmap)
        {
            if (imageCanvasBitmap == null)
            {
                MessageBox.Show("CanvasBitmap not set");
                return;
            }

            int column = point.X;
            int row = point.Y;
            System.Windows.Media.Color color = point.PixelColor;

            try
            {
                // Reserve the back buffer for updates.
                imageCanvasBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = imageCanvasBitmap.BackBuffer;
                    // Find the address of the pixel to draw.
                    pBackBuffer += row * imageCanvasBitmap.BackBufferStride;
                    pBackBuffer += column * 3;

                    // Compute the pixel's color.
                    int color_data = color.R << 16; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B

                    //try
                    //{
                    //Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
                    // Specify the area of the bitmap that changed.
                    imageCanvasBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    //}
                    //catch(Exception e)
                    //{
                    //    MessageBox.Show(e.Message);
                    //    return;
                    //}
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                imageCanvasBitmap.Unlock();
            }
        }
    }
}

