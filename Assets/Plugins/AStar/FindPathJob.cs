using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace  Client
{
    public struct FindPathJob : IJob
    {
        // public NativeHashMap<int, NativeList<int>> graph; //or connections
        public NativeMultiHashMap<int, int> m_graph;
        public int StartNode;
        public int EndNode;
        public NativeList<int> Result;
        
        public void Execute()
        {
            NativeQueue<int> frontier = new NativeQueue<int>(Allocator.Temp);
            frontier.Enqueue(StartNode);

            NativeArray<int> came_from = new NativeArray<int>(m_graph.Capacity, Allocator.Temp);
            
            came_from[StartNode] = 0;
            // came_from.Add(StartNode);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                // Debug.Log("visiting  " + current);
                if (current == EndNode)
                    break;
                var conncetions = m_graph.GetValuesForKey(current);
                foreach (var con in conncetions)
                {
                   if(!came_from.Contains(con)) 
                   {
                       frontier.Enqueue(con);
                       
                       came_from[con] = current;
                    //    came_from.Add(con);
                   }
                }
            }


            int currentPathNode = EndNode;
            Result.Add(currentPathNode);
            while (currentPathNode != StartNode)
            {
                currentPathNode = came_from[currentPathNode];
                Result.Add(currentPathNode);
                // Debug.Log(currentPathNode);
            }
            Result.Add(StartNode);
            Result.Reverse();


            frontier.Dispose();
            came_from.Dispose();

        }
    }

}
