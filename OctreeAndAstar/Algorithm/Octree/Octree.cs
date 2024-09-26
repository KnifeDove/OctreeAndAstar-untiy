using System;
using System.Collections.Generic;
using UnityEngine;


namespace OctreeAndAstar
{
    public class Octree
    {
        //起始点
        public Vector3 treeCenter;
        //初始节点的长度
        public float initialLength;
        //叶节点集合
        public Dictionary<Vector3, OctreeNode> leafNodes;
        //用于地面单位寻路的网格
        public Dictionary<Vector2, PositionNode> pavementMap;


        //根节点
        public OctreeNode tree;

        public Octree(float length, Vector3 centerPosition)
        {
            treeCenter = centerPosition;
            initialLength = length;
            leafNodes = new Dictionary<Vector3, OctreeNode>();
            pavementMap = new Dictionary<Vector2, PositionNode>();

            tree = new OctreeNode(length, centerPosition, this);
        }

        //向树的管理器中添加叶节点
        public void AddNodeToTreeAsLeaf(OctreeNode node)
        {
            leafNodes.Add(node.centerPosition, node);
            switch (node.octreeNodeState)
            {
                case OctreeNodeState.Empty:
                    break;
                case OctreeNodeState.Pavement:
                    Vector2 vector = new Vector2(node.centerPosition.x, node.centerPosition.z);
                    if (pavementMap.ContainsKey(vector))
                    {
                        PositionNode posNode = pavementMap[vector];
                        while (posNode.next != null) posNode = posNode.next;
                        posNode.next = new PositionNode(node.centerPosition, posNode);
                        posNode.next.previous = posNode;
                    }
                    else
                    {
                        pavementMap.Add(vector, new PositionNode(node.centerPosition, null));
                    }
                    break;
                default:
                    break;
            }
        }
        //从树的管理器中移除叶节点
        public void RemoveNodeToTreeAsLeaf(OctreeNode node)
        {
            leafNodes.Remove(node.centerPosition);
            switch (node.octreeNodeState)
            {
                case OctreeNodeState.Empty:
                    break;
                case OctreeNodeState.Pavement:
                    Vector2 vector = new Vector2(node.centerPosition.x, node.centerPosition.z);
                    if (pavementMap.ContainsKey(vector))
                    {
                        PositionNode posNode = pavementMap[vector];
                        while (posNode.next != null && posNode.value != node.centerPosition) posNode = posNode.next;
                        if (posNode.value != node.centerPosition) return;
                        if (posNode.previous == null)
                        {
                            if (posNode.next != null)
                            {
                                pavementMap[vector] = posNode.next;
                                posNode.next.previous = null;
                            }
                            else
                            {
                                pavementMap.Remove(vector);
                            }
                        }
                        else
                        {
                            posNode.previous.next = posNode.next;
                            posNode.next.previous = posNode.previous;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //向一个A星寻路对象中的待检查路径集合通过提供的vector3其添加周围的节点，这个vector3必须是可以一个节点的中心坐标
        public void AddNeedCheckNodesToAStar(AstarPathfinding aStarPathfinding, Vector3 centerPosition)
        {
            aStarPathfinding.needCheckNodes.Clear();
            switch (aStarPathfinding.pathfindMode)
            {
                case PathfindMode.Pavement:
                    Vector2 index = new Vector2(centerPosition.x, centerPosition.z);
                    index.x += OctreeSetting.minNodeSize;
                    ForGround(index);
                    index.x -= OctreeSetting.minNodeSize * 2;
                    ForGround(index);
                    index.x += OctreeSetting.minNodeSize;
                    index.y += OctreeSetting.minNodeSize;
                    ForGround(index);
                    index.y -= OctreeSetting.minNodeSize * 2;
                    ForGround(index);
                    break;
                case PathfindMode.Sky:
                    if (leafNodes.ContainsKey(centerPosition))
                    {
                        leafNodes[centerPosition].FillSurroundEmptyNodePositionToList(aStarPathfinding.needCheckNodes);
                    }
                    break;
            }

            void ForGround(Vector2 index)
            {
                PositionNode node;
                if (pavementMap.ContainsKey(index))
                {
                    node = pavementMap[index];
                    if (Vector3.Distance(centerPosition, node.value) < 1.5f * OctreeSetting.minNodeSize)
                    {
                        aStarPathfinding.needCheckNodes.Add(node.value);
                    }
                    while (node.next != null)
                    {
                        node = node.next;
                        if (Vector3.Distance(centerPosition, node.value) < 1.5f * OctreeSetting.minNodeSize)
                        {
                            aStarPathfinding.needCheckNodes.Add(node.value);
                        }
                    }
                }
            }
        }

        //通过寻路的模式拿到这个点所在的节点（路面就会得到x，z坐标符合的。 空中就会得到正常的）
        public PositionNode GetNodeByPosition(PathfindMode mode, Vector3 position)
        {
            PositionNode result = null;
            switch (mode)
            {
                case PathfindMode.Sky:
                    OctreeNode treeNode = tree;
                    while (treeNode.isHadSon)
                    {
                        int i;
                        if (position.z < treeNode.centerPosition.z) i = 2;
                        else i = 0;
                        if (position.x > treeNode.centerPosition.x) i++;
                        if (position.y < treeNode.centerPosition.y) i += 4;

                        treeNode = treeNode.sonNodes[i];
                    }
                    if (treeNode.octreeNodeState == OctreeNodeState.Pavement || treeNode.octreeNodeState == OctreeNodeState.Empty)
                    {
                        result = new PositionNode(treeNode.centerPosition, null);
                    }
                    break;
                case PathfindMode.Pavement:
                    Vector2 index = new Vector2();
                    index.x = treeCenter.x + (RoundUpAndSub(position.x - treeCenter.x) / OctreeSetting.minNodeSize) * OctreeSetting.minNodeSize;
                    index.y = treeCenter.z + (RoundUpAndSub(position.z - treeCenter.z) / OctreeSetting.minNodeSize) * OctreeSetting.minNodeSize;
                    if (pavementMap.ContainsKey(index))
                    {
                        PositionNode node = pavementMap[index];
                        result = node;
                        float min = Mathf.Abs(position.y - node.value.y);
                        while (node.next != null)
                        {
                            node = node.next;
                            if (min > Mathf.Abs(position.y - node.value.y))
                            {
                                result = node;
                                min = Mathf.Abs(position.y - node.value.y);
                            }
                        }
                    }
                    break;
            }
            return result;
        }

        private float RoundUpAndSub(float num)
        {
            if(num > 0)
            {
                return (float)(Math.Ceiling(num) - 0.5f);
            }
            else
            {
                return (float)(Math.Floor(num) + 0.5f);
            }
        }
    }
}
