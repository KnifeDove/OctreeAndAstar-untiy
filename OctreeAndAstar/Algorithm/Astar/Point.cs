using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeAndAstar
{
    public class Point
    {
        public Point father;
        public Point son;
        public Vector3 position;
        public float f;
        public float g;
        public float h;
    }
}