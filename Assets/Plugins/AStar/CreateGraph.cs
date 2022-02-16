using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Client;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
public class CreateGraph : MonoBehaviour
{    
    public Tilemap tilemap; //tilemap whitch we based to create graph
    Graph graph = new Graph();   
    Dictionary<Node, int> PlatformIds= new Dictionary<Node, int>(); 
    int _nodeIds = 0;
    // NativeHashMap<int, NativeArray<int>> graphContainer;
    NativeMultiHashMap<int, int> mgraphContainer;
    NativeMultiHashMap<int, n_node> m_Node_graphContainer;
    // Start is called before the first frame update
    void Start()
    {
        Create();
        mgraphContainer = new NativeMultiHashMap<int, int>(graph.m_Nodes.Count, Allocator.Persistent);

        m_Node_graphContainer = new NativeMultiHashMap<int, n_node>(graph.m_Nodes.Count, Allocator.Persistent);

        // graphContainer = new NativeHashMap<int, NativeArray<int>>(graph.m_Nodes.Count, Allocator.Persistent);        
        foreach (var item in graph.m_Nodes)
        {
            // graphContainer.Add(item.Index, new NativeArray<int>(item.m_Connections.Count, Allocator.Persistent) );
            foreach (var con in item.m_Connections)
            {
                // Debug.Log(con.node.Index);
                
                m_Node_graphContainer.Add(item.Index, new n_node(con.node.Index, con.node.PosInt2));
                mgraphContainer.Add(item.Index, con.node.Index);
                // graphContainer[item.Index].Add(con.node.Index);
            }
            // mgraphContainer.
        }
        // var efe = mgraphContainer.GetValuesForKey(7);
        //  foreach (var item in efe)
        // {
        //     Debug.Log(item);
        // }

        // Pathfinder.Initialize(graph);

        
    }
    private void OnDisable() {
        mgraphContainer.Dispose();
        m_Node_graphContainer.Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            var startNode = graph.GetNearestNode(new Vector2Int(-1, -3)).Index;
            var endNode = graph.GetNearestNode(new Vector2Int(-15, -9)).Index;

            
            
            NativeList<int> result = new NativeList<int>(Allocator.TempJob);

            FindPathBreadthFirstJob findPathJob = new FindPathBreadthFirstJob();
            findPathJob.m_graph = mgraphContainer;
            findPathJob.StartNode = startNode;
            findPathJob.EndNode = endNode;
            findPathJob.Result = result;

            JobHandle handle = findPathJob.Schedule();
            handle.Complete();

            if(handle.IsCompleted)
            {
                //do something
                foreach (var item in result)
                {
                    Debug.DrawRay((Vector2)graph.m_Nodes[item].PosVec2, Vector3.up, Color.red, 100f);
                }
                result.Dispose(); //free memory
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var startNode = graph.GetNearestNode(new Vector2Int(-1, -3));
            var endNode = graph.GetNearestNode(new Vector2Int(-15, -9));

            // var her = m_Node_graphContainer.GetKeyArray(Allocator.TempJob);

            NativeList<int> result = new NativeList<int>(Allocator.TempJob);
            NativeHeap<n_node, mDistanceComparer> frontier = new NativeHeap<n_node, mDistanceComparer>(Allocator.TempJob);

            FindPathAStarJob findPathJob = new FindPathAStarJob();
            findPathJob.frontier = frontier;
            findPathJob.m_graph = m_Node_graphContainer;
            findPathJob.StartNode = new n_node(startNode.Index, startNode.PosInt2);
            findPathJob.EndNode = new n_node(endNode.Index, endNode.PosInt2);
            findPathJob.Result = result;

            JobHandle handle = findPathJob.Schedule();
            handle.Complete();

            if(handle.IsCompleted)
            {
                //do something
                foreach (var item in result)
                {
                    Debug.DrawRay((Vector2)graph.m_Nodes[item].PosVec2, Vector3.up, Color.red, 100f);
                }
                result.Dispose(); //free memory
                frontier.Dispose();
            }

            // her.Dispose();
        }
    }
    void Create()
    {
        //adding point nodes 
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            var pos = position + new Vector3(0.5f, 0.5f);
            // tilemap.GetTile(pos).
            bool hasTile = !tilemap.HasTile(position) && tilemap.HasTile(position - new Vector3Int(0,1,0));

            bool isLeft = tilemap.HasTile(position + new Vector3Int(-1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
            bool isRight = tilemap.HasTile(position + new Vector3Int(1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(1,-1, 0));

            if (hasTile && isRight && isLeft)
            {
                var node = new Node(_nodeIds, position.x, position.y);
                _nodeIds++;
                node.SetType(PlatformType.OneTile);
                graph.m_Nodes.Add(node);
            }
            else if (hasTile && isLeft)
            {
                var node = new Node(_nodeIds, position.x, position.y);
                _nodeIds++;
                node.SetType(PlatformType.LeftCliff);
                graph.m_Nodes.Add(node);
            }
            else if (hasTile && isRight)
            {
                var node = new Node(_nodeIds, position.x, position.y);
                _nodeIds++;
                node.SetType(PlatformType.RightCliff);
                graph.m_Nodes.Add(node);
            }

            else if (hasTile && !isRight && !isLeft)
            {
                var node = new Node(_nodeIds, position.x, position.y);
                _nodeIds++;
                node.SetType(PlatformType.Regular);
                graph.m_Nodes.Add(node);
            }
        }

        // node platform ids setup
        int platformId = 0;
        foreach (var item in graph.m_Nodes)
        {
            if (item.PlatformType == PlatformType.LeftCliff) 
            {
                platformId++;
                var posNode = item.PosVec2; 
                int i = 0;
                bool isRightNodeReached = false;
                
                while (!isRightNodeReached)
                {
                    var node = graph.GetNode(posNode + new Vector2Int(i, 0));
                    PlatformIds.Add(node, platformId);                               
                    i++;
                    if (node.PlatformType == PlatformType.RightCliff)
                    {
                        isRightNodeReached = true;
                    }
                }
            }   
            if (item.PlatformType == PlatformType.OneTile)
            {
                platformId++;
                PlatformIds.Add(item, platformId);
            }
        }
        
        // adding connections 
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            var pos = position + new Vector3(0.5f, 0.5f);

            bool hasTile = !tilemap.HasTile(position) && tilemap.HasTile(position - new Vector3Int(0,1, 0));
            bool isLeft = tilemap.HasTile(position + new Vector3Int(-1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
            bool isRight = tilemap.HasTile(position + new Vector3Int(1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(1,-1, 0));

            //connection in between nodes
            if (hasTile && !isRight && !isLeft)
            {
                var node = graph.GetNode((Vector2Int)position);

                var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y));
                var leftNode = graph.GetNode(new Vector2Int(position.x - 1, position.y));

                if (!node.hasConnection(rightNode))
                    node.m_Connections.Add(new NodeConnection(rightNode));

                if (!node.hasConnection(leftNode))
                    node.m_Connections.Add(new NodeConnection(leftNode));

                if (!rightNode.hasConnection(node))
                    rightNode.m_Connections.Add(new NodeConnection(node));

                if (!leftNode.hasConnection(node))
                    leftNode.m_Connections.Add(new NodeConnection(node));
            }

            //one step connection from left to right clif of platform
            if (hasTile && isLeft && !isRight)
            {
                var node = graph.GetNode((Vector2Int)position);
                var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y ));
                // Debug.Log("it works");

                if (rightNode.PlatformType == PlatformType.RightCliff)
                {
                    if (!node.hasConnection(rightNode))
                        node.m_Connections.Add(new NodeConnection(rightNode));

                    if(!rightNode.hasConnection(node))
                        rightNode.m_Connections.Add(new NodeConnection(node));
                }
            }
            //fall links 
            //left fall links
            if (hasTile && isLeft)
            {
                //if nones blocks from left
                if (!tilemap.HasTile(position + new Vector3Int(-1, 0, 0)))
                {
                    bool ground = tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
                    int i = -1; 
                    int howMuch = 0;
                    while (!ground)
                    {
                        i-=1; 
                        ground = tilemap.HasTile(position + new Vector3Int(-1, i, 0));
                        howMuch = i;
                    }
                    var leftNode = graph.GetNode((Vector2Int)position);
                    var downNode = graph.GetNode(new Vector2Int(position.x - 1, position.y + howMuch + 1));
                    
                    leftNode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
                }
            }
            //right fall links
            if (hasTile && isRight)
            {
                //if nones blocks from left
                if (!tilemap.HasTile(position + new Vector3Int(1, 0, 0)))
                {
                    bool ground = tilemap.HasTile(position + new Vector3Int(1,-1, 0));
                    int i = -1; 
                    int howMuch =0;
                    while (!ground)
                    {
                        i-=1; 
                        ground = tilemap.HasTile(position + new Vector3Int(1, i, 0));
                        howMuch = i;
                    }
                    var rightnode = graph.GetNode((Vector2Int)position);
                    var downNode = graph.GetNode(new Vector2Int(position.x + 1, position.y + howMuch + 1));
                    
                    rightnode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
                }
            }
            foreach (var item in graph.m_Nodes)
            {
                foreach (var con in item.m_Connections)
                {
                    Color conColor = con.conectionsType == ConnectionsTypes.Walk ? Color.cyan : Color.magenta;
                    Debug.DrawLine((Vector2)item.PosVec2, (Vector2)con.node.PosVec2, conColor, 1000f);
                }
            }

            //jump links
            // Vector2 landingPoint;
            // if (hasTile)
            // {
            //     foreach (var item in _entitesConfigs.aiJumpsVariant)
            //     {
            //         Vector3 landingPoint = Vector2.zero;
            //         bool goodJump = physSim.SimulateJumpViewPhysics( physSim.Humanoid,
            //             pos,
            //             item.jumpSpeed,
            //             item.jumpHeight,
            //             ref landingPoint);

            //         if (goodJump)
            //         {
            //             var startJumpNode = pointGraph.GetNearest(pos); 
            //             var landingJumpNode = pointGraph.GetNearest(landingPoint); 
            //             // Debug.Log(nodeIds[startJumpNode.node]);
            //             if (nodeIds[startJumpNode.node] != nodeIds[landingJumpNode.node])
            //             {

            //                 var cost = (uint)(landingJumpNode.node.position - startJumpNode.node.position).costMagnitude;
            //                 startJumpNode.node.AddConnection(landingJumpNode.node, cost);
            //                 conecterDic[startJumpNode.node].Add(new NodeConnection(landingJumpNode.node, ConnectionsTypes.Jump, item));

            //             }
            //             goodJump = false;
            //         }    
                    
                    
            //     }

            // }

        }
    }
}

public static class mathExt
{
    public static Vector2 ToVector2(this int2 value)
    {
        return new Vector2(value.x, value.y);
    }
}