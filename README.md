# Kinect Drawing
A simple drawing app using Kinect, C#, and XAML.

This short demo project will show you the following:
* How to track a hand using Kinect version 2.
* How to display the camera stream.
* How to draw the trail of the hand on top of a XAML canvas.

## Prerequisites
Kinect version 2
* [Kinect for Windows v2 sensor](http://amzn.to/1DQtBSV) or [Kinect for XBOX sensor](http://amzn.to/1AvdswC) with an [adapter](http://amzn.to/1wPJG55)
* [Kinect for Windows SDK v2](http://www.microsoft.com/en-us/download/details.aspx?id=44561)
* Visual Studio 2013 or higher

## How to run
Simply download the project and hit "Start" using Visual Studio. This project is a showcase demo.

## XAML Canvas

		<Image Name="camera" />
		<Canvas Name="canvas">
			<local:DrawingBrush x:Name="brush" Width="300" Height="300" />
			<Polyline Name="trail" Stroke="Red" StrokeThickness="15">
				<Polyline.Effect>
					<BlurEffect Radius="20" />
				</Polyline.Effect>
			</Polyline>
		</Canvas>

## Drawing

		Joint handRight = body.Joints[JointType.HandRight];

		if (handRight.TrackingState != TrackingState.NotTracked)
		{
			CameraSpacePoint handRightPosition = handRight.Position;
			ColorSpacePoint handRightPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);

			float x = handRightPoint.X;
			float y = handRightPoint.Y;

			if (!float.IsInfinity(x) && ! float.IsInfinity(y))
			{
				// DRAW!
				trail.Points.Add(new Point { X = x, Y = y });
			}
		}

## Contributors
* [Vangos Pterneas](http://pterneas.com) from [LightBuzz](http://lightbuzz.com)

## License
You are free to use this source code in personal and commercial projects. Licensed under the [MIT License](https://github.com/LightBuzz/Kinect-Drawing/blob/master/LICENSE).