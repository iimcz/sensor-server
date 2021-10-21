using nuitrack;
using System;

namespace DepthCamera
{
    class CameraController : IDisposable
    {
        private bool _finished = false;
        private readonly DataSender _dataSender;
        private readonly GestureDetector _gestureDetector;
        private readonly HandTracker _handTracker;
        private readonly SkeletonTracker _skeletonTracker;
        private readonly float _minConfidence = 0.5f;
        public CameraController(DataSender DataSender)
        {
            _dataSender = DataSender;
            _gestureDetector = new GestureDetector();
            try
            {
                Nuitrack.Init("");
            }
            catch
            {
                Console.WriteLine("Cannot initialize Nuitrack.");
            }
            try
            {
                _handTracker = HandTracker.Create();
                _skeletonTracker = SkeletonTracker.Create();
            }
            catch
            {
                Console.WriteLine("Cannot create Nuitrack module.");
            }
            _handTracker.OnUpdateEvent += OnHandTrackerUpdate;
            _skeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        }
        public void Dispose()
        {
            try
            {
                _handTracker.OnUpdateEvent -= OnHandTrackerUpdate;
                _skeletonTracker.OnSkeletonUpdateEvent -= OnSkeletonUpdate;
                Nuitrack.Release();
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
            }
            catch
            {
                Console.WriteLine("Cannot start Nuitrack.");
            }
            while (!_finished)
            {
                Nuitrack.Update();
            }
        }
        public void Stop()
        {
            _finished = true;
        }
        private void OnHandTrackerUpdate(HandTrackerData handTrackerData)
        {
            foreach(UserHands userHands in handTrackerData.UsersHands)
            {
                Gesture gesture = new Gesture();
                bool gestureDetected = false;
                if(userHands.LeftHand != null)
                {
                    HandContent hand = userHands.LeftHand.Value;
                    //_dataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandLeft, hand);
                    //gestureDetected = _gestureDetector.Update(userHands.UserId, Naki3D.Common.Protocol.HandType.HandLeft, hand, out gesture);
                }
                if (userHands.RightHand != null)
                {
                    HandContent hand = userHands.RightHand.Value;
                    //_dataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandRight, hand);
                    //gestureDetected = _gestureDetector.Update(userHands.UserId, Naki3D.Common.Protocol.HandType.HandRight, hand, out gesture);
                    //Console.WriteLine(hand.YReal);
                }
                if(gestureDetected)
                {
                    _dataSender.SendGestureData("1", handTrackerData.Timestamp, gesture);
                }
            }
        }
        private void OnSkeletonUpdate(SkeletonData skeletonData){
            bool gestureDetected = false;
            Gesture gesture = new Gesture();
            foreach(Skeleton skeleton in skeletonData.Skeletons){
                Joint rightHand = skeleton.GetJoint(JointType.RightHand);
                Joint leftHand = skeleton.GetJoint(JointType.LeftHand);

                if(rightHand.Confidence >= _minConfidence){ 
                    HandContent rightHandContent = new HandContent();
                    rightHandContent.X = rightHand.Proj.X;
                    rightHandContent.Y = rightHand.Proj.Y;
                    gestureDetected = _gestureDetector.Update(skeleton.ID, Naki3D.Common.Protocol.HandType.HandRight, rightHandContent, out gesture);
                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData("1", skeletonData.Timestamp, gesture);
                    }
                }

                if(leftHand.Confidence >= _minConfidence){
                    HandContent leftHandContent = new HandContent();
                    leftHandContent.X = leftHand.Proj.X;
                    leftHandContent.Y = leftHand.Proj.Y;
                    gestureDetected = _gestureDetector.Update(skeleton.ID, Naki3D.Common.Protocol.HandType.HandRight, leftHandContent, out gesture);
                    if(gestureDetected)
                    {
                        _dataSender.SendGestureData("1", skeletonData.Timestamp, gesture);
                    }
                }
            }
        }
    }
}