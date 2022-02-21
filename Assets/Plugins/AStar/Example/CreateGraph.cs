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
    Graph graph;   
    Dictionary<Node, int> PlatformIds= new Dictionary<Node, int>(); 
    int _nodeIds = 0;
    public Vector2Int testStart;
    public Vector2Int testEnd;
    
    JobHandle handle1;
    NativeList<int> result;
    NativeHeap<Node, mDistanceComparer> frontier;
    void Start()
    {
        graph = new Graph();
        CreateTileGraph();
        // graph.InitGraphContainer();
        // Debug.Log("done");
    }
    private void OnDisable() 
    {
        graph.DisposeContainer();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // var startNode = graph.GetNode(new Vector2Int(-17, -9));
            // var endNode = graph.GetNode(new Vector2Int(0, -3));
            var startNodeBFS = graph.GetNearestNode(testStart);
            var endNodeBFS = graph.GetNearestNode(testEnd);

            var pathBFS = graph.FindPathBFS(startNodeBFS, endNodeBFS);
            foreach (var item in pathBFS)
            {
                Debug.DrawRay(graph.Nodes[item].pos.ToVector2(), Vector3.up * 0.2f, Color.red, 10f);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var startNode = graph.GetNearestNode(testStart);
            var endNode = graph.GetNearestNode(testEnd);

            var path = graph.FindPathAStar(startNode, endNode);
            foreach (var item in path)
            {
                Debug.DrawRay(graph.Nodes[item].pos.ToVector2(), Vector3.up * 0.2f, Color.red, 10f);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            
            var startNodeGreedy = graph.GetNearestNode(testStart);
            var endNodeGreedy = graph.GetNearestNode(testEnd);

            var pathGreedy = graph.FindPathGreedy(startNodeGreedy, endNodeGreedy);
            foreach (var item in pathGreedy)
            {
                Debug.DrawRay(graph.Nodes[item].pos.ToVector2(), Vector3.up * 0.2f, Color.red, 10f);
            }
        }
    }
    void CreateTileGraph()
    {
        // int count = 0; 
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            // count +=1;
            graph.AddNode( (Vector2Int)position );
        }
        graph.InitGraphContainer();
        // Debug.Log(count);

        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            var currentNode = graph.GetNode((Vector2Int)position );

            var topNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(0, 1));
            var bottomNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(0, -1));
            var rightNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(1, 0));
            var leftNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(-1, 0));
            
            var topLeftNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(-1, 1));
            var topRightNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(1, 1));
            var bottomLeftNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(-1, -1));
            var bottomRightNeighbour = graph.GetNode((Vector2Int)position +  new Vector2Int(1, -1));

            graph.AddConnection(currentNode, new NodeConnection(topLeftNeighbour, 2));
            graph.AddConnection(currentNode, new NodeConnection(topRightNeighbour, 2));
            graph.AddConnection(currentNode, new NodeConnection(bottomLeftNeighbour, 2));
            graph.AddConnection(currentNode, new NodeConnection(bottomRightNeighbour, 2));

            graph.AddConnection(currentNode, new NodeConnection(topNeighbour));
            graph.AddConnection(currentNode, new NodeConnection(bottomNeighbour));
            graph.AddConnection(currentNode, new NodeConnection(rightNeighbour));
            graph.AddConnection(currentNode, new NodeConnection(leftNeighbour));

        }
        // foreach (var item in graph.graphContainer)
        // {
        //     var values = graph.graphContainer.GetValuesForKey(item.Key);    
        //     // var curNode = graph.nodes[item.Key];
        //     foreach (var v in values)
        //     {
        //         // Debug.DrawLine(curNode.pos.ToVector2(), v.node.pos.ToVector2(), Color.red, 100f);
        //     }
        // }
    }
    // void Create()
    // {
    //     //adding point nodes 
    //     foreach (var position in tilemap.cellBounds.allPositionsWithin)
    //     {
    //         var pos = position + new Vector3(0.5f, 0.5f);
    //         // tilemap.GetTile(pos).
    //         bool hasTile = !tilemap.HasTile(position) && tilemap.HasTile(position - new Vector3Int(0,1,0));

    //         bool isLeft = tilemap.HasTile(position + new Vector3Int(-1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
    //         bool isRight = tilemap.HasTile(position + new Vector3Int(1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(1,-1, 0));

    //         if (hasTile && isRight && isLeft)
    //         {
    //             var node = new Node(_nodeIds, position.x, position.y);
    //             _nodeIds++;
    //             node.SetType(PlatformType.OneTile);
    //             graph.m_Nodes.Add(node);
    //         }
    //         else if (hasTile && isLeft)
    //         {
    //             var node = new Node(_nodeIds, position.x, position.y);
    //             _nodeIds++;
    //             node.SetType(PlatformType.LeftCliff);
    //             graph.m_Nodes.Add(node);
    //         }
    //         else if (hasTile && isRight)
    //         {
    //             var node = new Node(_nodeIds, position.x, position.y);
    //             _nodeIds++;
    //             node.SetType(PlatformType.RightCliff);
    //             graph.m_Nodes.Add(node);
    //         }

    //         else if (hasTile && !isRight && !isLeft)
    //         {
    //             var node = new Node(_nodeIds, position.x, position.y);
    //             _nodeIds++;
    //             node.SetType(PlatformType.Regular);
    //             graph.m_Nodes.Add(node);
    //         }
    //     }

    //     // node platform ids setup
    //     int platformId = 0;
    //     foreach (var item in graph.m_Nodes)
    //     {
    //         if (item.PlatformType == PlatformType.LeftCliff) 
    //         {
    //             platformId++;
    //             var posNode = item.PosVec2; 
    //             int i = 0;
    //             bool isRightNodeReached = false;
                
    //             while (!isRightNodeReached)
    //             {
    //                 var node = graph.GetNode(posNode + new Vector2Int(i, 0));
    //                 PlatformIds.Add(node, platformId);                               
    //                 i++;
    //                 if (node.PlatformType == PlatformType.RightCliff)
    //                 {
    //                     isRightNodeReached = true;
    //                 }
    //             }
    //         }   
    //         if (item.PlatformType == PlatformType.OneTile)
    //         {
    //             platformId++;
    //             PlatformIds.Add(item, platformId);
    //         }
    //     }
        
    //     // adding connections 
    //     foreach (var position in tilemap.cellBounds.allPositionsWithin)
    //     {
    //         var pos = position + new Vector3(0.5f, 0.5f);

    //         bool hasTile = !tilemap.HasTile(position) && tilemap.HasTile(position - new Vector3Int(0,1, 0));
    //         bool isLeft = tilemap.HasTile(position + new Vector3Int(-1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
    //         bool isRight = tilemap.HasTile(position + new Vector3Int(1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(1,-1, 0));

    //         //connection in between nodes
    //         if (hasTile && !isRight && !isLeft)
    //         {
    //             var node = graph.GetNode((Vector2Int)position);

    //             var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y));
    //             var leftNode = graph.GetNode(new Vector2Int(position.x - 1, position.y));

    //             if (!node.hasConnection(rightNode))
    //                 node.m_Connections.Add(new NodeConnection(rightNode));

    //             if (!node.hasConnection(leftNode))
    //                 node.m_Connections.Add(new NodeConnection(leftNode));

    //             if (!rightNode.hasConnection(node))
    //                 rightNode.m_Connections.Add(new NodeConnection(node));

    //             if (!leftNode.hasConnection(node))
    //                 leftNode.m_Connections.Add(new NodeConnection(node));
    //         }

    //         //one step connection from left to right clif of platform
    //         if (hasTile && isLeft && !isRight)
    //         {
    //             var node = graph.GetNode((Vector2Int)position);
    //             var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y ));
    //             // Debug.Log("it works");

    //             if (rightNode.PlatformType == PlatformType.RightCliff)
    //             {
    //                 if (!node.hasConnection(rightNode))
    //                     node.m_Connections.Add(new NodeConnection(rightNode));

    //                 if(!rightNode.hasConnection(node))
    //                     rightNode.m_Connections.Add(new NodeConnection(node));
    //             }
    //         }
    //         //fall links 
    //         //left fall links
    //         if (hasTile && isLeft)
    //         {
    //             //if nones blocks from left
    //             if (!tilemap.HasTile(position + new Vector3Int(-1, 0, 0)))
    //             {
    //                 bool ground = tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
    //                 int i = -1; 
    //                 int howMuch = 0;
    //                 while (!ground)
    //                 {
    //                     i-=1; 
    //                     ground = tilemap.HasTile(position + new Vector3Int(-1, i, 0));
    //                     howMuch = i;
    //                 }
    //                 var leftNode = graph.GetNode((Vector2Int)position);
    //                 var downNode = graph.GetNode(new Vector2Int(position.x - 1, position.y + howMuch + 1));
                    
    //                 leftNode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
    //             }
    //         }
    //         //right fall links
    //         if (hasTile && isRight)
    //         {
    //             //if nones blocks from left
    //             if (!tilemap.HasTile(position + new Vector3Int(1, 0, 0)))
    //             {
    //                 bool ground = tilemap.HasTile(position + new Vector3Int(1,-1, 0));
    //                 int i = -1; 
    //                 int howMuch =0;
    //                 while (!ground)
    //                 {
    //                     i-=1; 
    //                     ground = tilemap.HasTile(position + new Vector3Int(1, i, 0));
    //                     howMuch = i;
    //                 }
    //                 var rightnode = graph.GetNode((Vector2Int)position);
    //                 var downNode = graph.GetNode(new Vector2Int(position.x + 1, position.y + howMuch + 1));
                    
    //                 rightnode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
    //             }
    //         }
    //         foreach (var item in graph.m_Nodes)
    //         {
    //             foreach (var con in item.m_Connections)
    //             {
    //                 Color conColor = con.conectionsType == ConnectionsTypes.Walk ? Color.cyan : Color.magenta;
    //                 Debug.DrawLine((Vector2)item.PosVec2, (Vector2)con.node.PosVec2, conColor, 1000f);
    //             }
    //         }

    //         //jump links
    //         // Vector2 landingPoint;
    //         // if (hasTile)
    //         // {
    //         //     foreach (var item in _entitesConfigs.aiJumpsVariant)
    //         //     {
    //         //         Vector3 landingPoint = Vector2.zero;
    //         //         bool goodJump = physSim.SimulateJumpViewPhysics( physSim.Humanoid,
    //         //             pos,
    //         //             item.jumpSpeed,
    //         //             item.jumpHeight,
    //         //             ref landingPoint);

    //         //         if (goodJump)
    //         //         {
    //         //             var startJumpNode = pointGraph.GetNearest(pos); 
    //         //             var landingJumpNode = pointGraph.GetNearest(landingPoint); 
    //         //             // Debug.Log(nodeIds[startJumpNode.node]);
    //         //             if (nodeIds[startJumpNode.node] != nodeIds[landingJumpNode.node])
    //         //             {

    //         //                 var cost = (uint)(landingJumpNode.node.position - startJumpNode.node.position).costMagnitude;
    //         //                 startJumpNode.node.AddConnection(landingJumpNode.node, cost);
    //         //                 conecterDic[startJumpNode.node].Add(new NodeConnection(landingJumpNode.node, ConnectionsTypes.Jump, item));

    //         //             }
    //         //             goodJump = false;
    //         //         }    
                    
                    
    //         //     }

    //         // }

    //     }
    // }
}
