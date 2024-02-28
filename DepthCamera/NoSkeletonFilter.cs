using nuitrack;

namespace SensorServer.DepthCamera
{
    class NoSkeletonFilter : ISkeletonFilter
    {
        public bool ShouldDiscardSkeleton(Skeleton skeleton)
        {
            return false;
        }
    }
}