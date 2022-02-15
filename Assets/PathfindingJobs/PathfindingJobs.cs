/*
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class PathfindingJobs : MonoBehaviour
{
    public Tilemap tilemap; //tilemap whitch we based to create graph
    Graph graph = new Graph();   
    Dictionary<Node, int> PlatformIds= new Dictionary<Node, int>(); 
    int _nodeIds = 0;

    void Start()
    {
        CreateGraph();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            var startNode = graph.GetNearestNode(new Vector2Int(-14,-9));
            var endNode =  graph.GetNearestNode(new Vector2Int(-7,-9));
            graph.SetHCost(startNode.position, endNode.position);

            NativeArray<int> graphNodes = new NativeArray<int>(10, Allocator.TempJob);
            NativeArray<int> result = new NativeArray<int> (20,Allocator.TempJob);
            NativeHashMap<int, NativeList<int>> connections = new NativeHashMap<int, NativeList<int>>();            
            
            FindPath findPathJob = new FindPath();
            // findPathJob.connectedNodes = connectedNode;
            // findPathJob.graph = graphNodes;
            // findPathJob.startPath = startNode;
            // findPathJob.endPath = endNode;
            // findPathJob.result = result;
            // findPathJob.frontier = frontier;
            // findPathJob.visited = visited;
            
            JobHandle handle = findPathJob.Schedule();
            handle.Complete();

            if (handle.IsCompleted)
            {
                var path = result.ToArray();
                for (int i = 0; i < path.Count() - 1; i++)
                {
                    // Debug.DrawRay((Vector2)path[i].GetVector(), Vector3.up *2, Color.red, 100f);
                }
                result.Dispose();
                
                // graphNodes.Dispose();
            }
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
    void CreateGraph()
    {
        
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            var pos = position + new Vector3(0.5f, 0.5f);
            // tilemap.GetTile(pos).
            bool hasTile = !tilemap.HasTile(position) && tilemap.HasTile(position - new Vector3Int(0, 1, 0));

            bool isLeft = tilemap.HasTile(position + new Vector3Int(-1, 0, 0)) || !tilemap.HasTile(position + new Vector3Int(-1,-1, 0));
            bool isRight = tilemap.HasTile(position + new Vector3Int(1, 0,0)) || !tilemap.HasTile(position + new Vector3Int(1,-1, 0));

            if (hasTile && isRight && isLeft)
            {
                var node = new Node(_nodeIds, (Vector2Int)position);
                _nodeIds++;
                node.SetType(PlatformType.OneTile);
                graph.m_Nodes.Add(node);
            }
            else if (hasTile && isLeft)
            {
                var node = new Node(_nodeIds, (Vector2Int)position);
                _nodeIds++;
                node.SetType(PlatformType.LeftCliff);
                graph.m_Nodes.Add(node);
            }
            else if (hasTile && isRight)
            {
                var node = new Node(_nodeIds, (Vector2Int)position);
                _nodeIds++;
                node.SetType(PlatformType.RightCliff);
                graph.m_Nodes.Add(node);
            }

            else if (hasTile && !isRight && !isLeft)
            {
                var node = new Node(_nodeIds, (Vector2Int)position);
                _nodeIds++;
                node.SetType(PlatformType.Regular);
                graph.m_Nodes.Add(node);
            }
        }
        foreach (var item in graph.m_Nodes)
        {
            Debug.DrawRay((Vector2)item.position, Vector3.up, Color.red, 100f);
        }

        // node platform ids setup
        int platformId = 0;
        foreach (var item in graph.m_Nodes)
        {
            if (item.PlatformType == PlatformType.LeftCliff) 
            {
                platformId++;
                var posNode = item.position; 
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
                var node = graph.GetNearestNode((Vector2Int)position);

                var rightNode = graph.GetNearestNode(new Vector2Int(position.x + 1, position.y));
                var leftNode = graph.GetNearestNode(new Vector2Int(position.x - 1, position.y));

                //connection type is missied                
                node.connections.Add(rightNode);
                node.connections.Add(leftNode);
                leftNode.connections.Add(node);
                rightNode.connections.Add(node);
            }

            //one step connection from left to right clif of platform
            if (hasTile && isLeft && !isRight)
            {
                var node = graph.GetNode((Vector2Int)position);
                var rightNode = graph.GetNode(new Vector2Int(position.x + 1, position.y ));
                Debug.Log("it works");

                if (rightNode.PlatformType == PlatformType.RightCliff)
                {
                    node.connections.Add(rightNode);
                    rightNode.connections.Add(node);
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
                    
                    leftNode.connections.Add(downNode);
                    // leftNode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
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
                    
                    rightnode.connections.Add(downNode);
                    // rightnode.m_Connections.Add(new NodeConnection(downNode, ConnectionsTypes.Fall));
                }
            }
            foreach (var item in graph.m_Nodes)
            {
                
                // Debug.DrawRay((Vector2)item.GetVector(), Vector3.up, Color.blue, 1000f);

                // foreach (var con in item.m_Connections)
                // {
                //     Color conColor = con.conectionsType == ConnectionsTypes.Walk ? Color.cyan : Color.magenta;
                //     Debug.DrawLine((Vector2)item.GetVector(), (Vector2)con.node.PosVec2, conColor, 1000f);
                // }
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

public struct FindPath : IJob
{
    public NativeArray<Node> graph;
    public Node startPath;
    public Node endPath;
    public NativeArray<Node> result;//dont forget to dispose
    // public NativeList<Node> connectedNodes ;


    // public NativeQueue<Node> frontier;
    // public NativeList<Node> visited;
    public void Execute()
    {
        Debug.Log(" i am started");
        NativeQueue<Node> frontier = new NativeQueue<Node>(Allocator.Temp);
        NativeList<Node> visited = new NativeList<Node>(Allocator.Temp);

        NativeList<Node> connectedNodes = new NativeList<Node>(Allocator.Temp);
        
        visited.Add(startPath);


        while (frontier.Count > 0)
        {
            var currentnode = frontier.Dequeue();
            var curr = currentnode;

            //go out if we reached our goal
            if(curr.index == endPath.index )
                break;
            
            //add connection or neighbours
            connectedNodes.Clear();
            for (int i = 0; i < 2; i++)
            {
                for (int u = 0; u < 4; u++)
                {
                    connectedNodes.Add(GetNodeByIndex(currentnode.connectionsNode[i][u], graph));
                }
            }

            for (int i = 0; i < connectedNodes.Length - 1; i++)
            {
                var next = connectedNodes[i];
                //not valid node
                if (next.index == 0)
                    continue;

                int tentativeGCost = curr.gCost + Equations.CalculateDistanceCost(curr.position, next.position);
                if (tentativeGCost < next.gCost)
                {
                    next.nodeParent = curr.index;
                    next.gCost = tentativeGCost;
                    next.CalculateFCost();
                    int nextIndexGraph = GetIndexGraphByNodeIndex(next.index,graph);
                    graph[nextIndexGraph] = next;

                    if (!IsContainsValueInList(next, visited).x)
                    {
                        frontier.Enqueue(next);
                    }
                }
            }
            

        }
        var lastNodeIndex = GetIndexGraphByNodeIndex(endPath.index, graph);
        Node lastNode = graph[lastNodeIndex];

        if (lastNode.nodeParent != -1)
        {
            result = CalculatePath(graph, lastNode);
            // NativeArray<Node> path = CalculatePath(graph, lastNode);
            // result = path;
            // path.Dispose();
        }

        // graph.Dispose();
        frontier.Dispose();
        visited.Dispose();
        connectedNodes.Dispose();

    }
    private NativeList<Node> CalculatePath(NativeArray<Node> graph, Node endNode) 
    {
        if (endNode.nodeParent == -1) {
            // Couldn't find a path!
            return new NativeList<Node>(Allocator.Temp);
        } else {
            // Found a path
            NativeList<Node> path = new NativeList<Node>(Allocator.Temp);
            path.Add(endNode);

            Node currentNode = endNode;
            while (currentNode.nodeParent != -1) {
                Node cameFromNode = GetNodeByIndex(currentNode.index, this.graph);
                path.Add(cameFromNode);
                currentNode = cameFromNode;
            }

            return path;
        }
    }


    Node GetNodeByIndex(int index, NativeArray<Node> graph)
    {
        for (int i = 0; i < graph.Length - 1; i++)
        {
            if (graph[i].index == index)
            {
                return graph[i];
            }
        }
        Debug.LogError("node not founded by index");
        return graph[0];
    }
    
    int GetIndexGraphByNodeIndex(int nodeIndex, NativeArray<Node> graph)
    {

        for (int i = 0; i < graph.Length - 1; i++)
        {
            if (graph[i].index == nodeIndex)
            {
                return i;
            }
        }
        Debug.LogError("node not founded by index");
        return 0;
    }
    bool2 IsContainsValueInList (Node node, NativeList<Node> list)
    {
        bool2 retValue;
        
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].index == node.index)
            {
                return retValue.x = true;
            }
        }
        return retValue.x = false;
    }

}

public class Node 
{
    public int index;
    public Vector2Int position;
    public PlatformType PlatformType;
    public List<NodeConnection> connections = new List<NodeConnection>();
    
    public Node(int index, Vector2Int pos)
    {
        position = pos;
        this.index = index;
        PlatformType = PlatformType.Regular;
    }
    public void SetType(PlatformType targetType )
    {
        if(PlatformType == targetType)
            return;
        PlatformType = targetType;

    }
}
public class NodeConnection 
{
    Node endNode;
    public ConnectionsTypes conectionsType;
    public AiJump jump; 
    public NodeConnection(Node node, ConnectionsTypes type = ConnectionsTypes.Walk){
        this.endNode = node;
        this.conectionsType = type;

        if (type == ConnectionsTypes.Jump)
        {
            Debug.LogError("Connection type is jump but you dint set it");
        }
    }
    public NodeConnection(Node node, ConnectionsTypes type, AiJump jump){
        this.endNode = node;
        this.conectionsType = type;
        this.jump = jump;

        if (type != ConnectionsTypes.Jump) 
        {
            Debug.LogError("Type of connection isnt jump to set jump");
        }
    }
}
public enum ConnectionsTypes {
    Walk,
    Jump,
    Fall
}
public struct AiJump
{
    public float jumpSpeed;
    public float jumpHeight;
    public AiJump(float jumpSpeed, float jumpHeight)
    {
        this.jumpSpeed = jumpSpeed;
        this.jumpHeight = jumpHeight;
    }
}
public class Graph
{
    /// <summary>
    /// The nodes.
    /// </summary>
    public List<Node> m_Nodes = new List<Node> ();

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    /// <value>The nodes.</value>
    public virtual List<Node> nodes
    {
        get
        {
            return m_Nodes;
        }
    }
    
    public Node GetNode(Vector2Int nodePosition)
    {
        return m_Nodes.Where(x => x.position == nodePosition).First();
    }
    public Node GetNearestNode(Vector2Int nodePosition)
    {
        return m_Nodes.Select( n => new {n, distance = Vector2.Distance(nodePosition, n.position ) } )
            .OrderBy(p => p.distance)
            .First().n;
    }
}
public static class Equations
{
    public static int CalculateDistanceCost(int2 aPosition, int2 bPosition) 
    {
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);
        return math.min(xDistance, yDistance) + remaining;
    }
    public static Vector2 int2ToVector2(this int2 value)
    {
        return new Vector2(value.x, value.y);
    }
}
public enum PlatformType 
{
    OneTile,
    RightCliff,
    LeftCliff,
    Regular 
}
*/