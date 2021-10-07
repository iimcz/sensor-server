namespace DepthCamera{
    class User{
        public UserHand LeftHand;
        public UserHand RightHand;
        public User()
        {
            LeftHand = new UserHand();
            RightHand = new UserHand();
        }
    }
}