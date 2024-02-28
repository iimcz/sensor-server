using System;
using nuitrack;
using SensorServer.Configuration;

namespace SensorServer.DepthCamera
{
    class ConeSkeletonFilter : ISkeletonFilter
    {
        ConeSkeletonFilterConfiguration _config;

        public ConeSkeletonFilter(ConeSkeletonFilterConfiguration config)
        {
            _config = config;
        }

        public bool ShouldDiscardSkeleton(Skeleton skeleton)
        {
            var root = skeleton.GetJoint(JointType.Waist);
            var angle = Math.Atan2(Math.Abs(root.Real.X), root.Real.Z) / Math.PI * 180.0;

            if (angle < _config.ConeAngle && root.Real.Z > _config.MinimumDistance && root.Real.Z < _config.MaximumDistance)
            {
                return false;
            }

            return true;
        }
    }

}