using nuitrack;
using System;

namespace DepthCamera
{
    class CameraController : IDisposable
    {
        private bool _finished = false;
        private readonly DataSender _dataSender;
        private readonly GestureDetector _gestureDetector;
        private readonly SkeletonTracker _skeletonTracker;
        private readonly float _minConfidence = 0.75f;

        public CameraController(DataSender DataSender)
        {
            _dataSender = DataSender;
            _gestureDetector = new GestureDetector();

            try
            {
                Nuitrack.Init();
                Console.WriteLine("Nuitrack initialized.");
            }
            catch
            {
                Console.WriteLine("Cannot initialize Nuitrack.");
            }

            try
            {
                _skeletonTracker = SkeletonTracker.Create();
                Console.WriteLine("Nuitrack SkeletonTracker modul created.");
            }
            catch
            {
                Console.WriteLine("Cannot create Nuitrack SkeletonTracker module.");
            }

            _skeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        }
        public void Dispose()
        {
            try
            {
                _skeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
                Nuitrack.Release();
                Console.WriteLine("Nuitrack released.");
            }
            catch
            {
                Console.WriteLine("Nuitrack release failed.");
            }
        }
        public void Start()
        {
            try
            {
                Nuitrack.Run();
                Console.WriteLine("Nuitrack started.");
            }
            catch
            {
                Console.WriteLine("Cannot start Nuitrack.");
            }

            while (!_finished)
            {
                Nuitrack.WaitUpdate(_skeletonTracker);
            }
        }
        public void Stop()
        {
            _finished = true;
        }

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
                    //_dataSender.SendHandMovement("1", skeleton.ID, skeletonData.Timestamp, Naki3D.Common.Protocol.HandType.HandRight, rightHandContent);
                    gestureDetected = _gestureDetector.Update(skeleton.ID, Naki3D.Common.Protocol.HandType.HandRight, rightHandContent, out gesture);

                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData("1", skeletonData.Timestamp, gesture);
                    }
                }

                if(leftHand.Confidence >= _minConfidence){
                    //_dataSender.SendHandMovement("1", skeleton.ID, skeletonData.Timestamp, Naki3D.Common.Protocol.HandType.HandLeft, leftHandContent);
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