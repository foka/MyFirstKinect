using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;

namespace FirstApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			KinectSensorChooser = new KinectSensorChooser();
			KinectSensorManager = new KinectSensorManager();

			InitializeComponent();
		}

		public KinectSensorChooser KinectSensorChooser { get; private set; }
		public KinectSensorManager KinectSensorManager { get; private set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			KinectSensorChooser.KinectChanged += KinectChanged;
			KinectSensorChooser.Start();
			kinectSensorChooserUI.KinectSensorChooser = KinectSensorChooser;
		}

		private void KinectChanged(object sender, KinectChangedEventArgs e)
		{
			if (e.OldSensor != null)
				StopKinect(e.OldSensor);
			if (e.NewSensor != null)
				StartKinect(e.NewSensor);
		}

		private void StartKinect(KinectSensor sensor)
		{
			sensor.ColorStream.Enable();
			sensor.DepthStream.Enable();
			sensor.SkeletonStream.Enable();
			sensor.AllFramesReady += sensor_AllFramesReady;

			sensor.Start();
			// ? sensor.AudioSource.Start()

			KinectSensorManager.KinectSensor = sensor;
		}

		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{ 
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame == null)
					return;

				byte[] pixels = GenerateColoredBytes(depthFrame);

				int stride = depthFrame.Width * 4;
				depthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
					96, 96, PixelFormats.Bgr32, null, pixels, stride);
			}
		}

		private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
		{
			short[] rawDepthData = new short[depthFrame.PixelDataLength];
			depthFrame.CopyPixelDataTo(rawDepthData);

			byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

			const int BlueIndex = 0;
			const int GreenIndex = 1;
			const int RedIndex = 2;

			for (int depthIndex = 0, colorIndex = 0;
				depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
				depthIndex++, colorIndex+=4)
			{
				int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
				int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

				if (depth <= 900)
				{
					pixels[colorIndex + BlueIndex] = 255;
					pixels[colorIndex + GreenIndex] = 0;
					pixels[colorIndex + RedIndex] = 0;
				}
				else if (depth > 900 && depth < 2000)
				{
					pixels[colorIndex + BlueIndex] = 0;
					pixels[colorIndex + GreenIndex] = 255;
					pixels[colorIndex + RedIndex] = 0;
				}
				else if (depth > 2000)
				{
					pixels[colorIndex + BlueIndex] = 0;
					pixels[colorIndex + GreenIndex] = 0;
					pixels[colorIndex + RedIndex] = 255;
				}


				if (player > 0)
				{
					pixels[colorIndex + BlueIndex] = Colors.Pink.B;
					pixels[colorIndex + GreenIndex] = Colors.Pink.G;
					pixels[colorIndex + RedIndex] = Colors.Pink.R;
				}
			}

			return pixels;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopKinect(KinectSensorChooser.Kinect);
		}

		private void StopKinect(KinectSensor sensor)
		{
			sensor.Stop();
			if (sensor.AudioSource != null)
				sensor.AudioSource.Stop();
		}
	}
}
