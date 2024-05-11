using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ComputerGraphicsProject3_4
{
    [Serializable]
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Color PixelColor;


        public Point(int x, int y, Color color)
        {
            X = x;
            Y = y;
            PixelColor = color;
        }

        public Point()
        {
            X = -1;
            Y = -1;
            PixelColor = Colors.Yellow;
        }

        public override string ToString()
        {
            return "(" + this.X + ", " + this.Y + ")";
        }
    }

    [Serializable]
    [XmlInclude(typeof(Line))]
    [XmlInclude(typeof(Circle))]
    [XmlInclude(typeof(Polygon))]
    [XmlInclude(typeof(Rectangle))]
    public abstract class Shape
    {
        public Color PixelColor { get; set; }
        public Color BgColor { get; set; }
        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
        [XmlIgnore]
        public WriteableBitmap? imageCanvasBitmap { get; set; }
        public int Thickness { get; set; }
        public abstract void Draw();
        public bool Antialiasing { get; set; }

        public bool ThickLine { get; set; }
        public void PutSinglePixel(Point point)
        {
            // source: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?view=windowsdesktop-8.0
            if (imageCanvasBitmap == null)
            {
                MessageBox.Show("CanvasBitmap not set");
                return;
            }

            int column = point.X;
            int row = point.Y;
            Color color = point.PixelColor;

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
        protected void PutPixel(Point point)
        {
            int radius;
            if (Thickness == 1 || ThickLine == false)
            {
                PutSinglePixel(point);
                return;
            }

            if (Thickness % 2 == 0)
                radius = (++Thickness) / 2;
            else
                radius = Thickness / 2;

            double x, y;
            int numPoints = Thickness * Thickness;

            for (double i = 0; i < numPoints; i++)
            {
                double theta = 2 * Math.PI * i / numPoints;
                x = radius * Math.Cos(theta);
                y = radius * Math.Sin(theta);
                PutSinglePixel(new Point((int)x + point.X, (int)y + point.Y, point.PixelColor));
            }
        }
        public abstract bool IsSelected(int x, int y);
        public int getDistance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }

    [Serializable]
    public class Line : Shape
    {
        public Line()
        {
            Thickness = 1;
            ThickLine = false;
            PixelColor = Colors.Yellow;
            startPoint = new Point(-1, -1, PixelColor);
            endPoint = new Point(-1, -1, PixelColor);
            Antialiasing = false;
            BgColor = MainWindow.defaultBgColor;
        }


        private void CalculateMidpointLineAlgorithm()
        {

            // Source: https://www.geeksforgeeks.org/mid-point-line-generation-algorithm/

            int X1 = this.startPoint.X;
            int X2 = this.endPoint.X;
            int Y1 = this.startPoint.Y;
            int Y2 = this.endPoint.Y;

            int dx = Math.Abs(X2 - X1);
            int dy = Math.Abs(Y2 - Y1);

            bool negativeY = false, negativeX = false;

            if (X1 > X2)
            {
                X1 = -X1;
                X2 = -X2;
                negativeX = true;
            }
            if (Y1 > Y2)
            {
                Y1 = -Y1;
                Y2 = -Y2;
                negativeY = true;
            }


            int x = X1, y = Y1;
            PutPixel(new Point(Math.Abs(x), Math.Abs(y), PixelColor));

            //For antialiasing
            double m = (double)dy / dx;
            double m_inv = (double)dx / dy; // slope inverse

            double y_exact = Y1;
            double x_exact = X1;

            if (dy <= dx)
            {
                int d = dy - (dx / 2);

                while (x < X2)
                {
                    x++;
                    // choose E
                    if (d < 0)
                        d = d + dy;

                    // choose NE
                    else
                    {
                        d += (dy - dx);
                        y++;
                    }
                    if (Antialiasing)
                    {
                        double modf_y_exact = y_exact - (int)y_exact;

                        byte r1 = (byte)(PixelColor.R * (1 - modf_y_exact) + BgColor.R * modf_y_exact);
                        byte g1 = (byte)(PixelColor.G * (1 - modf_y_exact) + BgColor.G * modf_y_exact);
                        byte b1 = (byte)(PixelColor.B * (1 - modf_y_exact) + BgColor.B * modf_y_exact);
                        Color c1 = Color.FromRgb(r1, g1, b1);

                        byte r2 = (byte)(PixelColor.R * modf_y_exact + BgColor.R * (1 - modf_y_exact));
                        byte g2 = (byte)(PixelColor.G * modf_y_exact + BgColor.G * (1 - modf_y_exact));
                        byte b2 = (byte)(PixelColor.B * modf_y_exact + BgColor.B * (1 - modf_y_exact));
                        Color c2 = Color.FromRgb(r2, g2, b2);

                        if (negativeY)
                        {
                            PutPixel(new Point(Math.Abs(x), Math.Abs((int)y_exact), c2));
                            PutPixel(new Point(Math.Abs(x), Math.Abs((int)y_exact) + 1, c1));
                        }
                        else
                        {
                            PutPixel(new Point(Math.Abs(x), Math.Abs((int)y_exact), c1));
                            PutPixel(new Point(Math.Abs(x), Math.Abs((int)y_exact) + 1, c2));
                        }


                        y_exact += m;
                    }
                    else
                        PutPixel(new Point(Math.Abs(x), Math.Abs(y), PixelColor));
                }
            }
            else if (dx < dy)
            {
                int d = dx - (dy / 2);

                while (y < Y2)
                {
                    y++;
                    // Choose E

                    if (d < 0)
                        d = d + dx;

                    // Choose NE
                    else
                    {
                        d += (dx - dy);
                        x++;
                    }
                    if (Antialiasing)
                    {
                        double modf_x_exact = x_exact - (int)x_exact;

                        byte r1 = (byte)(PixelColor.R * (1 - modf_x_exact) + BgColor.R * modf_x_exact);
                        byte g1 = (byte)(PixelColor.G * (1 - modf_x_exact) + BgColor.G * modf_x_exact);
                        byte b1 = (byte)(PixelColor.B * (1 - modf_x_exact) + BgColor.B * modf_x_exact);
                        Color c1 = Color.FromRgb(r1, g1, b1);

                        byte r2 = (byte)(PixelColor.R * modf_x_exact + BgColor.R * (1 - modf_x_exact));
                        byte g2 = (byte)(PixelColor.G * modf_x_exact + BgColor.G * (1 - modf_x_exact));
                        byte b2 = (byte)(PixelColor.B * modf_x_exact + BgColor.B * (1 - modf_x_exact));
                        Color c2 = Color.FromRgb(r2, g2, b2);
                        if (negativeX)
                        {
                            PutPixel(new Point(Math.Abs((int)x_exact), Math.Abs(y), c2));
                            PutPixel(new Point(Math.Abs((int)x_exact) + 1, Math.Abs(y), c1));
                        }
                        else
                        {
                            PutPixel(new Point(Math.Abs((int)x_exact), Math.Abs(y), c1));
                            PutPixel(new Point(Math.Abs((int)x_exact) + 1, Math.Abs(y), c2));
                        }


                        x_exact += m_inv;
                    }
                    else
                        PutPixel(new Point(Math.Abs(x), Math.Abs(y), PixelColor));

                    //PutPixel(new Point(Math.Abs(x), Math.Abs(y), PixelColor));

                }
            }
        }

        public override void Draw()
        {
            CalculateMidpointLineAlgorithm();
        }

        public override bool IsSelected(int x, int y)
        {
            int proximity = 10;
            int x1 = this.startPoint.X, x2 = this.endPoint.X, y1 = this.startPoint.Y, y2 = this.endPoint.Y;
            double lineLength = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            double distance = Math.Abs((x - x1) * (y2 - y1) - (y - y1) * (x2 - x1)) / lineLength;

            return distance <= proximity;
        }
    }

    [Serializable]
    public class Circle : Shape
    {
        public Circle()
        {
            Thickness = 1;
            ThickLine = false;
            PixelColor = Colors.Yellow;
            startPoint = new Point(-1, -1, PixelColor);
            endPoint = new Point(-1, -1, PixelColor);
            Antialiasing = false;
            BgColor = MainWindow.defaultBgColor;
            ;
        }

        private void CalculateMidpointCircleAlgorithm()
        {

            int radius = getRadius();
            int d = 1 - radius;
            int x = 0;
            int y = radius;

            putPoints(x, y, PixelColor); // The starting point
            while (y > x)
            {
                if (d < 0)
                    d += 2 * x + 3;
                else
                {
                    d += 2 * (x - y) + 5;
                    --y;
                }
                ++x;

                if (Antialiasing)
                {
                    double y_exact = Math.Sqrt(radius * radius - x * x);
                    int y_exact_ceil = (int)Math.Ceiling(y_exact);

                    double T = Math.Abs(y_exact_ceil - y_exact);
                    byte r2 = (byte)(PixelColor.R * (1 - T) + BgColor.R * T);
                    byte g2 = (byte)(PixelColor.G * (1 - T) + BgColor.G * T);
                    byte b2 = (byte)(PixelColor.B * (1 - T) + BgColor.B * T);
                    Color c2 = Color.FromRgb(r2, g2, b2);

                    byte r1 = (byte)(PixelColor.R * T + BgColor.R * (1 - T));
                    byte g1 = (byte)(PixelColor.G * T + BgColor.G * (1 - T));
                    byte b1 = (byte)(PixelColor.B * T + BgColor.B * (1 - T));
                    Color c1 = Color.FromRgb(r1, g1, b1);

                    putPoints(x, y_exact_ceil, c2);
                    putPoints(x, y_exact_ceil - 1, c1);

                }
                else
                {
                    putPoints(x, y, PixelColor);
                }
            }
        }

        private int getRadius()
        {
            return (int)Math.Sqrt(Math.Pow(this.endPoint.X - this.startPoint.X, 2) + Math.Pow(this.endPoint.Y - this.startPoint.Y, 2));
        }

        private void putPoints(int x, int y, Color pixelColor)
        {
            PutPixel(new Point(this.startPoint.X + x, this.startPoint.Y + y, pixelColor));
            PutPixel(new Point(this.startPoint.X + y, this.startPoint.Y + x, pixelColor));
            PutPixel(new Point(this.startPoint.X - x, this.startPoint.Y + y, pixelColor));
            PutPixel(new Point(this.startPoint.X - y, this.startPoint.Y + x, pixelColor));
            PutPixel(new Point(this.startPoint.X + x, this.startPoint.Y - y, pixelColor));
            PutPixel(new Point(this.startPoint.X + y, this.startPoint.Y - x, pixelColor));
            PutPixel(new Point(this.startPoint.X - x, this.startPoint.Y - y, pixelColor));
            PutPixel(new Point(this.startPoint.X - y, this.startPoint.Y - x, pixelColor));
        }

        public override void Draw()
        {
            if (this.startPoint.X != -1 && this.endPoint.Y != -1)
            {
                CalculateMidpointCircleAlgorithm();
            }
        }

        public override bool IsSelected(int x, int y)
        {
            int radius = getRadius();
            int centerX = this.startPoint.X, centerY = this.startPoint.Y;
            double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

            return distance <= radius;
        }
    }

    [Serializable]
    public class Polygon : Shape
    {
        private Point _nextPoint;
        public Point nextPoint
        {
            get => _nextPoint;
            set
            {
                if (prevPoint == null && _nextPoint.X != -1)
                    prevPoint = _nextPoint; // If prevPoint was never initialized (the frist edge of poygon)
                _nextPoint = value;
            }
        }
        private Point? prevPoint;

        private Line? line;

        public List<Line> lineList { get; set; }

        public Polygon()
        {
            this.Thickness = 1;
            this.ThickLine = false;
            this.PixelColor = Colors.Yellow;
            _nextPoint = new Point(-1, -1, PixelColor);
            lineList = new List<Line>();
            Antialiasing = false;
            BgColor = MainWindow.defaultBgColor;
        }

        private void CalculatePolygonPoints()
        {
            line = new Line();
            // Assimulate line to our Polygon settings
            line.PixelColor = this.PixelColor;
            line.Thickness = this.Thickness;
            line.ThickLine = this.ThickLine;
            line.Antialiasing = this.Antialiasing;
            line.imageCanvasBitmap = imageCanvasBitmap;

            // Set points
            line.startPoint = prevPoint!;
            if (lastEdge(nextPoint))
                line.endPoint = lineList.First().startPoint;
            else
                line.endPoint = nextPoint;

            line.Draw();
            prevPoint = line.endPoint; // Keep the previous edge's endpoint

            lineList.Add(line); // Possible redrawing duplication of one pixel on edge joints but not an issue
        }

        public bool lastEdge(Point point)
        {
            if (lineList != null && lineList.Count > 0)
                return getDistance(lineList.First().startPoint, point) <= 10;
            return false;
        }


        public override void Draw()
        {
            foreach (Line line in lineList)
            {
                line.imageCanvasBitmap = imageCanvasBitmap;
                line.Antialiasing = this.Antialiasing;
                line.ThickLine = this.ThickLine;
                line.Thickness = this.Thickness;
                line.PixelColor = this.PixelColor;
                line.Draw();
            }

        }

        public void LineDraw()
        {
            CalculatePolygonPoints();
        }

        public override bool IsSelected(int x, int y)
        {
            foreach (Line line in lineList)
            {
                if (line.IsSelected(x, y))
                    return true;
            }
            return false;
        }
    }

    public class LabPartClass : Shape
    {
        public Point point0 { get; set; }
        public Point point1 { get; set; }
        public Point point2 { get; set; }
        public Point point3 { get; set; }

        public LabPartClass(bool antialiasing = false)
        {
            Thickness = 1;
            ThickLine = false;
            PixelColor = Colors.Yellow;
            point0 = new Point(-1, -1, PixelColor);
            point1 = new Point(-1, -1, PixelColor);
            point2 = new Point(-1, -1, PixelColor);
            point3 = new Point(-1, -1, PixelColor);

            Antialiasing = antialiasing;
            BgColor = Colors.Black;
        }

        public override void Draw()
        {
            InterpolatePoints();
        }
        private void InterpolatePoints()
        {
            for (double t = 0; t <= 1; t += 0.01)
            {
                double x = Math.Pow((1 - t), 3) * point0.X + 3 * Math.Pow((1 - t), 2) * t * point1.X + 3 * (1 - t) * t * t * point2.X + Math.Pow(t, 3) + Math.Pow(t, 3) * point3.X;
                double y = Math.Pow((1 - t), 3) * point0.Y + 3 * Math.Pow((1 - t), 2) * t * point1.Y + 3 * (1 - t) * t * t * point2.Y + Math.Pow(t, 3) + Math.Pow(t, 3) * point3.Y;

                PutSinglePixel(new Point((int)x, (int)y, PixelColor));
            }
        }

        public override bool IsSelected(int x, int y)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class Rectangle : Shape
    {
 
        public List<Line> lineList { get; set; }

        public Rectangle()
        {
            this.Thickness = 1;
            this.ThickLine = false;
            this.PixelColor = Colors.Yellow;
            startPoint = new Point(-1, -1, PixelColor);
            endPoint = new Point(-1, -1, PixelColor);
            lineList = new List<Line>();
            Antialiasing = false;
            BgColor = MainWindow.defaultBgColor;
        }

        private void drawLine(Point point1, Point point2)
        {
             Line line = new Line();
            // Assimulate line to our Polygon settings
            line.PixelColor = this.PixelColor;
            line.Thickness = this.Thickness;
            line.ThickLine = this.ThickLine;
            line.Antialiasing = this.Antialiasing;
            line.imageCanvasBitmap = imageCanvasBitmap;

            line.startPoint = point1;
            line.endPoint = point2;

            line.Draw();

            lineList.Add(line);
        }

        private void drawRectangle()
        {
            int x1, x2, y1, y2;
            if(startPoint.Y < endPoint.Y && startPoint.X < endPoint.X)
            {
                x1 = startPoint.X;
                y1 = startPoint.Y;
                x2 = endPoint.X;
                y2 = endPoint.Y;
            }
            else if (startPoint.Y > endPoint.Y && startPoint.X > endPoint.X)
            {
                x2 = startPoint.X;
                y2 = startPoint.Y;
                x1 = endPoint.X;
                y1 = endPoint.Y;
            }
            else if(startPoint.Y < endPoint.Y && startPoint.X > endPoint.X)
            {
                x2 = startPoint.X;
                y1 = startPoint.Y;
                x1 = endPoint.X;
                y2 = endPoint.Y;
            }
            else if (startPoint.Y > endPoint.Y && startPoint.X < endPoint.X)
            {
                x1 = startPoint.X;
                y2 = startPoint.Y;
                x2 = endPoint.X;
                y1 = endPoint.Y;
            }
            else
            {
                MessageBox.Show("Can't draw a rectangle from selected points");
                return;
            }
            // Clear the previous lines the list
            lineList.Clear();
            // Draw rectangle
            drawLine(new Point(x1, y1, this.PixelColor), new Point(x2, y1, this.PixelColor)); // Top horizontal line
            drawLine(new Point(x2, y1, this.PixelColor), new Point(x2, y2, this.PixelColor)); // Right vertical line
            drawLine(new Point(x1, y2, this.PixelColor), new Point(x2, y2, this.PixelColor)); // Bottom horizontal line
            drawLine(new Point(x1, y1, this.PixelColor), new Point(x1, y2, this.PixelColor)); // Left vertical line

        }


        public override void Draw()
        {
            drawRectangle();

        }

        public override bool IsSelected(int x, int y)
        {
            Point TopLeft = lineList[0].startPoint;
            Point BottomRight = lineList[1].endPoint;

            return x >= TopLeft.X && x <= BottomRight.X &&
            y >= TopLeft.Y && y <= BottomRight.Y;
        }
    }
}


