using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockEngine;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.WorldGenerator.Agents
{
    public class BSPNode
    {
        // node _input parameters
        public IntVector2 topLeft;
        public IntVector2 dimensions;
        private AgentManipulator _input;
        private Random _random;

        public BSPNode leftChild;
        public BSPNode rightChild;

        public BSPRoom room;            // optional room of this node
        public List<BSPRoom> hallways;  // optional hallways of this node 

        public static int MIN_ROOM_SIZE = 7;

        public BSPNode(AgentManipulator input, IntVector2 topLeft, IntVector2 dimensions, Random random)
        {
            this.dimensions = dimensions;
            this.topLeft = topLeft;
            _input = input;
            _random = random;
        }

        public bool Split()
        {
            if (leftChild != null || rightChild != null)
                return false;
 
            bool splitH = _random.NextDouble() > 0.5; // direction of split (horizontally / vertically)
            if (dimensions.x > dimensions.y && (float)dimensions.y / dimensions.x >= 0.05)
                splitH = false;
            else if (dimensions.y > dimensions.x && (float)dimensions.x / dimensions.y >= 0.05)
                splitH = true;

            int max = (splitH ? dimensions.y : dimensions.x) - MIN_ROOM_SIZE;
            if (max <= MIN_ROOM_SIZE)
                return false;

            int split = _random.Next(MIN_ROOM_SIZE, max);
 
            // create left and right children based on the direction of the split
            if (splitH)
            {
                leftChild = new BSPNode(_input, topLeft, new IntVector2(dimensions.x, split), _random);
                rightChild = new BSPNode(_input, new IntVector2(topLeft.x, topLeft.y + split), new IntVector2(dimensions.x, dimensions.y - split), _random);
            }
            else
            {
                leftChild = new BSPNode(_input, topLeft, new IntVector2(split, dimensions.y), _random);
                rightChild = new BSPNode(_input, new IntVector2(topLeft.x + split, topLeft.y), new IntVector2(dimensions.x - split, dimensions.y), _random);
            }
            return true;
        }

        public void CreateRooms()
        {
            // generates all the rooms and hallways for this node and all of its children.

            // create child rooms and hallways
            if (leftChild != null || rightChild != null)
            {
                if (leftChild != null)
                    leftChild.CreateRooms();
                if (rightChild != null)
                    rightChild.CreateRooms();

                if (leftChild != null && rightChild != null)
                    hallways = CreateHallway(leftChild.GetRoom(), rightChild.GetRoom());
            }
            else // create a room
            {
                IntVector2 roomSize = new IntVector2(_random.Next(MIN_ROOM_SIZE, dimensions.x), _random.Next(MIN_ROOM_SIZE, dimensions.y));
                IntVector2 roomPos = new IntVector2(_random.Next(0, dimensions.x - roomSize.x), _random.Next(0, dimensions.y - roomSize.y));
                room = new BSPRoom(new IntVector2(topLeft.x + roomPos.x, topLeft.y + roomPos.y), roomSize);
            }
        }

        public List<BSPNode> GetTreeAsList()
        {
            List<BSPNode> ret = new List<BSPNode> {this};
            if(leftChild != null)
                ret.AddRange(leftChild.GetTreeAsList());
            if(rightChild != null)
                ret.AddRange(rightChild.GetTreeAsList());
            return ret;
        }

        public BSPRoom GetRoom()
        {
            // returns the first room it finds in the child nodes

            if (room != null)
                return room;
            else
            {
                BSPRoom lRoom = null;
                BSPRoom rRoom = null;

                if (leftChild != null)
                    lRoom = leftChild.GetRoom();
                if (rightChild != null)
                    rRoom = rightChild.GetRoom();

                if (lRoom == null && rRoom == null)
                    return null;
                else if (rRoom == null)
                    return lRoom;
                else if (lRoom == null)
                    return rRoom;
                else if (_random.NextDouble() > .5)
                    return lRoom;
                else
                    return rRoom;
            }
        }

        public List<BSPRoom> CreateHallway(BSPRoom l, BSPRoom r)
        {
            // checks which point is where and then either draws a straight line, or a pair of lines to make a right-angle to connect them
 
            List<BSPRoom> halls = new List<BSPRoom>();
 
            IntVector2 point1 = new IntVector2(_random.Next(l.left + 1, l.right - 2), _random.Next(l.top + 1, l.bottom - 2));
            IntVector2 point2 = new IntVector2(_random.Next(r.left + 1, r.right - 2), _random.Next(r.top + 1, r.bottom - 2));
 
            int w = point2.x - point1.x;
            int h = point2.y - point1.y;
 
            if (w < 0) {
                w = Mathf.Abs(w);
                if (h < 0) {
                    if (_random.NextDouble() * 0.5 >= 0.5) {
                        halls.Add(new BSPRoom(new IntVector2(point2.x, point1.y), new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(point2, new IntVector2(1, -h)));
                    } else {
                        halls.Add(new BSPRoom(point2, new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(new IntVector2(point1.x, point2.y), new IntVector2(1, -h)));
                    }
                } else if (h > 0) {
                    if (_random.NextDouble() * 0.5 >= 0.5) {
                        halls.Add(new BSPRoom(new IntVector2(point2.x, point1.y), new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(new IntVector2(point2.x, point1.y), new IntVector2(1, h)));
                    } else {
                        halls.Add(new BSPRoom(point2, new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(point1, new IntVector2(1, h)));
                    }
                } else {
                    halls.Add(new BSPRoom(point2, new IntVector2(w, 1)));
                }
            }
            else if (w > 0) {
                if (h < 0) {
                    if (_random.NextDouble() * 0.5 >= 0.5) {
                        halls.Add(new BSPRoom(new IntVector2(point1.x, point2.y), new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(new IntVector2(point1.x, point2.y), new IntVector2(1, -h)));
                    } else {
                        halls.Add(new BSPRoom(point1, new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(point2, new IntVector2(1, -h)));
                    }
                } else if (h > 0) {
                    if (_random.NextDouble() * 0.5 >= 0.5) {
                        halls.Add(new BSPRoom(point1, new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(new IntVector2(point2.x, point1.y), new IntVector2(1, h)));
                    } else {
                        halls.Add(new BSPRoom(new IntVector2(point1.x, point2.y), new IntVector2(w, 1)));
                        halls.Add(new BSPRoom(point1, new IntVector2(1, h)));
                    }
                } else
                    halls.Add(new BSPRoom(point1, new IntVector2(w, 1)));
            } else {
                if (h < 0)
                    halls.Add(new BSPRoom(point2, new IntVector2(1, -h)));
                else if (h > 0)
                    halls.Add(new BSPRoom(point1, new IntVector2(1, h)));
            }

            return halls;
        }

        public void DrawRooms()
        {
            // draws all rooms that are in this node's children

            List<BSPNode> nodes = GetTreeAsList();
            foreach (BSPNode node in nodes)
            {
                if(node.room != null)
                    node.room.Draw(_input);
                if (node.hallways != null)
                {
                    foreach (BSPRoom r in node.hallways)
                    {
                        r.Draw(_input);
                    }
                }
            }
            _input.Finish();
        }
        
        public static void CreateNodes(AgentManipulator input, IntVector2 topLeft, IntVector2 dimensions, int minRoomSize, int maxRoomSize, Random numGen)
        {
            List<BSPNode> nodes = new List<BSPNode>();

            BSPNode root = new BSPNode(input, topLeft, dimensions, numGen);
            nodes.Add(root);

            MIN_ROOM_SIZE = minRoomSize;
 
            bool did_split = true;
            // run over all nodes as long as one can be split
            while (did_split)
            {
                did_split = false;
                for(int i=0; i<nodes.Count; i++)
                {
                    if (nodes[i].leftChild == null && nodes[i].rightChild == null) // if not split...
                    {
                        if (nodes[i].dimensions.x > maxRoomSize || nodes[i].dimensions.y > maxRoomSize || numGen.NextDouble() > 0.25) // ... and too big for single room...
                        {
                            if (nodes[i].Split())
                            {
                                nodes.Add(nodes[i].leftChild);
                                nodes.Add(nodes[i].rightChild);
                                did_split = true;
                            }
                        }
                    }
                }
            }

            root.CreateRooms();
            root.DrawRooms();
        }
    }
}
