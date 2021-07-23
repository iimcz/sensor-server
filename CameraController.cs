using nuitrack;
using System;

namespace DepthCamera
{
    class CameraController : IDisposable
    {
        private bool _finished = false;
        private readonly ProtobufDataSender _protobufDataSender;
        private readonly GestureRecognizer _gestureRecognizer;
        private readonly HandTracker _handTracker;
        public CameraController(ProtobufDataSender protobufDataSender)
        {
            _protobufDataSender = protobufDataSender;
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
                _protobufDataSender.SendGestureData("1", gestureData.Timestamp, gesture);
                Console.WriteLine("Recognized {0} from user {1}", gesture.Type.ToString(), gesture.UserID);
            }
        }
        private void OnHandTrackerUpdate(HandTrackerData handTrackerData)
        {
            foreach(UserHands userHands in handTrackerData.UsersHands)
            {
                if(userHands.LeftHand != null)
                {
                    HandContent hand = userHands.LeftHand.Value;
                    _protobufDataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandLeft, hand);
                    Console.WriteLine("User {0} left hand: Position: [{1}, {2}, {3}], Click:{4}", userHands.UserId, hand.X, hand.Y, hand.ZReal, hand.Click);
                }
                if (userHands.RightHand != null)
                {
                    HandContent hand = userHands.RightHand.Value;
                    _protobufDataSender.SendHandMovement("1", userHands.UserId, handTrackerData.Timestamp, Naki3D.Common.Protocol.HandType.HandRight, hand);
                    Console.WriteLine("User {0} right hand: Position: [{1}, {2}, {3}], Click:{4}", userHands.UserId, hand.X, hand.Y, hand.ZReal, hand.Click);
                }
            }
        }
    }
}
