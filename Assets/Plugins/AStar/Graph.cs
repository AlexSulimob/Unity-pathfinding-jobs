//using Priority_Queue;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace Client
{
    public class Graph 
    {
        private NativeList<Node> _nodes = new NativeList<Node>(Allocator.Persistent);
        private NativeMultiHashMap<int, NodeConnection> graphContainer;
        public Dictionary<int, Node> Nodes { get; private set; }

        public static readonly Node emptyNode = new Node(-1, Vector2Int.zero.ToInt2());
        
        public void InitGraphContainer()
        {
            graphContainer = new NativeMultiHashMap<int, NodeConnection>(_nodes.Length , Allocator.Persistent);
            Nodes = _nodes.ToArray().ToDictionary(x=> x.index, x => x);
            // graphContainerOnInt = new NativeMultiHashMap<int, int>(nodes.Length, Allocator.Persistent);
        }
        public void DisposeContainer()
        {
            _nodes.Dispose();
            graphContainer.Dispose();
            // graphContainerOnInt.Dispose();
        }

        public void AddNode( Vector2Int pos)
        {
            _nodes.Add(new Node(_nodes.Length, pos.ToInt2()));
        }
        public void AddConnection(Node FromNode, NodeConnection toNodeCon)
        {
            if (toNodeCon.node.index != -1 && FromNode.index != -1)
            {
                graphContainer.Add(FromNode.index, toNodeCon);
                // graphContainerOnInt.Add(FromNode.index, toNodeCon.node.index);
            }
        }
        public int NodesCount()
        {
            return _nodes.Length;
        }
        #region GetNodes
        public Node GetNearestNode(Vector2Int nodePos)
        {
            NativeArray<Node> result = new NativeArray<Node>(1, Allocator.TempJob);
            GetNearestNodeJob findJob = new GetNearestNodeJob()
            {
                pos = nodePos.ToInt2(),
                nodes = this._nodes,
                Result = result,
                distance = float.MaxValue 
            }; 
            findJob.Schedule().Complete();
            Node returnValue = result[0];
            result.Dispose();

            return returnValue;
        }

        [BurstCompile]
        private struct GetNearestNodeJob : IJob
        {
            [ReadOnly]
            public int2 pos; 
            [ReadOnly]
            public NativeArray<Node> nodes;
            [NativeDisableParallelForRestriction] 
            public NativeArray<Node> Result;
            public float distance;
            public void Execute()
            {
                for (int i = 0; i < nodes.Length -1; i++)
                {
                    var nodeDistance = math.distance(pos, nodes[i].pos);
                    if (nodeDistance < distance && nodes[i].index != -1)
                    {
                        Result[0] = nodes[i];
                        distance = nodeDistance;
                    }
                }
                // var nodeDistance = math.distance(pos, nodes[index].pos);
            }
        }

        public Node GetNode(Vector2Int nodePosition)
        {
            NativeArray<Node> resultJob = new NativeArray<Node>(1, Allocator.TempJob);
            resultJob[0] = emptyNode;
            GetNodeJob getNodeJob = new GetNodeJob()
            {
                posFindNode = nodePosition.ToInt2(),
                nodes = this._nodes,
                result = resultJob
            };
            getNodeJob.Schedule().Complete();

            var retResult = resultJob[0];
            resultJob.Dispose();
            return retResult;
        }

        [BurstCompile]
        private struct GetNodeJob : IJob
        {
            public int2 posFindNode;
            public NativeArray<Node> nodes;
            
            [NativeDisableParallelForRestriction] 
            public NativeArray<Node> result;    
            public void Execute()
            {
                for (int i = 0; i < nodes.Length -1; i++)
                {
                    if ( nodes[i].pos.Equals(posFindNode) )
                    {
                        result[0] = nodes[i];
                        break;
                    }
                }
            }
        }
        #endregion
        
        #region FindPath
        public int[] FindPathBFS(Node startNode, Node endNode)
        {
            NativeList<int> result = new NativeList<int>(Allocator.TempJob);

            FindPathBreadthFirstJob findPathJob = new FindPathBreadthFirstJob()
            {
                // frontier = frontier,
                m_graph = graphContainer,
                StartNode = startNode.index,
                EndNode = endNode.index,
                nodesCount = NodesCount(),
                Result = result
            };
            findPathJob.Schedule().Complete();
            var RetValue = result.ToArray();
            result.Dispose();

            return RetValue;
        }
        public int[] FindPathGreedy(Node startNode, Node endNode)
        {
            NativeList<int> result = new NativeList<int>(Allocator.TempJob);
            NativeHeap<Node, mDistanceComparer> frontier = new NativeHeap<Node, mDistanceComparer>(Allocator.TempJob);

            FindPathGreedyBestJob findPathJob = new FindPathGreedyBestJob()
            {
                frontier = frontier,
                m_graph = graphContainer,
                StartNode = startNode,
                EndNode = endNode,
                nodesCount = NodesCount(),
                Result = result
            };
            findPathJob.Schedule().Complete();
            var RetValue = result.ToArray();
            result.Dispose();
            frontier.Dispose();
            return RetValue;
        }
        public int[] FindPathAStar(Node startNode, Node endNode)
        {
            NativeList<int> result = new NativeList<int>(Allocator.TempJob);
            NativeHeap<Node, mDistanceComparer> frontier = new NativeHeap<Node, mDistanceComparer>(Allocator.TempJob);

            FindPathAStarJob findPathJob = new FindPathAStarJob()
            {
                frontier = frontier,
                m_graph = graphContainer,
                StartNode = startNode,
                EndNode = endNode,
                nodesCount = NodesCount(),
                Result = result
            };
            findPathJob.Schedule().Complete();
            var RetValue = result.ToArray();
            result.Dispose();
            frontier.Dispose();
            return RetValue;
        }
        #endregion
    }
    
}


