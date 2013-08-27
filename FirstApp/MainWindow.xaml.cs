using System.Windows;
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
