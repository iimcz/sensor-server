using System;

namespace DepthCamera{
    class Hand
    {
        private static readonly int _verticalTolerance = 1;
        public int X;
        public int Y;
        public Hand(float x, float y)
        {
            X = (int)(x * 10);
            X /= 2;
            Y = (int)(y * 10);
        }
        public HandMovement CalculateMovement(Hand hand)
        {
            if(X == hand.X && Y == hand.Y) return HandMovement.None;
            if(Math.Abs(X - hand.X) <= _verticalTolerance && Y < hand.Y) return HandMovement.Down;
            if(Math.Abs(X - hand.X) <= _verticalTolerance && Y > hand.Y) return HandMovement.Up;
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