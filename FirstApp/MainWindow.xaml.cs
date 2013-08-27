using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
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
		const int skeletonCount = 6;
		Skeleton[] allSkeletons = new Skeleton[skeletonCount];

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
			sensor.AllFramesReady += sensor_AllFramesReady;
			sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;

			sensor.Start();
			// ? sensor.AudioSource.Start()

			KinectSensorManager.KinectSensor = sensor;
			KinectSensorManager.SkeletonStreamEnabled = true;
			KinectSensorManager.SkeletonTrackingMode = SkeletonTrackingMode.Seated;
			KinectSensorManager.SkeletonEnableTrackingInNearMode = true;
			KinectSensorManager.DepthStreamEnabled = true;
			KinectSensorManager.ColorStreamEnabled = true;

			sensor.ElevationAngle = -20;
		}

		void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
		}
		 
		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			var skeleton = GetFirstSkeleton(e);
			if (skeleton != null)
				SetCameraPoints(skeleton, e);
		}

		void SetCameraPoints(Skeleton skeleton, AllFramesReadyEventArgs e)
		{
			using (DepthImageFrame depth = e.OpenDepthImageFrame())
			{
				if (depth == null || KinectSensorChooser.Kinect == null)
				{
					return;
				}

				var headColorPoint = KinectSensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(
					skeleton.Joints[JointType.Head].Position, KinectSensorChooser.Kinect.ColorStream.Format);
				var leftColorPoint = KinectSensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(
					skeleton.Joints[JointType.HandLeft].Position, KinectSensorChooser.Kinect.ColorStream.Format);
				var rightColorPoint = KinectSensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(
					skeleton.Joints[JointType.HandRight].Position, KinectSensorChooser.Kinect.ColorStream.Format);

				CameraPosition(headEllipse, headColorPoint);
				CameraPosition(leftEllipse, leftColorPoint);
				CameraPosition(rightEllipse, rightColorPoint);
			}
		}

		private void CameraPosition(FrameworkElement element, ColorImagePoint point)
		{
			element.Margin = new Thickness(
				point.X - (element.Width / 2) + kinectColorViewer1.Margin.Left,
				point.Y - (element.Height / 2) + kinectColorViewer1.Margin.Top,
				0,
				0);
		}

		Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
		{
			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame == null)
					return null;

				skeletonFrame.CopySkeletonDataTo(allSkeletons);
				Skeleton first = allSkeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
					.FirstOrDefault();
				return first;
			}
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
