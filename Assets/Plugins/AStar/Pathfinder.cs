using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Profiling;
using Client;

namespace Client
{
    public static class Pathfinder
    {
        // private static GridController gridController;

        private static Graph graph;
        private static FastPriorityQueue<Node> openListPriorityQueue;
        private static Dictionary<int, Node> closeDictionary;

        private static List<Node> finalPath = new List<Node>();

        public static void Initialize( Graph targetGraph )
        {
            graph = targetGraph;
            openListPriorityQueue = new FastPriorityQueue<Node>( graph.m_Nodes.Count );
            finalPath = new List<Node>( Mathf.RoundToInt( graph.m_Nodes.Count * 0.1f ) );
            closeDictionary = new Dictionary<int, Node>( Mathf.RoundToInt( graph.m_Nodes.Count  * 0.1f ) );
        }

        public static List<Node> GetPath( Vector2Int from, Vector2Int to )
        {
            finalPath.Clear();

            Node initialNode = graph.GetNearestNode(from);
            Node destinationTile = graph.GetNearestNode(to);

            openListPriorityQueue.Enqueue( initialNode, 0 );

            Node currentNode = null;
            while ( openListPriorityQueue.Count > 0 )
            {
                currentNode = openListPriorityQueue.Dequeue();

                closeDictionary.Add( currentNode.Index, currentNode );  
                
                if ( currentNode == destinationTile )
                    break;


                for ( int i = currentNode.m_Connections.Count - 1; i >= 0; --i )
                {
                    Node neighbourPathNode = currentNode.m_Connections[i].node;
                    if ( neighbourPathNode == null )
                        continue;

                    if (closeDictionary.ContainsKey( neighbourPathNode.Index ))
                        continue;

                    bool isAtOpenList = openListPriorityQueue.Contains( neighbourPathNode );
                    
                    float movementCostToNeighbour = currentNode.GCost + GetDistance( currentNode, neighbourPathNode );
                    if ( movementCostToNeighbour < neighbourPathNode.GCost || !isAtOpenList )
                    {
                        neighbourPathNode.SetGCost( movementCostToNeighbour );
                        neighbourPathNode.SetHCost( GetDistance( neighbourPathNode, destinationTile ) );
                        neighbourPathNode.SetParent( currentNode );

                        if ( !isAtOpenList )
                        {
                            openListPriorityQueue.Enqueue( neighbourPathNode,
                                                           neighbourPathNode.FCost );
                        }
                        else
                        {
                            openListPriorityQueue.UpdatePriority( neighbourPathNode, neighbourPathNode.FCost );
                        }
                    }
                }
            }

            finalPath.Clear();
            while ( currentNode.Parent != null && !Equals( currentNode, initialNode ) )
            {
                finalPath.Add( currentNode );
                currentNode = currentNode.Parent;

            }
            finalPath.Add( initialNode );

            openListPriorityQueue.Clear();
            closeDictionary.Clear();
            return finalPath;
        }


        private static float GetDistance( Node targetFromTile, Node targetToTile )
        {
            int fromPositionX = targetFromTile.PositionX;
            int toPositionX = targetToTile.PositionX;
            int fromPositionY = targetFromTile.PositionY;
            int toPositionY = targetToTile.PositionY;
            return (fromPositionX - toPositionX) *
                   (fromPositionX - toPositionX) +
                   (fromPositionY - toPositionY) *
                   (fromPositionY - toPositionY);
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
            return m_Nodes.Where(x => x.PosVec2 == nodePosition).First();
        }
        public Node GetNearestNode(Vector2Int nodePosition)
        {
            return m_Nodes.Select( n => new {n, distance = Vector2.Distance(nodePosition, n.PosVec2)} )
                .OrderBy(p => p.distance)
                .First().n;
        }
    }
}
