using nuitrack;

namespace SensorServer.DepthCamera
{
    interface ISkeletonFilter
    {
        bool ShouldDiscardSkeleton(Skeleton skeleton);
    }
}