using nuitrack;
using System;

namespace DepthCamera
{
    class CameraController : IDisposable
    {
        private bool _finished = false;
        private readonly DataSender _dataSender;
        private readonly GestureRecognizer _gestureRecognizer;
        private readonly HandTracker _handTracker;
        public CameraController(DataSender DataSender)
        {
            _dataSender = DataSender;
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
                _gestureRecognizer = GestureRecognizer.Create();
                _handTracker = HandTracker.Create();
            }
            catch
            {
                Console.WriteLine("Cannot create Nuitrack module.");
            }
            _gestureRecognizer.OnNewGesturesEvent += OnNewGestures;
            _handTracker.OnUpdateEvent += OnHandTrackerUpdate;
        }
        public void Dispose()
        {
            try
            {
                _gestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
                _handTracker.OnUpdateEvent -= OnHandTrackerUpdate;
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
        private void OnNewGestures(GestureData gestureData)
        {
            foreach (Gesture gesture in gestureData.Gestures)
            {
                _dataSender.SendGestureData("1", gestureData.Timestamp, gesture);
            }
        }
        private void OnHandTrackerUpdate(HandTrackerData handTrackerData)
        {
            foreach(UserHands userHands in handTrackerData.UsersHands)
            {
                if(userHands.LeftHand != null)
                {
                    HandContent hand = userHands.LeftHand.Value;
                    _dataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandLeft, hand);
                }
                if (userHands.RightHand != null)
                {
                    HandContent hand = userHands.RightHand.Value;
                    _dataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandRight, hand);
                }
            }
        }
    }
}
