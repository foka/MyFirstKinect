using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace FirstApp
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

		private KinectSensorChooser kinectSensorChooser;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			kinectSensorChooser = new KinectSensorChooser();
			kinectSensorChooser.KinectChanged += KinectChanged;
			kinectSensorChooser.Start();
			kinectSensorChooserUI.KinectSensorChooser = kinectSensorChooser;
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
		}

		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame == null)
				{
					return;
				}

				byte[] pixels = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(pixels);

				int stride = colorFrame.Width * 4;
				image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height,
					96, 96, PixelFormats.Bgr32, null, pixels, stride);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopKinect(kinectSensorChooser.Kinect);
		}

		private void StopKinect(KinectSensor sensor)
		{
			sensor.Stop();
			if (sensor.AudioSource != null)
				sensor.AudioSource.Stop();
		}
	}
}
