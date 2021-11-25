using System;

namespace DepthCamera{
    class Hand
    {
        public float RealX;
        public float RealY;
        private static readonly float _horizontalStepSize = 0.1f;
        private static readonly float _verticalStepSize = 0.1f;
        public Hand(float x, float y)
        {
            RealX = x;
            RealY = y;
        }

        public HandMovement CalculateMovement(Hand hand){
            float horizontalMovement = RealX - hand.RealX;
            float verticalMovement = RealY - hand.RealY;

            HandMovement res = HandMovement.None;

            if(Math.Abs(horizontalMovement) > _horizontalStepSize && Math.Abs(verticalMovement) > _verticalStepSize)
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
            else if (Math.Abs(horizontalMovement) > _horizontalStepSize)
            {
                if (horizontalMovement < 0) res = HandMovement.Left;
                if (horizontalMovement > 0) res = HandMovement.Right;
            }
            else if (Math.Abs(verticalMovement) > _verticalStepSize)
            {
                if (verticalMovement < 0) res = HandMovement.Down;
                if (verticalMovement > 0) res = HandMovement.Up;
            }

            //if (res != HandMovement.None) Console.WriteLine(res);
            return res;
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