using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

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
		private SpeechRecognitionEngine speechRecognitionEngine;

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

			sensor.Start();
			sensor.AudioSource.Start();

			KinectSensorManager.ElevationAngle = sensor.ElevationAngle;
			KinectSensorManager.KinectSensor = sensor;
			KinectSensorManager.SkeletonStreamEnabled = true;
			KinectSensorManager.TransformSmoothParameters = new TransformSmoothParameters
			{
				Smoothing = 0.99f,
				Correction = 0.1f,
				Prediction = 0.1f,
				JitterRadius = 0.05f,
				MaxDeviationRadius = 0.05f,
			};
			KinectSensorManager.SkeletonTrackingMode = SkeletonTrackingMode.Seated;
			KinectSensorManager.SkeletonEnableTrackingInNearMode = true;
			KinectSensorManager.DepthStreamEnabled = true;
			KinectSensorManager.ColorStreamEnabled = true;

			InitializeSpeechRecognition();
		}

		private void InitializeSpeechRecognition()
		{
			var kinectRecognizer = GetKinectRecognizer();
			speechRecognitionEngine = new SpeechRecognitionEngine(kinectRecognizer.Id);

			var choices = new Choices();
			choices.Add("left", "right", "up", "down", "fuck", "ass",
				"motherfucker", "thoroughly", "shit", "beach", "bitch",
				"beech", "koorvah", "doopah", "shtchepan", "shchepan",
				"meehow");

			var grammarBuilder = new GrammarBuilder(choices) { Culture = kinectRecognizer.Culture };

			speechRecognitionEngine.LoadGrammar(new Grammar(grammarBuilder));

			speechRecognitionEngine.SpeechRecognized += SpeechRecognized;
			speechRecognitionEngine.SpeechHypothesized += SpeechHypothesized;
			speechRecognitionEngine.SpeechRecognitionRejected += SpeechRecognitionRejected;

			speechRecognitionEngine.SetInputToAudioStream(
				KinectSensorManager.KinectSensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
			speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
		}

		void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			Console.Out.WriteLine("Recognized: {0} ({1})", e.Result.Text, e.Result.Confidence);
		}

		void SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
		{
			//Console.Out.WriteLine("Hypothesized: {0} ({1})", e.Result.Text, e.Result.Confidence);
		}

		void SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
		{
//			Console.Out.WriteLine("Rejected: {0} ({1})", e.Result.Text, e.Result.Confidence);
		}

		private static RecognizerInfo GetKinectRecognizer()
		{
			foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
			{
				string value;
				recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
				if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
				{
					return recognizer;
				}
			}

			return null;
		}


		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			
		}




		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopKinect(KinectSensorChooser.Kinect);
			speechRecognitionEngine.Dispose();
		}

		private void StopKinect(KinectSensor sensor)
		{
			sensor.Stop();
			if (sensor.AudioSource != null)
				sensor.AudioSource.Stop();
		}

		private void Button1_OnClick(object sender, RoutedEventArgs e)
		{
			KinectSensorManager.KinectSensor.ElevationAngle = int.Parse(textBox1.Text);
		}
	}
}
