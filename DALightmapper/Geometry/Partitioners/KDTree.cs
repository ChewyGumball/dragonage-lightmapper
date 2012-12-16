using System.Collections.Generic;
using System.Linq;

using OpenTK;

namespace Geometry
{
    public class KDTree : PhotonPartitioner
    {
        Photon p;
        KDTree left, right;
        public KDTree(List<Photon> points)
        {
            build(points, 0);
        }
        private KDTree(List<Photon> points, int depth)
        {
            build(points, depth);
        }
        private void build(List<Photon> points, int depth)
        {
            if (points.Count == 1)
            {
                p = points[0];
                left = null;
                right = null;
            }
            else
            {
                points.OrderBy(x => KDTree.getComponentFromDepth(x.position, depth));

                int medianIndex = points.Count / 2;
                p = points[medianIndex];
                List<Photon> leftList = points.GetRange(0, medianIndex);
                List<Photon> rightList = points.GetRange(medianIndex + 1, points.Count - medianIndex - 1);

                left = new KDTree(leftList, depth + 1);                                 //This should never be null as the case with 1 point is 
                //  taken care of above
                right = rightList.Count > 0 ? new KDTree(rightList, depth + 1) : null;  //This will be null if only 2 points                              

            }
        }

        public List<Photon> nearest(int number, Vector3 point)
        {
            return findNearestNeighbours(number, point, new List<Photon>(), 0);
        }

        private List<Photon> findNearestNeighbours(int number, Vector3 point, List<Photon> currentBest, int depth)
        {
            //If we are not a leaf node and we don't have any current bests, recurse down the tree 
            if (currentBest.Count == 0 && !isLeaf())
            {
                if (getComponentFromDepth(point, depth) < getComponentFromDepth(p.position, depth))
                {
                    currentBest = left.findNearestNeighbours(number, point, currentBest, depth + 1);
                }
                else if (right != null)
                {
                    currentBest = right.findNearestNeighbours(number, point, currentBest, depth + 1);
                }
            }

            //If there aren't enough points yet, add this one and return
            if (currentBest.Count < number)
            {
                currentBest.Add(p);
            }
            //Else see if any of the points are farther
            else
            {
                currentBest.OrderByDescending(x => (x.position - point).LengthSquared);
                //If so, remove the farthest, add this one, and return
                if ((currentBest[0].position - point).LengthSquared > (p.position - point).LengthSquared)
                {
                    currentBest.RemoveAt(0);
                    currentBest.Add(p);
                }
            }

            //If we are a leaf we are done
            //Otherwise we have to check to see if there might be points on the other side of the splitting
            //  plane which are closer than the current best, farthest point
            if (!isLeaf())
            {
                currentBest.OrderByDescending(x => (x.position - point).LengthSquared);
                float difference = getComponentFromDepth(point, depth) - getComponentFromDepth(p.position, depth);
                if ((currentBest[0].position - point).LengthSquared > difference * difference)
                {
                    if (difference < 0)
                    {
                        currentBest = left.findNearestNeighbours(number, point, currentBest, depth + 1);
                    }
                    else if (right != null)
                    {
                        currentBest = right.findNearestNeighbours(number, point, currentBest, depth + 1);
                    }
                }
            }

            return currentBest;
        }

        static private float getComponentFromDepth(Vector3 point, int depth)
        {
            switch (depth % 3)
            {
                case 0: return point.X;
                case 1: return point.Y;
                case 2: return point.Z;
                default: return point.X;    //This should never happen
            }
        }
        private bool isLeaf()
        {
            //The left node is only null on leaves, the right node can be null yet this is not a leaf
            //  when there are 2 points left while building the tree 
            return left == null;
        }
    }
}
