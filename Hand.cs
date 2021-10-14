namespace DepthCamera{
    class Hand
    {
        public int X;
        public int Y;
        private static readonly int _verticalTolerance = 1;
        public Hand(float x, float y)
        {
            X = (int)(x * 10);
            Y = (int)(y * 10);
        }
        public HandMovement CalculateMovement(Hand hand)
        {
            if(X == hand.X && Y == hand.Y) return HandMovement.None;
            if(X == hand.X && Y < hand.Y) return HandMovement.Down;
            if(X == hand.X + _verticalTolerance && Y < hand.Y){
                hand.X -= _verticalTolerance;
                return HandMovement.Down;
            }
            if(X == hand.X - _verticalTolerance && Y < hand.Y){
                hand.X += _verticalTolerance;
                return HandMovement.Down;
            }
            if(X == hand.X && Y > hand.Y) return HandMovement.Up;
            if(X == hand.X + _verticalTolerance && Y > hand.Y){
                hand.X -= _verticalTolerance;
                return HandMovement.Up;
            }
            if(X == hand.X - _verticalTolerance && Y > hand.Y){
                hand.X += _verticalTolerance;
                return HandMovement.Up;
            }
            if(X < hand.X && Y == hand.Y) return HandMovement.Right;
            if(X > hand.X && Y == hand.Y) return HandMovement.Left;
            return HandMovement.None;
        }
    }
    enum HandMovement
    {
        None,
        Left,
        Right,
        Up,
        Down
    }
}