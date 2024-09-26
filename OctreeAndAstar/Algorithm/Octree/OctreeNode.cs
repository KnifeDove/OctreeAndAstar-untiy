using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace OctreeAndAstar
{
    public class OctreeNode
    {
        public OctreeNodeState octreeNodeState;
        public Vector3 centerPosition;

        public float length;
        public int indexInFather;

        public OctreeNode fatherNode;
        public bool isHadSon;
        public OctreeNode[] sonNodes;

        public Bounds bounds;

        public List<OctreeNode> surroundEmptyNode;

        public OctreeNode(OctreeNode father, Vector3 centerPosition, int indexInFather, Octree tree, bool isCheck = true)
        {
            octreeNodeState = OctreeNodeState.Empty;
            this.centerPosition = centerPosition;

            length = father.length / 2;
            this.indexInFather = indexInFather;

            fatherNode = father;
            isHadSon = false;
            sonNodes = null;

            bounds = new Bounds();
            bounds.center = centerPosition;
            bounds.size = new Vector3(length, length, length);

            if (isCheck)
            {
                CheckAndCreateSonNode(tree);
            }
        }

        public OctreeNode(float length, Vector3 centerPosition, Octree tree)
        {
            octreeNodeState = OctreeNodeState.Empty;
            this.centerPosition = centerPosition;

            this.length = length;
            this.indexInFather = -1;

            fatherNode = null;
            isHadSon = false;
            sonNodes = null;

            bounds = new Bounds();
            bounds.center = centerPosition;
            bounds.size = new Vector3(length, length, length);

            CheckAndCreateSonNode(tree);
        }

        public void CheckAndCreateSonNode(Octree tree)
        {
            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity);

            bool isEmpty = true;
            bool isObstacle = false;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<IEmptyInMap>() == null)
                {
                    if (colliders[i].GetComponent<IObstacleInMap>() != null)
                    {
                        isObstacle = true;
                        if (length < OctreeSetting.minNodeSize)
                        {
                            tree.AddNodeToTreeAsLeaf(this);
                            octreeNodeState = OctreeNodeState.Obstacle;
                            return;
                        }
                    }
                    isEmpty = false;
                }
            }
            //如果需要创建叶子节点
            if (length > OctreeSetting.minNodeSize && colliders.Length > 0 && !isEmpty)
            {
                isHadSon = true;
                sonNodes = new OctreeNode[8];

                //上层
                sonNodes[0] = new OctreeNode(this, new Vector3(centerPosition.x - length / 4, centerPosition.y + length / 4, centerPosition.z + length / 4), 0, tree);
                sonNodes[1] = new OctreeNode(this, new Vector3(centerPosition.x + length / 4, centerPosition.y + length / 4, centerPosition.z + length / 4), 1, tree);
                sonNodes[2] = new OctreeNode(this, new Vector3(centerPosition.x - length / 4, centerPosition.y + length / 4, centerPosition.z - length / 4), 2, tree);
                sonNodes[3] = new OctreeNode(this, new Vector3(centerPosition.x + length / 4, centerPosition.y + length / 4, centerPosition.z - length / 4), 3, tree);
                //下层
                sonNodes[4] = new OctreeNode(this, new Vector3(centerPosition.x - length / 4, centerPosition.y - length / 4, centerPosition.z + length / 4), 4, tree);
                sonNodes[5] = new OctreeNode(this, new Vector3(centerPosition.x + length / 4, centerPosition.y - length / 4, centerPosition.z + length / 4), 5, tree);
                sonNodes[6] = new OctreeNode(this, new Vector3(centerPosition.x - length / 4, centerPosition.y - length / 4, centerPosition.z - length / 4), 6, tree);
                sonNodes[7] = new OctreeNode(this, new Vector3(centerPosition.x + length / 4, centerPosition.y - length / 4, centerPosition.z - length / 4), 7, tree);

                return;
            }
            //已经是叶子节点
            tree.AddNodeToTreeAsLeaf(this);
            //内部还有障碍物
            if (!isEmpty && !isObstacle)
            {
                octreeNodeState = OctreeNodeState.Ground;
                //获取上层的最小格子（如果不是最小会切割）并且设置为路面
                OctreeNode nowNode = this;
                if (indexInFather > 3 && indexInFather < 8)
                {
                    nowNode = fatherNode.sonNodes[indexInFather - 4];
                }
                if (indexInFather > -1 && indexInFather < 4)
                {
                    Stack<int> indexForFindUp = new Stack<int>();
                    int nowIndex = indexInFather;

                    while (nowIndex < 4 && nowIndex > -1)
                    {
                        indexForFindUp.Push(nowIndex);
                        nowNode = nowNode.fatherNode;
                        nowIndex = nowNode.indexInFather;
                    }

                    if (nowIndex == -1) return;

                    nowNode = nowNode.fatherNode.sonNodes[nowIndex - 4];

                    while (indexForFindUp.Count > 0)
                    {
                        if (nowNode.isHadSon)
                            nowNode = nowNode.sonNodes[indexForFindUp.Pop() + 4];
                        else
                        {
                            tree.RemoveNodeToTreeAsLeaf(nowNode);
                            CreateSonUntilMinSize(nowNode, tree);
                        }
                    }
                }
                if (nowNode.octreeNodeState == OctreeNodeState.Empty)
                {
                    tree.RemoveNodeToTreeAsLeaf(nowNode);
                    nowNode.octreeNodeState = OctreeNodeState.Pavement;
                    tree.AddNodeToTreeAsLeaf(nowNode);
                }
                return;
            }
        }

        public void CreateSonUntilMinSize(OctreeNode node, Octree tree)
        {
            //如果是地面或者障碍物节点则不变
            if (node.octreeNodeState == OctreeNodeState.Ground || node.octreeNodeState == OctreeNodeState.Obstacle)
                return;
            //不是最小则切割
            if (node.length > OctreeSetting.minNodeSize)
            {
                node.isHadSon = true;
                node.sonNodes = new OctreeNode[8];

                //上层不需要再分
                node.sonNodes[0] = new OctreeNode(node, new Vector3(node.centerPosition.x - node.length / 4, node.centerPosition.y + node.length / 4, node.centerPosition.z + node.length / 4), 0, tree, false);
                tree.AddNodeToTreeAsLeaf(node.sonNodes[0]);
                node.sonNodes[1] = new OctreeNode(node, new Vector3(node.centerPosition.x + node.length / 4, node.centerPosition.y + node.length / 4, node.centerPosition.z + node.length / 4), 1, tree, false);
                tree.AddNodeToTreeAsLeaf(node.sonNodes[1]);
                node.sonNodes[2] = new OctreeNode(node, new Vector3(node.centerPosition.x - node.length / 4, node.centerPosition.y + node.length / 4, node.centerPosition.z - node.length / 4), 2, tree, false);
                tree.AddNodeToTreeAsLeaf(node.sonNodes[2]);
                node.sonNodes[3] = new OctreeNode(node, new Vector3(node.centerPosition.x + node.length / 4, node.centerPosition.y + node.length / 4, node.centerPosition.z - node.length / 4), 3, tree, false);
                tree.AddNodeToTreeAsLeaf(node.sonNodes[3]);
                //下层需要继续分
                node.sonNodes[4] = new OctreeNode(node, new Vector3(node.centerPosition.x - node.length / 4, node.centerPosition.y - node.length / 4, node.centerPosition.z + node.length / 4), 4, tree, false);
                CreateSonUntilMinSize(node.sonNodes[4], tree);
                node.sonNodes[5] = new OctreeNode(node, new Vector3(node.centerPosition.x + node.length / 4, node.centerPosition.y - node.length / 4, node.centerPosition.z + node.length / 4), 5, tree, false);
                CreateSonUntilMinSize(node.sonNodes[5], tree);
                node.sonNodes[6] = new OctreeNode(node, new Vector3(node.centerPosition.x - node.length / 4, node.centerPosition.y - node.length / 4, node.centerPosition.z - node.length / 4), 6, tree, false);
                CreateSonUntilMinSize(node.sonNodes[6], tree);
                node.sonNodes[7] = new OctreeNode(node, new Vector3(node.centerPosition.x + node.length / 4, node.centerPosition.y - node.length / 4, node.centerPosition.z - node.length / 4), 7, tree, false);
                CreateSonUntilMinSize(node.sonNodes[7], tree);
            }
            else
            {
                tree.AddNodeToTreeAsLeaf(node);
            }
        }

        public void FillSurroundEmptyNodePositionToList(List<Vector3> container)
        {
            if (surroundEmptyNode != null)
            {
                for (int i = 0; i < surroundEmptyNode.Count; i++)
                    container.Add(surroundEmptyNode[i].centerPosition);
                return;
            }
            else
            {
                surroundEmptyNode = new List<OctreeNode>();

                Queue<OctreeNode> fatherNodes = new Queue<OctreeNode>();
                Stack<int> indexForFind = new Stack<int>();
                OctreeNode nowNode = this;
                int nowIndex = indexInFather;

                foreach (int index in OctreeSetting.SixFace.Keys)
                {
                    fatherNodes.Clear();
                    indexForFind.Clear();
                    nowNode = this;
                    nowIndex = indexInFather;


                    if (CheckIndex(index, indexInFather))
                    {
                        while (CheckIndex(index, nowIndex))
                        {
                            indexForFind.Push(nowIndex);
                            nowNode = nowNode.fatherNode;
                            nowIndex = nowNode.indexInFather;
                        }
                        if (nowIndex != -1)
                        {
                            nowNode = nowNode.fatherNode.sonNodes[nowIndex - index];

                            while (indexForFind.Count > 0)
                            {
                                if (nowNode.isHadSon)
                                    nowNode = nowNode.sonNodes[indexForFind.Pop() + index];
                                else
                                    break;
                            }
                            fatherNodes.Enqueue(nowNode);
                        }
                    }
                    else
                        fatherNodes.Enqueue(this.fatherNode.sonNodes[indexInFather - index]);
                    while (fatherNodes.Count > 0)
                    {
                        nowNode = fatherNodes.Dequeue();
                        if (nowNode.isHadSon)
                            for (int i = 0; i < OctreeSetting.SixFace[index].Length; i++)
                                fatherNodes.Enqueue(nowNode.sonNodes[OctreeSetting.SixFace[index][i]]);
                        else
                            CheckAndAdd(nowNode);
                    }
                }
            }
            void CheckAndAdd(OctreeNode node)
            {
                if (node.octreeNodeState == OctreeNodeState.Empty || node.octreeNodeState == OctreeNodeState.Pavement)
                {
                    container.Add(node.centerPosition);
                    surroundEmptyNode.Add(node);
                }
            }
            bool CheckIndex(int index, int needCheck)
            {
                for (int i = 0; i < OctreeSetting.SixFace[index].Length; i++)
                    if (needCheck == OctreeSetting.SixFace[index][i])
                        return true;
                return false;
            }
        }
    }
}
