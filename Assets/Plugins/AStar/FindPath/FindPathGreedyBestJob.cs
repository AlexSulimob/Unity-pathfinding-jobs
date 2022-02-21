using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Client
{
    [BurstCompile]
    public struct FindPathGreedyBestJob : IJob
    {
        [ReadOnly]
        public NativeMultiHashMap<int, NodeConnection> m_graph;
        public Node StartNode;
        public Node EndNode;
        public NativeList<int> Result;
        public int nodesCount;
        
        public NativeHeap<Node, mDistanceComparer> frontier;
        bool hasExit;
        public void Execute()
        {
            hasExit = false;
            frontier.Insert(StartNode);

            NativeArray<int> came_from = new NativeArray<int>(nodesCount, Allocator.Temp);
            NativeArray<bool> visited = new NativeArray<bool>(nodesCount, Allocator.Temp);
            
            visited[StartNode.index] = true;
            // came_from.Add(StartNode);

            while (frontier.Count > 0)
            {
                // var current = frontier.Dequeue();
                var current = frontier.Pop();

                // Debug.Log("visiting  " + current);
                if (current.index == EndNode.index)
                {
                    hasExit = true;
                    break;
                }
                var conncetions = m_graph.GetValuesForKey(current.index);
                
                
                foreach (var con in conncetions)
                {
                   if( !visited[con.node.index] ) 
                   {
                        //do calculate priority
                        //calculate manhetan distance and most closed con must be enqueue last
                        int mDistancePriority = ManhattanDistance(EndNode.pos, con.node.pos);
                        var nNode = con.node;
                        nNode.mDistance = mDistancePriority;
                        frontier.Insert(nNode);

                        visited[con.node.index] = true;
                        came_from[con.node.index] = current.index;
                   }
                }
            }

            if (hasExit)
            {
                //path calculate
                int currentPathNode = EndNode.index;
                Result.Add(currentPathNode);
                while (currentPathNode != StartNode.index)
                {
                    currentPathNode = came_from[currentPathNode];
                    Result.Add(currentPathNode);
                    // Debug.Log(currentPathNode);
                }
                Result.Add(StartNode.index);

            }

            visited.Dispose();
            came_from.Dispose();
        }
        int ManhattanDistance(int2 pointOne, int2 pointTwo)
        {
            return math.abs(pointOne.x - pointTwo.x) + math.abs(pointOne.y - pointTwo.y);
        }
    }
}
