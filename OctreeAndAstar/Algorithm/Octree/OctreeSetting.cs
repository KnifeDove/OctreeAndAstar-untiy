using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeAndAstar
{

    public class OctreeSetting
    {
        //最小的节点的尺寸
        public const float minNodeSize = 1f;

        //检查下方有没有碰撞器的额外距离
        public const float checkBelowExtraDistance = 1f;

        public static Dictionary<int, int[]> SixFace = new Dictionary<int, int[]>()
        {
           //上下前后左右
           {4 ,  new int[4] { 0, 1, 2, 3 }},
            {-4 , new int[4] { 4, 5, 6, 7 }},
            {2 ,  new int[4] { 0, 1, 4, 5 }},
            {-2 , new int[4] { 2, 3, 6, 7 }},
            {1 ,  new int[4] { 0, 2, 4, 6 }},
           {-1 , new int[4] { 1, 3, 5, 7 }},
        };
    }
}
