//using Priority_Queue;
using UnityEngine;
using Unity.Mathematics;

namespace Client
{
    public static class VecExt
    {
        public static int2 ToInt2(this Vector2Int vec)
        {
            return new int2(vec.x, vec.y);
        }
        public static Vector2 ToVector2(this int2 value)
        {
            return new Vector2(value.x, value.y);
        }
    }
    
}


