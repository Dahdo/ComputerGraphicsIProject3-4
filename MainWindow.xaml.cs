using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace ComputerGraphicsProject2_4
{
    
    public partial class MainWindow : Window
    {
        // Rasterization
        private WriteableBitmap imageCanvasBitmap;
        List<Shape> shapes = new List<Shape>();
        private Line currentLine;
        private Circle currentCircle;
        private Polygon currentPolygon;
        private LabPartClass currentLabPart;
        public static System.Windows.Media.Color defaultBgColor = Colors.Black;

        int mouseDownCount = 0;
        string selectedShape = "line"; // Line selected by default
        private static int shapeThickness = 1;
        private Point rightClickedPoint;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeRasterizationBitmap();
            initNewLine(); // Selected by default
        }

        private void InitializeRasterizationBitmap()
        {
            int width = (int)ImageCanvas.Width;
            int height = (int)ImageCanvas.Height;
            imageCanvasBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            setDefaultBgColor();

            ImageCanvas.Source = imageCanvasBitmap;
        }


        private void ImageCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (selectedShape)
            {
                case "line":
                    ++mouseDownCount;
                    if (mouseDownCount == 1)
                    {
                        currentLine.startPoint.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLine.startPoint.Y = (int)e.GetPosition(ImageCanvas).Y;
                    }
                    if (mouseDownCount == 2)
                    {
                        currentLine.endPoint.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLine.endPoint.Y = (int)e.GetPosition(ImageCanvas).Y;
                        currentLine.Draw();

                        initNewLine(); // Create new Line object and reset mouseDownCount
                    }
                    break;
                case "polygon":
                    ++mouseDownCount;
                    Point tmpPoint;
                    if (mouseDownCount > 1)
                    {
                        tmpPoint = new Point((int)e.GetPosition(ImageCanvas).X,
                            (int)e.GetPosition(ImageCanvas).Y, currentPolygon.PixelColor);
                        currentPolygon.nextPoint = tmpPoint;
                        currentPolygon.LineDraw();
                    }
                    else
                    {
                        tmpPoint = new Point((int)e.GetPosition(ImageCanvas).X,
                            (int)e.GetPosition(ImageCanvas).Y, currentPolygon.PixelColor);
                        currentPolygon.nextPoint = tmpPoint;
                    }
                    if (currentPolygon.lastEdge(tmpPoint))
                    {
                        initNewPolygon(); // Create new Polygon object and reset mouseDownCount
                    }

                    break;
                case "circle":
                    ++mouseDownCount;
                    if (mouseDownCount == 1)
                    {
                        currentCircle.startPoint.X = (int)e.GetPosition(ImageCanvas).X;
                        currentCircle.startPoint.Y = (int)e.GetPosition(ImageCanvas).Y;
                    }
                    if (mouseDownCount == 2)
                    {
                        currentCircle.endPoint.X = (int)e.GetPosition(ImageCanvas).X;
                        currentCircle.endPoint.Y = (int)e.GetPosition(ImageCanvas).Y;
                        currentCircle.Draw();

                        initNewCircle(); // Create new Circle object and reset mouseDownCount
                    }
                    break;

                case "labpart":
                    ++mouseDownCount;
                    if (mouseDownCount == 1)
                    {
                        currentLabPart.point0.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLabPart.point0.Y = (int)e.GetPosition(ImageCanvas).Y;
                    }
                    if (mouseDownCount == 2)
                    {
                        currentLabPart.point1.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLabPart.point1.Y = (int)e.GetPosition(ImageCanvas).Y;
                    }
                    if (mouseDownCount == 3)
                    {
                        currentLabPart.point2.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLabPart.point2.Y = (int)e.GetPosition(ImageCanvas).Y;
                    }
                    if (mouseDownCount == 4)
                    {
                        currentLabPart.point3.X = (int)e.GetPosition(ImageCanvas).X;
                        currentLabPart.point3.Y = (int)e.GetPosition(ImageCanvas).Y;
                        currentLabPart.Draw();
                        initNewLabPart();
                    }
                    break;
            }

        }

        private void initNewLine()
        {
            mouseDownCount = 0;
            currentLine = new Line();
            currentLine.imageCanvasBitmap = imageCanvasBitmap;
            if (AntiAliasingCheckBox != null)
                currentLine.Antialiasing = AntiAliasingCheckBox.IsChecked ?? false;
            if (ThickLineCheckBox != null)
                currentLine.ThickLine = ThickLineCheckBox.IsChecked ?? false;
            currentLine.Thickness = shapeThickness;
            shapes.Add(currentLine);
        }
        private void initNewCircle()
        {
            mouseDownCount = 0;
            currentCircle = new Circle();
            currentCircle.imageCanvasBitmap = imageCanvasBitmap;
            if (AntiAliasingCheckBox != null)
                currentCircle.Antialiasing = AntiAliasingCheckBox.IsChecked ?? false;
            if (ThickLineCheckBox != null)
                currentCircle.ThickLine = ThickLineCheckBox.IsChecked ?? false;
            currentCircle.Thickness = shapeThickness;
            shapes.Add(currentCircle);
        }
        private void initNewPolygon()
        {
            mouseDownCount = 0;
            currentPolygon = new Polygon();
            currentPolygon.imageCanvasBitmap = imageCanvasBitmap;
            if (AntiAliasingCheckBox != null)
                currentPolygon.Antialiasing = AntiAliasingCheckBox.IsChecked ?? false;
            if (ThickLineCheckBox != null)
                currentPolygon.ThickLine = ThickLineCheckBox.IsChecked ?? false;
            currentPolygon.Thickness = shapeThickness;
            shapes.Add(currentPolygon);
        }

        private void initNewLabPart()
        {
            mouseDownCount = 0;
            currentLabPart = new LabPartClass();
            currentLabPart.imageCanvasBitmap = imageCanvasBitmap;
            if (AntiAliasingCheckBox != null)
                currentLabPart.Antialiasing = AntiAliasingCheckBox.IsChecked ?? false;
            if (ThickLineCheckBox != null)
                currentLabPart.ThickLine = ThickLineCheckBox.IsChecked ?? false;
            shapes.Add(currentLabPart);
        }

        private void LineRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedShape = "line";
            initNewLine();
        }

        private void PolygonRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedShape = "polygon";
            initNewPolygon();
        }

        private void LabPart3RadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedShape = "labpart";
            initNewLabPart();
        }

        private void CircleRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedShape = "circle";
            initNewCircle();
        }

        private void ThickLineCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            setDefaultBgColor();
            foreach (Shape shape in shapes)
            {
                shape.ThickLine = true;
                shape.Thickness = shapeThickness;
                shape.imageCanvasBitmap = imageCanvasBitmap;
                shape.Draw();
            }

        }

        private void ThickLineCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            setDefaultBgColor();
            foreach (Shape shape in shapes)
            {
                shape.ThickLine = false;
                shape.Thickness = 1; // defaut thickness
                shape.imageCanvasBitmap = imageCanvasBitmap;
                shape.Draw();
            }
        }

        private void ThickLineTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ThickLineTextBox.Text;

            if (int.TryParse(text, out int thickness))
            {
                shapeThickness = thickness;
            }
        }

        private void AntiAliasingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            setDefaultBgColor();
            foreach (Shape shape in shapes)
            {
                shape.Antialiasing = true;
                shape.imageCanvasBitmap = imageCanvasBitmap;
                shape.Draw();
            }
        }

        private void AntiAliasingCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            setDefaultBgColor();
            foreach (Shape shape in shapes)
            {
                shape.Antialiasing = false;
                shape.imageCanvasBitmap = imageCanvasBitmap;
                shape.Draw();
            }
        }



        private void setDefaultBgColor()
        {

            try
            {
                imageCanvasBitmap.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = imageCanvasBitmap.BackBuffer;

                    int stride = imageCanvasBitmap.BackBufferStride;

                    for (int row = 0; row < imageCanvasBitmap.PixelHeight; row++)
                    {
                        for (int column = 0; column < imageCanvasBitmap.PixelWidth; column++)
                        {
                            IntPtr pPixel = pBackBuffer + row * stride + column * 3;
                            System.Windows.Media.Color color = defaultBgColor;
                            int color_data = color.R << 16 | color.G << 8 | color.B;

                            *((int*)pPixel) = color_data;
                        }
                    }

                    imageCanvasBitmap.AddDirtyRect(new Int32Rect(0, 0, imageCanvasBitmap.PixelWidth, imageCanvasBitmap.PixelHeight));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                imageCanvasBitmap.Unlock();
            }

        }

        private void ClearAllShapes_Click(object sender, RoutedEventArgs e)
        {
            setDefaultBgColor();
            shapes.Clear(); // clear the shapes off the list
        }

        private void SaveShapes_Click(object sender, RoutedEventArgs e)
        {
            // Serialize the list of shapes
            XmlSerializer serializer = new XmlSerializer(typeof(List<Shape>));
            using (FileStream stream = new FileStream("shapes.xml", FileMode.Create))
            {
                serializer.Serialize(stream, shapes);
            }
        }

        private void ImageCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(ImageCanvas).X;
            int y = (int)e.GetPosition(ImageCanvas).Y;
            rightClickedPoint = new Point(x, y, Colors.Black);
            //foreach(Shape shape in shapes)
            //{
            //    MessageBox.Show(shape.IsSelected(x, y).ToString());
            //}
            e.Handled = true;
        }
        private void ShapeDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (Shape shape in shapes)
            {
                if (shape.IsSelected(rightClickedPoint.X, rightClickedPoint.Y))
                {
                    shape.PixelColor = shape.BgColor;
                    shape.imageCanvasBitmap = imageCanvasBitmap;
                    shape.Draw();
                    shapes.Remove(shape);
                    break;
                }

            }
        }
        private void ShapeChangeColor_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRed.Text, out int red) &&
                int.TryParse(txtGreen.Text, out int green) &&
                int.TryParse(txtBlue.Text, out int blue))
            {
                // Check if the values are within the valid range (0-255)
                if (red >= 0 && red <= 255 &&
                    green >= 0 && green <= 255 &&
                    blue >= 0 && blue <= 255)
                {
                    System.Windows.Media.Color newColor = System.Windows.Media.Color.FromRgb((byte)red, (byte)green, (byte)blue);
                    foreach (Shape shape in shapes)
                    {
                        if (shape.IsSelected(rightClickedPoint.X, rightClickedPoint.Y))
                        {
                            shape.PixelColor = newColor;
                            shape.imageCanvasBitmap = imageCanvasBitmap;
                            shape.Draw();
                            break;
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid RGB values (0-255).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter numeric values for RGB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShapeChangeThickness_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtThickness.Text, out int thickness))
            {
                foreach (Shape shape in shapes)
                {
                    if (shape.IsSelected(rightClickedPoint.X, rightClickedPoint.Y))
                    {
                        shape.Thickness = thickness;
                        shape.ThickLine = true;
                        shape.imageCanvasBitmap = imageCanvasBitmap;
                        shape.Draw();
                        break;
                    }

                }
            }
            else
            {
                MessageBox.Show("Please enter a valid thickness values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}