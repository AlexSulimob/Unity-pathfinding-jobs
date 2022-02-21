using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace Client
{
    [BurstCompile]
    public struct FindPathBreadthFirstJob : IJob
    {
        [ReadOnly]
        public NativeMultiHashMap<int, NodeConnection> m_graph;
        public int StartNode;
        public int EndNode;
        public NativeList<int> Result;
        public int nodesCount;

        bool hasExit;
        
        public void Execute()
        {
            hasExit = false;

            NativeQueue<int> frontier = new NativeQueue<int>(Allocator.Temp);
            frontier.Enqueue(StartNode);

            NativeArray<bool> visited = new NativeArray<bool>(nodesCount, Allocator.Temp);
            NativeArray<int> cameFrom = new NativeArray<int>(nodesCount, Allocator.Temp);

            visited[StartNode] = true;
            
            while (!frontier.IsEmpty())
            {
                int current = frontier.Dequeue();
                
                if (current == EndNode)
                {
                    hasExit = true;
                    break;
                }
                var conncetions = m_graph.GetValuesForKey(current);
                foreach (var con in conncetions)
                {
                    if( !visited[con.node.index] ) 
                    {
                        frontier.Enqueue(con.node.index);
                        visited[con.node.index] = true;
                        cameFrom[con.node.index] = current;
                    }
                }
            }

            if (hasExit)
            {
                int currentPathNode = EndNode;
                Result.Add(currentPathNode);
                while (currentPathNode != StartNode)
                {
                    currentPathNode = cameFrom[currentPathNode];
                    Result.Add(currentPathNode);
                }
                Result.Add(StartNode);
            }

            frontier.Dispose();
            visited.Dispose();
            cameFrom.Dispose();
        }
    }
}
