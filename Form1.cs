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

namespace WindowsFormsKinectTest
{
    public partial class Form1 : Form
    {
        private KinectSensorChooser _chooser;
        private Bitmap _bitmap;
        private Bitmap _depthBitmap;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _chooser = new KinectSensorChooser();
            _chooser.KinectChanged += ChooserSensorChanged;
            _chooser.Start();
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

            newsensor.SkeletonStream.Enable();
            newsensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            newsensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newsensor.AllFramesReady += SensorAllFramesReady;

            try
            {
                newsensor.Start();
                rtbMessages.Text = "Kinect Started" + "\r";
            }
            catch (System.IO.IOException)
            {
                rtbMessages.Text = "Kinect Not Started" + "\r";
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
            SensorFrameReady(e);
            video.Image = _bitmap;
        }


        bool isReady = true;
        void SensorFrameReady(AllFramesReadyEventArgs e)
        {
            if (isReady == false)
            {
                System.Console.WriteLine("SKIPPING");
                return;
            }

            // if the window is displayed, show the depth buffer image
            if (WindowState != FormWindowState.Minimized)
            {
                Bitmap mask = null;
                using (var frame = e.OpenDepthImageFrame())
                {
                    _depthBitmap = CreateBitMapFromDepthFrame(frame);
                    if (_depthBitmap != null)
                    {
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

                using (var frame = e.OpenColorImageFrame())
                {
                    Bitmap colorBitmap = ImageToBitmap(frame);
                    if (colorBitmap == null)
                    {
                        isReady = true;
                        return;
                    }
                    if (mask != null)
                    {
                        ApplyMask subtract = new ApplyMask(mask);
                        colorBitmap = subtract.Apply(colorBitmap);
                    }


                    // create filter
                    Median med = new Median();
                    //System.Console.WriteLine(colorBitmap.PixelFormat);
                    // apply the filter
                    med.ApplyInPlace(colorBitmap);

                    // create filter
                    ColorFiltering filter = new ColorFiltering();
                    // set color ranges to keep
                    filter.Red = new IntRange(140, 220);
                    filter.Green = new IntRange(50, 130);
                    filter.Blue = new IntRange(50, 130);
                    // apply the filter
                    //System.Console.WriteLine(colorBitmap.PixelFormat);
                    filter.ApplyInPlace(colorBitmap);

                    // create filter
                    HSLFiltering hsl = new HSLFiltering();
                    // set color ranges to keep
                    hsl.Hue = new IntRange(0, 360);
                    hsl.Saturation = new Range(0f, 1);
                    hsl.Luminance = new Range(0.5f, 1);
                    // apply the filter
                    filter.ApplyInPlace(colorBitmap);

                    
                    Grayscale gray = new Grayscale(0.4,0.7,0.1);
                    colorBitmap = gray.Apply(colorBitmap);

                    _bitmap = colorBitmap;
                    isReady = true;
                    return;
                    


                    BlobsFiltering blob = new BlobsFiltering();
                    blob.CoupledSizeFiltering = false;
                    blob.MinHeight = 20;
                    blob.MinWidth = blob.MinHeight;
                    blob.MaxHeight = blob.MaxWidth = 100;
                    blob.ApplyInPlace(colorBitmap);

                    // locate objects using blob counter
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.ProcessImage(colorBitmap);
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    // create Graphics object to draw on the image and a pen
                    colorBitmap = AForge.Imaging.Image.Clone(colorBitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(colorBitmap);
                    Pen redPen = new Pen(Color.Red, 2);
                    Pen greenPen = new Pen(Color.Green, 2);
                    Pen purplePen = new Pen(Color.Purple, 5);
                    Pen pen;
                    // check each object and draw circle around objects, which
                    // are recognized as circles
                    int maxBlob = -1;
                    double maxFullness = -1;
                    for (int i = 0, n = blobs.Length; i < n; i++)
                    {
                        float hw_ratio = (float)(blobs[i].Rectangle.Height) / blobs[i].Rectangle.Width;

                        if (hw_ratio > 0.5 && hw_ratio < 2 && blobs[i].Fullness >= 0.25)
                        {
                            if (blobs[i].Fullness > maxFullness)
                                maxBlob = i;
                        }
                    }

                    for (int i = 0, n = blobs.Length; i < n; i++)
                    {
                        float hw_ratio = (float)(blobs[i].Rectangle.Height) / blobs[i].Rectangle.Width; 

                        if (hw_ratio > 0.5 && hw_ratio < 2)
                        {
                            AForge.Point center = blobs[i].CenterOfGravity;
                            if (maxBlob == i)
                                pen = purplePen;
                            else if (blobs[i].Fullness > 0.25)
                                pen = greenPen;
                            else
                                pen = redPen;

                            float radius = blobs[i].Rectangle.Width / 2;
                            g.DrawEllipse(pen,
                                (int)(center.X - radius),
                                (int)(center.Y - radius),
                                (int)(radius * 2),
                                (int)(radius * 2));
                        }
                    }

                    redPen.Dispose();
                    g.Dispose();

                    _bitmap = colorBitmap;
                }

                isReady = true;
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
    }
}
