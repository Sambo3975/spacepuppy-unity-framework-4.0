﻿using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Pathfinding
{

    public static class PathUtil
    {

        public static PathCalculateWaitHandle Wait(this IPath path)
        {
            return new PathCalculateWaitHandle(path);
        }

        public static IPath CreatePath(this IPathSeeker seeker, Vector3 target)
        {
            if (seeker == null) throw new System.ArgumentNullException("seeker");

            return seeker.PathFactory.Create(seeker, target);
        }

        public static bool IsDone(this IPath path)
        {
            return path.Status == PathCalculateStatus.Invalid || path.Status > PathCalculateStatus.Calculating;
        }

        /// <summary>
        /// Gets the next target waypoint after currentIndex. Updating currentIndex if you've passed it.
        /// </summary>
        /// <param name="path">The path to find the next target on.</param>
        /// <param name="currentPosition">The current position of the agent</param>
        /// <param name="currentIndex">The index of the waypoint that was last targeted, 0 starts at beginning, <0 will find the best target</param>
        /// <returns>Returns the target position or a NaN vector if no target found.</returns>
        public static Vector3 GetNextTarget(this IPath path, Vector3 currentPosition, ref int currentIndex)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));

            var waypoints = path.Waypoints;
            if (waypoints == null || waypoints.Count == 0) return VectorUtil.NaNVector3;
            if (currentIndex >= waypoints.Count - 1) return path.Waypoints[path.Waypoints.Count - 1];

            if (currentIndex < 0) return GetBestTarget(path, currentPosition, ref currentIndex);

            var targ = path.Waypoints[currentIndex];
            if (currentIndex == path.Waypoints.Count - 1)
            {
                return targ;
            }

            var dir1 = targ - currentPosition;
            var dir2 = waypoints[currentIndex + 1] - targ;
            float dot = Vector3.Dot(dir1, dir2);
            if (dot < MathUtil.EPSILON)
            {
                currentIndex++;
                targ = path.Waypoints[currentIndex];
            }

            return targ;
        }


        /// <summary>
        /// Gets the next target waypoint after currentIndex. Updating currentIndex if you've passed it.
        /// </summary>
        /// <param name="path">The path to find the next target on.</param>
        /// <param name="currentPosition">The current position of the agent</param>
        /// <param name="currentIndex">The index of the waypoint that was last targeted, 0 starts at beginning, <0 will find the best target</param>
        /// <returns>Returns the target position or a NaN vector if no target found.</returns>
        public static Vector3 GetNextTarget(this IPath path, Vector3 currentPosition, ref int currentIndex, float nearEnoughDistance)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));

            var waypoints = path.Waypoints;
            if (waypoints == null || waypoints.Count == 0) return VectorUtil.NaNVector3;
            if (currentIndex >= waypoints.Count - 1) return path.Waypoints[path.Waypoints.Count - 1];

            if (currentIndex < 0) return GetBestTarget(path, currentPosition, ref currentIndex);

            var targ = path.Waypoints[currentIndex];
            if (currentIndex == path.Waypoints.Count - 1)
            {
                return targ;
            }

            var dir1 = targ - currentPosition;
            if (dir1.sqrMagnitude < (nearEnoughDistance * nearEnoughDistance))
            {
                currentIndex++;
                targ = path.Waypoints[currentIndex];
            }
            else
            {
                var dir2 = waypoints[currentIndex + 1] - targ;
                float dot = Vector3.Dot(dir1, dir2);
                if (dot < MathUtil.EPSILON)
                {
                    currentIndex++;
                    targ = path.Waypoints[currentIndex];
                }
            }

            return targ;
        }

        /// <summary>
        /// Finds the best next target by finding the nearest waypoint to the 
        /// currentPosition after currentIndex, then calculates the next target 
        /// from there.
        /// 
        /// This is beneficial if you're starting from an arbitrary point along the path.
        /// 
        /// This is slower than GetNextTarget, use that if you know your starting position.
        /// </summary>
        /// <param name="path">The path to find the next target on.</param>
        /// <param name="currentPosition">The current position of the agent</param>
        /// <returns>Returns the target position or a NaN vector if no target found.</returns>
        public static Vector3 GetBestTarget(this IPath path, Vector3 currentPosition)
        {
            int index = 0;
            return GetBestTarget(path, currentPosition, ref index);
        }

        /// <summary>
        /// Finds the best next target by finding the nearest waypoint to the 
        /// currentPosition after currentIndex, then calculates the next target 
        /// from there.
        /// 
        /// This is beneficial if you're starting from an arbitrary point along the path.
        /// 
        /// This is slower than GetNextTarget, use that if you know your starting position.
        /// </summary>
        /// <param name="path">The path to find the next target on.</param>
        /// <param name="currentPosition">The current position of the agent</param>
        /// <param name="currentIndex">The index to start searching from, pass in 0 to search entire path, 
        /// method will return this value with the selected best target</param>
        /// <returns>Returns the target position or a NaN vector if no target found.</returns>
        public static Vector3 GetBestTarget(this IPath path, Vector3 currentPosition, ref int currentIndex)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));

            var waypoints = path.Waypoints;
            if (waypoints == null || waypoints.Count == 0)
            {
                currentIndex = -1;
                return VectorUtil.NaNVector3;
            }
            if (currentIndex >= waypoints.Count - 1)
            {
                currentIndex = path.Waypoints.Count - 1;
                return path.Waypoints[currentIndex];
            }

            if (currentIndex < 0)
                currentIndex = 0;

            float dist = float.PositiveInfinity;
            for (int i = currentIndex; i < waypoints.Count; i++)
            {
                var d = (waypoints[i] - currentPosition).SetY(0f).sqrMagnitude;
                if (d < dist)
                {
                    currentIndex = i;
                    dist = d;
                }
            }

            var targ = path.Waypoints[currentIndex];
            if (currentIndex == path.Waypoints.Count - 1)
            {
                return targ;
            }

            var dir1 = targ - currentPosition;
            var dir2 = waypoints[currentIndex + 1] - targ;

            if (Vector3.Dot(dir1, dir2) < MathUtil.EPSILON)
            {
                currentIndex++;
                targ = path.Waypoints[currentIndex];
            }

            return targ;
        }

        public static float GetPathLength(this IPath path)
        {
            if (path == null) return float.PositiveInfinity;

            var lst = path.Waypoints;
            if (lst == null) return float.PositiveInfinity;

            float tot = 0;
            for (int i = 0; i < lst.Count - 1; i++) tot += Vector3.Distance(lst[i], lst[i + 1]);
            return tot;
        }

    }

}
