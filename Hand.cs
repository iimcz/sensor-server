using System;

namespace DepthCamera{
    /// <summary>
    /// Information about hand position (coordinates)
    /// </summary>
    class Hand
    {
        public float RealX;
        public float RealY;
        private readonly float _horizontalStepSize;
        private readonly float _verticalStepSize;
        public Hand(float x, float y, float horizontalStepSize, float verticalStepSize)
        {
            RealX = x;
            RealY = y;
            _horizontalStepSize = horizontalStepSize;
            _verticalStepSize = verticalStepSize;
        }

        /// <summary>
        /// Calculate direction of hand movement
        /// </summary>
        /// <param name="hand">New hand position</param>
        /// <returns>Direction of hand movement (left, right, up, down, none)</returns>
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