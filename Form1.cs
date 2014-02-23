using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Threading;

namespace WindowsFormsKinectTest
{
    public partial class Form1 : Form
    {
        private KinectSensorChooser _chooser = null;
        private Bitmap _bitmap;
        private Bitmap _drawBitmap;
        private Bitmap _depthBitmap;

        private int player = 0;

        private Game game;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            game = new Game();
        }

        void ChooserSensorChanged(object sender, KinectChangedEventArgs e)
        {
            var old = e.OldSensor;
            StopKinect(old);

            var newsensor = e.NewSensor;
            if (newsensor == null)
            {
                return;
            }

            newsensor.SkeletonStream.Disable();
            newsensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            newsensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newsensor.AllFramesReady += SensorAllFramesReady;

            try
            {
                newsensor.Start();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Kinect Not Started");
                //maybe another app is using Kinect
                _chooser.TryResolveConflict();
            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    sensor.Stop();
                    sensor.AudioSource.Stop();
                }
            }
        }

        void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (game.hasWon == false)
            {
                SensorFrameReady(e);
                video.Image = _drawBitmap;
            }
        }

        void SensorFrameReady(AllFramesReadyEventArgs e)
        {
            // if the window is displayed, show the depth buffer image
            Bitmap mask = null;
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    _depthBitmap = CreateBitMapFromDepthFrame(frame);
                    ColorFiltering filter = new ColorFiltering();
                    filter.Red = new IntRange(50, 150);
                    filter.Green = new IntRange(200, 255);
                    filter.Blue = new IntRange(150, 255);
                    // apply the filter
                    _depthBitmap = AForge.Imaging.Image.Clone(_depthBitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    mask = filter.Apply(_depthBitmap);

                       
                    Grayscale gray = new Grayscale(0.7, 0.3, 0.1);
                    mask = gray.Apply(mask);

                    Threshold threshold = new Threshold(100);
                    threshold.ApplyInPlace(mask);

                    Invert invert = new Invert();
                    invert.ApplyInPlace(mask);
                }
            }

            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    _bitmap = _drawBitmap = ImageToBitmap(frame);
                    if (mask != null)
                    {
                        ApplyMask subtract = new ApplyMask(mask);
                        _bitmap = subtract.Apply(_bitmap);
                    }

                    HSLFiltering hsl = new HSLFiltering(new IntRange(330, 30), new Range(0.5f, 1), new Range(0.1f, 1));
                    hsl.ApplyInPlace(_bitmap);

                    Grayscale gray = new Grayscale(1, 0.8, 0.8);
                    _bitmap = gray.Apply(_bitmap);


                    Mean meanFilter = new Mean();
                    meanFilter.ApplyInPlace(_bitmap);


                    BlobsFiltering blob = new BlobsFiltering();
                    blob.CoupledSizeFiltering = false;
                    blob.MinHeight = 15;
                    blob.MinWidth = blob.MinHeight;
                    blob.MaxHeight = blob.MaxWidth = 300;
                    blob.ApplyInPlace(_bitmap);

                    // locate objects using blob counter
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.ProcessImage(_bitmap);
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    
                    Pen redPen = new Pen(Color.Red, 2);
                    Pen greenPen = new Pen(Color.Green, 2);
                    Pen purplePen = new Pen(Color.OrangeRed, 3);
                    Pen pen;
                    // check each object and draw circle around objects, which
                    // are recognized as circles
                    int maxBlob = -1;
                    double maxFullness = -1;
                    for (int i = 0, n = blobs.Length; i < n; i++) 
                    {
                        float hw_ratio = (float)(blobs[i].Rectangle.Height) / blobs[i].Rectangle.Width;

                        if (hw_ratio > 0.65 && hw_ratio < 1.5 && blobs[i].Area > 200 && blobs[i].Fullness > 0.35)
                        {
                            if (blobs[i].Area > maxFullness)
                            {
                                maxBlob = i;
                                maxFullness = blobs[i].Area;
                            }
                        }
                    }

                    System.Console.WriteLine("MAX BLOB: " + maxBlob.ToString());


                    //draw to screen!
                    Grayscale gray2 = new Grayscale(0.2125, 0.7154, 0.072);
                    _drawBitmap = gray2.Apply(_drawBitmap);
                    _drawBitmap = AForge.Imaging.Image.Clone(_drawBitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(_drawBitmap);

                    AForge.Point ballPos = new AForge.Point(-1, -1);
                    if (maxBlob >= 0)
                        ballPos = blobs[maxBlob].CenterOfGravity;
                    game.DrawGridAndScore(g, ballPos);
                    
                    for (int i = 0, n = blobs.Length; i < n; i++)
                    {
                        float hw_ratio = (float)(blobs[i].Rectangle.Height) / blobs[i].Rectangle.Width;

                        if (hw_ratio > 0.65 && hw_ratio < 1.5 && blobs[i].Area > 200 && blobs[i].Fullness > 0.35)
                        {
                            AForge.Point center = blobs[i].CenterOfGravity;
                            if (maxBlob == i)
                                pen = purplePen;
                            else if (blobs[i].Fullness > 0.35)
                                continue;//pen = greenPen;
                            else
                                continue;// pen = redPen;

                            float radius = blobs[i].Rectangle.Width / 2;
                            g.DrawEllipse(pen,
                                (int)(center.X - radius),
                                (int)(center.Y - radius),
                                (int)(radius * 2),
                                (int)(radius * 2));
                            //g.DrawString(hw_ratio.ToString(), new Font("Arial", 16), new SolidBrush(Color.Yellow), 
                             //   new System.Drawing.Point((int)center.X, (int)center.Y));
                        }
                    }

                    redPen.Dispose();
                    greenPen.Dispose();
                    purplePen.Dispose();
                    g.Dispose();

                    if (game.hasWon)
                        if (player == game.winningPlayer)
                            label1.Text = "You Won!!! Congrats :)";
                        else
                            label1.Text = "You Lost... :(";

                    this.Refresh();
                }

            }
        }

        private Bitmap CreateBitMapFromDepthFrame(DepthImageFrame frame)
        {
            if (frame != null)
            {
                var bitmapImage = new Bitmap(frame.Width, frame.Height, PixelFormat.Format16bppRgb565);

                //Copy the depth frame data onto the bitmap  
                var _pixelData = new short[frame.PixelDataLength];
                frame.CopyPixelDataTo(_pixelData);
                BitmapData bmapdata = bitmapImage.LockBits(new Rectangle(0, 0, frame.Width,
                 frame.Height), ImageLockMode.WriteOnly, bitmapImage.PixelFormat);
                IntPtr ptr = bmapdata.Scan0;
                Marshal.Copy(_pixelData, 0, ptr, frame.Width * frame.Height);
                bitmapImage.UnlockBits(bmapdata);

                return bitmapImage;
            }
            return null;
        }

        private Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            if (Image == null)
            {
                System.Console.WriteLine("RETURNING NULL");
                return _bitmap;
            }
            byte[] pixeldata = new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixeldata);
            Bitmap bmap = new Bitmap(Image.Width, Image.Height, PixelFormat.Format32bppRgb);

            BitmapData bmapdata = bmap.LockBits(
                new Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.WriteOnly,
                bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bmapdata);

            return bmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_chooser == null)
            {
                _chooser = new KinectSensorChooser();
                _chooser.KinectChanged += ChooserSensorChanged;
                _chooser.Start();
            }

            if (comboBox1.SelectedItem.ToString().Contains("1"))
                player = 1;
            else
                player = -1;

            game.reset(player);

            label1.Text = "";
            //todo: reset everything!
        }
    }
}
