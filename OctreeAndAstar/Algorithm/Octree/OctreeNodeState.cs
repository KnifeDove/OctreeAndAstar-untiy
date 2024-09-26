using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeAndAstar
{
    public enum OctreeNodeState
    {
        Empty,    //空的
        Pavement, //贴近地面的一层用于寻路的网络
        Obstacle, //障碍物
        Ground,   //地面
    }
}
