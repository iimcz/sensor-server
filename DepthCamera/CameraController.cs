using SensorServer.Configuration;
using nuitrack;
using System;

namespace SensorServer.DepthCamera
{
    /// <summary>
    /// Controller for depth camera
    /// </summary>
    class CameraController : IDisposable
    {
        private bool _finished = false;
        private readonly ProtobufCommunication _dataSender;
        private readonly GestureDetector _gestureDetector;
        private readonly SkeletonTracker _skeletonTracker;
        private readonly float _minConfidence;
        
        /// <summary>
        /// Setup depth camera
        /// </summary>
        /// <param name="DataSender">Data sender configuration</param>
        /// <param name="config">Cammera configuration</param>
        public CameraController(ProtobufCommunication DataSender, DepthCameraConfiguration config)
        {
            _minConfidence = config.JointMinConfidence;
            _dataSender = DataSender;
            _gestureDetector = new GestureDetector(config);

            try
            {
                Nuitrack.Init();
                if (config.BackgroundMode == "dynamic")
                {
                    Nuitrack.SetConfigValue("Segmentation.Background.BackgroundMode", "dynamic");
                }
                else
                {
                    Nuitrack.SetConfigValue("Segmentation.Background.BackgroundMode", "static_first_frame");
                    Nuitrack.SetConfigValue("Segmentation.Background.CalibrationFramesNumber", config.CalibrationFramesNumber.ToString());
                }
                
                Console.Write("Nuitrack initialized, ");
                Console.Write($"BackgroundMode: {Nuitrack.GetConfigValue("Segmentation.Background.BackgroundMode")}, ");
                Console.WriteLine($"CalibrationFramesNumber: {Nuitrack.GetConfigValue("Segmentation.Background.CalibrationFramesNumber")}");
            }
            catch
            {
                Console.WriteLine("Cannot initialize Nuitrack");
            }

            try
            {
                _skeletonTracker = SkeletonTracker.Create();
                Console.WriteLine("Nuitrack SkeletonTracker modul created");
            }
            catch
            {
                Console.WriteLine("Cannot create Nuitrack SkeletonTracker module");
            }

            _skeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        }
        public void Dispose()
        {
            try
            {
                _skeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
                Nuitrack.Release();
                Console.WriteLine("Nuitrack released");
            }
            catch
            {
                Console.WriteLine("Nuitrack release failed");
            }
        }

        /// <summary>
        /// Starts gesture detection
        /// </summary>
        public void Start()
        {
            try
            {
                Nuitrack.Run();
                Console.WriteLine("Nuitrack started");
            }
            catch
            {
                Console.WriteLine("Cannot start Nuitrack");
            }

            while (!_finished)
            {
                Nuitrack.WaitUpdate(_skeletonTracker);
            }
        }

        /// <summary>
        /// Stops gesture detection
        /// </summary>
        public void Stop()
        {
            _finished = true;
        }


        /// <summary>
        /// Handle skeleton update and send data abou detected gestures
        /// </summary>
        /// <param name="skeletonData">New skeleton data</param>
        private void OnSkeletonUpdate(SkeletonData skeletonData){
            bool gestureDetected = false;
            Gesture gesture = new Gesture();

            foreach(Skeleton skeleton in skeletonData.Skeletons){
                Joint rightHand = skeleton.GetJoint(JointType.RightWrist);
                HandContent rightHandContent = new();
                rightHandContent.X = rightHand.Proj.X;
                rightHandContent.Y = rightHand.Proj.Y;

                Joint leftHand = skeleton.GetJoint(JointType.LeftWrist);
                HandContent leftHandContent = new();
                leftHandContent.X = leftHand.Proj.X;
                leftHandContent.Y = leftHand.Proj.Y;

                if(rightHand.Confidence >= _minConfidence){
                    _dataSender.SendHandMovement("1", skeleton.ID, skeletonData.Timestamp, Naki3D.Common.Protocol.HandType.HandRight, rightHandContent);
                    gestureDetected = _gestureDetector.Update(skeleton.ID, Naki3D.Common.Protocol.HandType.HandRight, rightHandContent, out gesture);

                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData("1", skeletonData.Timestamp, gesture);
                    }
                }

                if(leftHand.Confidence >= _minConfidence){
                    _dataSender.SendHandMovement("1", skeleton.ID, skeletonData.Timestamp, Naki3D.Common.Protocol.HandType.HandLeft, leftHandContent);
                    gestureDetected = _gestureDetector.Update(skeleton.ID, Naki3D.Common.Protocol.HandType.HandLeft, leftHandContent, out gesture);
                    
                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData("1", skeletonData.Timestamp, gesture);
                    }
                }
            }
        }
    }
}