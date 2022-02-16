//using Priority_Queue;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Client
{
    public class Node //: FastPriorityQueueNode
    {

        public List<NodeConnection> m_Connections = new List<NodeConnection>();
        public bool hasConnection(Node node)
        {
            foreach (var item in m_Connections)
            {
                if (item.node.index == node.index)
                {
                    return true;
                }

            }
            return false;
        }
        public Vector2Int PosVec2 
        {
            get 
            {
                return new Vector2Int(positionX, positionY);
            }
        }
        public int2 PosInt2
        {
            get {
                return new int2(positionX, positionY);
            }
        } 
        private Node parent;
        public Node Parent
        {
            get { return parent; }
        }
        private int positionX;
        public int PositionX
        {
            get
            {
                return positionX;
            }
        }
        private int positionY;
        public int PositionY
        {
            get
            {
                return positionY;
            }
        }

        private float gCost;
        public float GCost
        {
            get { return gCost; }
        }

        private float hCost;
        public float HCost
        {
            get
            {
                return hCost;
            }
        }

        public float FCost
        {
            get
            {
                return gCost + hCost;
            }
        }
        public void SetParent( Node targetTile )
        {
            parent = targetTile;
        }
        private PlatformType platformType;
        public PlatformType PlatformType
        {
            get
            {
                return platformType;
            }
        }

        private int index;
        public int Index
        {
            get
            {
                return index;
            }
        }

        public Node( int targetTileIndex, int targetPositionX, int targetPOsitionY )
        {
            index = targetTileIndex;
            SetTilePostion( targetPositionX, targetPOsitionY );
        }


        private void SetTilePostion( int targetPositionX, int targetPOsitionY )
        {
            positionX = targetPositionX;
            positionY = targetPOsitionY;
        }

        public void SetGCost( float targetGCost )
        {
            gCost = targetGCost;
        }

        public void SetHCost( float targetHCost )
        {
            hCost = targetHCost;
        }

        public void SetType(PlatformType targetType )
        {
            if(platformType == targetType)
                return;
            
            platformType = targetType;
        }
        /*
        public override bool Equals( object obj )
        {
            Node otherTile = obj as Node;
            if ( otherTile == null )
                return false;

            return Index == otherTile.Index;
        }
        */
    }
    public enum PlatformType 
    {
        OneTile,
        RightCliff,
        LeftCliff,
        Regular 
    }
    public enum ConnectionsTypes {
        Walk,
        Jump,
        Fall
    }
    public class NodeConnection {
        public Node node;
        public ConnectionsTypes conectionsType;
        public AiJump jump; 
        public NodeConnection(Node node, ConnectionsTypes type = ConnectionsTypes.Walk){
            this.node = node;
            this.conectionsType = type;

            if (type == ConnectionsTypes.Jump)
            {
                Debug.LogError("Connection type is jump but you dint set it");
            }
        }
        public NodeConnection(Node node, ConnectionsTypes type, AiJump jump){
            this.node = node;
            this.conectionsType = type;
            this.jump = jump;

            if (type != ConnectionsTypes.Jump) 
            {
                Debug.LogError("Type of connection isnt jump to set jump");
            }
        }
    }
    public partial struct AiJump
    {
        public float jumpSpeed;
        public float jumpHeight;
        public AiJump(float jumpSpeed, float jumpHeight)
        {
            this.jumpSpeed = jumpSpeed;
            this.jumpHeight = jumpHeight;
        }
    }
}

