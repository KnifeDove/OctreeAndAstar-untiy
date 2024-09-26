using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace OctreeAndAstar
{
    public class AstarPathfinding
    {
        public PathfindMode pathfindMode;

        public List<Vector3> needCheckNodes;

        public Vector3 start;
        public Vector3 target;

        public int checkFrequency;

        public MinHeap open;
        public List<Point> close;

        public Stack<Vector3> path;
        private Dictionary<Vector3, int> hadCheck;

        public Octree octree;

        public AstarPathfinding(PathfindMode pathfindMode, Octree octree)
        {
            this.pathfindMode = pathfindMode;

            needCheckNodes = new List<Vector3>();

            checkFrequency = 100;

            open = new MinHeap();
            close = new List<Point>();
            path = new Stack<Vector3>();

            hadCheck = new Dictionary<Vector3, int>();

            this.octree = octree;
        }

        public IEnumerator FindPath()
        {
            bool isFind = false;
            open.Clear();
            close.Clear();
            path.Clear();

            Point p = new Point();
            p.father = null;
            p.position = start;
            p.g = 0;
            p.h = Mathf.Abs(p.position.x - target.x) + Mathf.Abs(p.position.z - target.z) + Mathf.Abs(p.position.y - target.y);
            p.f = p.g + p.h;

            hadCheck.Add(p.position, 1);
            open.Add(p);

            int num = 0;
            while (!isFind)
            {
                Point point = open.RemoveMin();
                if (point == null) break;
                Debug.Log(point.position);
                isFind = CheckSurroundPoint(point);
                num++;
                if (num > checkFrequency)
                {
                    num = 0;
                    yield return null;
                }
            }
            if (isFind)
            {
                Point po = close[close.Count - 1];
                path.Push(po.position);
                while (po.father != null)
                {
                    po = po.father;
                    path.Push(po.position);
                }
            }
            Debug.Log("寻路完成" + path.Count);
        }

        private bool CheckSurroundPoint(Point point)
        {
            close.Add(point);
            if (Vector3.Distance(point.position, target) > 1.1f * OctreeSetting.minNodeSize)
            {
                octree.AddNeedCheckNodesToAStar(this, point.position);

                Debug.Log(needCheckNodes.Count);

                for (int i = 0; i < needCheckNodes.Count; i++)
                {
                    Vector3 vector3 = needCheckNodes[i];

                    if (hadCheck.ContainsKey(vector3)) continue;

                    Point p = new Point();
                    p.father = point;
                    p.position = vector3;
                    p.g = p.father.g + 1;
                    p.h = Mathf.Abs(p.position.x - target.x) + Mathf.Abs(p.position.z - target.z) + Mathf.Abs(p.position.y - target.y);
                    p.f = p.g + p.h;

                    open.Add(p);
                    hadCheck.Add(p.position, 1);
                }
                return false;
            }
            else
            {
                Point p = new Point();
                p.father = point;
                p.position = target;

                close.Add(p);
                return true;
            }
        }

        public void InitAsVariable(Vector3 start, Vector3 target)
        {
            this.start = octree.GetNodeByPosition(this.pathfindMode, start).value;
            this.target = octree.GetNodeByPosition(this.pathfindMode, target).value;
        }
    }
}