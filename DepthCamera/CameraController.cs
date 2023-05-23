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
        private readonly IGestureDetector _gestureDetector;
        private readonly SkeletonTracker _skeletonTracker;
        private readonly DepthCameraConfiguration _depthCameraConfiguration;
        
        /// <summary>
        /// Setup depth camera
        /// </summary>
        /// <param name="DataSender">Data sender configuration</param>
        /// <param name="config">Cammera configuration</param>
        public CameraController(ProtobufCommunication DataSender, DepthCameraConfiguration config)
        {
            _depthCameraConfiguration = config;
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
                _skeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
                Console.WriteLine("Nuitrack SkeletonTracker modul created");
            }
            catch
            {
                Console.WriteLine("Cannot create Nuitrack SkeletonTracker module");
            }
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

        public enum HandSide
        {
            Left,
            Right
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

                if(rightHand.Confidence >= _depthCameraConfiguration.JointMinConfidence){
                    _dataSender.SendHandMovement(skeleton.ID, skeletonData.Timestamp, HandSide.Right, rightHandContent);
                    gestureDetected = _gestureDetector.Update(skeleton.ID, HandSide.Right, rightHandContent, out gesture);

                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData(skeleton.ID, skeletonData.Timestamp, gesture, HandSide.Right);
                    }
                }

                if(leftHand.Confidence >= _depthCameraConfiguration.JointMinConfidence){
                    _dataSender.SendHandMovement(skeleton.ID, skeletonData.Timestamp, HandSide.Left, leftHandContent);
                    gestureDetected = _gestureDetector.Update(skeleton.ID, HandSide.Left, leftHandContent, out gesture);
                    
                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData(skeleton.ID, skeletonData.Timestamp, gesture, HandSide.Left);
                    }
                }

                if (_depthCameraConfiguration.SendSkeletonData)
                {
                    foreach (Joint joint in skeleton.Joints)
                    {
                        _dataSender.SendJointRealPosition(skeleton.ID, skeletonData.Timestamp, joint);
                        _dataSender.SendJointNormalizedPosition(skeleton.ID, skeletonData.Timestamp, joint);
                        _dataSender.SendJointConfidence(skeleton.ID, skeletonData.Timestamp, joint.Type, joint.Confidence);
                    }
                }
            }
        }
    }
}