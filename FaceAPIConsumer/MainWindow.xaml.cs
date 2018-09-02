using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceAPIConsumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnUplaod_Click(object sender, RoutedEventArgs e)
        {
            var openImg = new OpenFileDialog { Filter = "JPEG Image(*.jpg)|*.jpg" };
            var result = openImg.ShowDialog(this);
            if (!(bool)result)
                return;
            var filePath = openImg.FileName;
            var fileUri = new Uri(filePath);
            var bitMapSourse = new BitmapImage();
            bitMapSourse.BeginInit();
            bitMapSourse.CacheOption = BitmapCacheOption.None;
            bitMapSourse.UriSource = fileUri;
            bitMapSourse.EndInit();
            FaceImage.Source = bitMapSourse;

            Title = "Detecting....";
            FaceRectangle[] facesFound = await FaceDetect(filePath);
            Title = string.Format("Found {0} Faces", facesFound.Length);


            if (facesFound.Length <= 0) return;
            var drawVisual = new DrawingVisual();
            var drawContext = drawVisual.RenderOpen();
            drawContext.DrawImage(bitMapSourse, new Rect(0, 0, bitMapSourse.Width, bitMapSourse.Height));
            var dpi = bitMapSourse.DpiX;
            var resizeFactor = 96 / dpi;
            foreach (var faceRect in facesFound)
            {
                drawContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue, 6)
                    , new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor
                        , faceRect.Width * resizeFactor, faceRect.Height * resizeFactor));

            }
            drawContext.Close();
            var renderImage = new RenderTargetBitmap((int)(bitMapSourse.PixelWidth*resizeFactor),
                (int)(bitMapSourse.PixelHeight*resizeFactor),96,96,
                PixelFormats.Pbgra32);
            renderImage.Render(drawVisual);
            FaceImage.Source=renderImage;
        }

        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("aa1b3d3ba52f4cb7aa6c2401c0068bc7", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
        private async Task<FaceRectangle[]> FaceDetect(string filePath)
        {
            try
            {
                using(var ImgStream = File.OpenRead(filePath))
                {
                    var faces = await faceServiceClient.DetectAsync(ImgStream);
                    var faceRectangles = faces.Select(f => f.FaceRectangle);
                    return faceRectangles.ToArray();
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

    }
}
