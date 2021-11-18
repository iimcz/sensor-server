using System;

namespace DepthCamera{
    class Hand
    {
        public int X;
        public int Y;
        public float RealX;
        public float RealY;
        private static readonly int _verticalTolerance = 1;
        private static readonly int _horizontalTolerance = 1;
        private static readonly float _stepSize = 0.2f;
        public Hand(float x, float y)
        {
            X = (int)(x * 10);
            //X /= 2;
            Y = (int)(y * 10);
            //Y /= 2;
            RealX = x;
            RealY = y;
        }

        public HandMovement CalculateMovement(Hand hand){
            float horizontalMovement = RealX - hand.RealX;
            float verticalMovement = RealY - hand.RealY;

            HandMovement res = HandMovement.None;

            if(Math.Abs(horizontalMovement) > _stepSize && Math.Abs(verticalMovement) > _stepSize)
            {
                if (Math.Abs(horizontalMovement) > Math.Abs(verticalMovement))
                {
                    if (horizontalMovement < 0) res = HandMovement.Left;
                    if (horizontalMovement > 0) res = HandMovement.Right;
                }
                else
                {
                    if (verticalMovement < 0) res = HandMovement.Down;
                    if (verticalMovement > 0) res = HandMovement.Up;
                }
            }
            else if (Math.Abs(horizontalMovement) > _stepSize)
            {
                if (horizontalMovement < 0) res = HandMovement.Left;
                if (horizontalMovement > 0) res = HandMovement.Right;
            }
            else if (Math.Abs(verticalMovement) > _stepSize)
            {
                if (verticalMovement < 0) res = HandMovement.Down;
                if (verticalMovement > 0) res = HandMovement.Up;
            }

            Console.WriteLine(res);
            return res;
        }
        /*public HandMovement CalculateMovement(Hand hand)
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
            if(X < hand.X && Y == hand.Y + _horizontalTolerance){
                hand.Y -= _horizontalTolerance;
                return HandMovement.Right;
            }
            if(X < hand.X && Y == hand.Y - _horizontalTolerance){
                hand.Y += _horizontalTolerance;
                return HandMovement.Right;
            }

            if(X > hand.X && Y == hand.Y) return HandMovement.Left;
            if(X > hand.X && Y == hand.Y + _horizontalTolerance){
                hand.Y -= _horizontalTolerance;
                return HandMovement.Left;
            }
            if(X > hand.X && Y == hand.Y - _horizontalTolerance){
                hand.Y += _horizontalTolerance;
                return HandMovement.Left;
            }

            return HandMovement.None;
        }*/
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