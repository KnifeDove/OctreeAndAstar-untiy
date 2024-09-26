using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OctreeAndAstar
{
    public class PositionNode
    {
        public Vector3 value;
        public PositionNode next;
        public PositionNode previous;

        public PositionNode(Vector3 value, PositionNode previous)
        {
            this.value = value;
            next = null;
            previous = previous;
        }
    }
}
