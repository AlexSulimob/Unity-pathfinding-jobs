using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Client;
public class CreateGraph : MonoBehaviour
{    
    public Tilemap tilemap; //tilemap whitch we based to create graph
    Graph graph = new Graph();   
    Dictionary<Node, int> PlatformIds= new Dictionary<Node, int>(); 
    int _nodeIds = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Create();
        // Pathfinder.Initialize(graph);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // var path = Pathfinder.GetPath(new Vector2Int(21, 6), new Vector2Int(1, -7));
            // var path = Pathfinder.GetPath( new Vector2Int(-1, -1), new Vector2Int(-2, -1));
            // path.Reverse();
            // if (path[path.Count -1].PosVec2 != new Vector2Int(-2, -1))
            // {
            //     Debug.LogWarning("dont find");
            // }
            // // var path = graph.GetShortestPath(graph.m_Nodes[4], graph.GetNode(new Vector2(23.5f, 5.5f)));
            // foreach (var item in path)
            // {
            //     Debug.Log(item.PosVec2);
            //     Debug.DrawRay((Vector2)item.PosVec2, Vector3.up, Color.red, 100f);
            // }
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
                
                node.m_Connections.Add(new NodeConnection(rightNode));
                node.m_Connections.Add(new NodeConnection(leftNode));

                rightNode.m_Connections.Add(new NodeConnection(node));
                leftNode.m_Connections.Add(new NodeConnection(node));
            }

            //one step connection from left to right clif of platform
            if (hasTile && isLeft && !isRight)
            {
                var node = graph.GetNode((Vector2Int)position);
                var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y ));
                Debug.Log("it works");

                if (rightNode.PlatformType == PlatformType.RightCliff)
                {
                    node.m_Connections.Add(new NodeConnection(rightNode));
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
