namespace DepthCamera{
    class UserHand{
        public Hand LastHandPosition;
        public HandMovement HandMovement;
        public int HandStepCounter;
        public UserHand()
        {
            LastHandPosition = null;
            this.HandMovement = HandMovement.None;
            HandStepCounter = 0;
        }
    }
}