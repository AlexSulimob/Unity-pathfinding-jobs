using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


namespace Client
{
    [BurstCompile]
    public struct FindPathAStarJob : IJob
    {
        [ReadOnly]
        public NativeMultiHashMap<int, NodeConnection> m_graph;
        public Node StartNode;
        public Node EndNode;

        public NativeList<int> Result;
        
        public NativeHeap<Node, mDistanceComparer> frontier;
        public int nodesCount;
        bool hasExit;

        public void Execute()
        {
            hasExit = false;

            frontier.Insert(StartNode);

            NativeArray<int> came_from = new NativeArray<int>(nodesCount, Allocator.Temp);
            NativeArray<int> costSoFar = new NativeArray<int>(nodesCount, Allocator.Temp);
            NativeArray<bool> visited = new NativeArray<bool>(nodesCount, Allocator.Temp);

            visited[StartNode.index] = true;
            costSoFar[StartNode.index] = 0;
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
                    var newCost = costSoFar[current.index] + con.cost;
                    if( !visited[con.node.index] || newCost < costSoFar[con.node.index]) 
                    {
                        //do calculate priority
                        //calculate manhetan distance and most closed con must be enqueue last
                        costSoFar[con.node.index] = newCost;
                        int mDistancePriority = newCost + ManhattanDistance(EndNode.pos, con.node.pos);

                        var nNode = con.node;
                        nNode.mDistance = mDistancePriority;

                        frontier.Insert(nNode);//needs ad priority

                        visited[con.node.index] = true;
                        came_from[con.node.index] = current.index;
                    }
                }
            }


            if (hasExit)
            {
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
            // came_from.Dispose();

        }
        int ManhattanDistance(int2 pointOne, int2 pointTwo)
        {
            return math.abs(pointOne.x - pointTwo.x) + math.abs(pointOne.y - pointTwo.y);
        }
    }
}
