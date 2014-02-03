using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System;

public class PathFind {

    #region Structs
    public struct Node
    {
        public int F;
        public int G;
        public int H;
        public int x;
        public int y;
        public int Px;
        public int Py;
    }
    #endregion

    #region Variables
    private BackgroundWorker worker;
    public GameController gameController;

    private sbyte[,] directions = new sbyte[4, 2] { { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 0 } };
    
    private GhostAI caller;
    
    public List<Node> openNodes = new List<Node>();
    public List<Node> closedNodes = new List<Node>();

    public Node lowestNode = new Node();
    public Node parentNode = new Node();

    private bool bFoundInList;
    #endregion

    // The arguments passed to this function are 
    // Vector2 startPoint, Vector2 endPoint, GhostAI caller, byte grid.
    public void DoWork(object sender, DoWorkEventArgs e)
    {
        worker = sender as BackgroundWorker;
        List<object> argumentList = e.Argument as List<object>;
        caller = (GhostAI)argumentList[2];

        findPath((Vector2)argumentList[0], (Vector2)argumentList[1], (byte[,])argumentList[3], (List<GhostAI.Directions>)argumentList[4], (bool)argumentList[5]);
    }

    public void findPath(Vector2 startPoint, Vector2 endPoint, byte[,] grid, List<GhostAI.Directions> bannedDirections, bool IsInsideHouse)
    {
        int heuristic = 1;
        bool IsFirstParent = true;
        bool bPathFound = false;
        lowestNode = new Node();
        parentNode = new Node();
        bFoundInList = false;
        openNodes.Clear();
        closedNodes.Clear();

        // Set the parent and starting point from arguments.
        parentNode.G = 0;
        parentNode.x = (int)startPoint.x;
        parentNode.y = (int)Math.Abs(startPoint.y);
        parentNode.Px = parentNode.x;
        parentNode.Py = parentNode.y;
        parentNode.H = heuristic * (Math.Abs(parentNode.x - (int)endPoint.x) + Math.Abs(parentNode.y - (int)endPoint.y));
        parentNode.H = GetTieBreakerHeuristic(parentNode, parentNode.H, startPoint, endPoint);
        parentNode.F = parentNode.G + parentNode.H;

        openNodes.Add(parentNode);

        while ((openNodes.Count > 0) && !bPathFound)
        {
            if (worker.CancellationPending)
            {
                break;
            }

            openNodes.Remove(parentNode);
            closedNodes.Add(parentNode);

            if ((parentNode.x == endPoint.x) && (parentNode.y == endPoint.y) && !IsFirstParent)
            {
                bPathFound = true;
                break;
            }

            if (IsFirstParent)
            {
                IsFirstParent = false;
            }

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                Node newNode = new Node();
                bool IsInvalidDirection = false;

                bFoundInList = false;

                newNode.x = parentNode.x + directions[i, 0];
                newNode.y = parentNode.y + directions[i, 1];
                newNode.Px = parentNode.x;
                newNode.Py = parentNode.y;
                newNode.G = parentNode.G + grid[newNode.x, newNode.y];
                newNode.H = heuristic * (Math.Abs(newNode.x - (int)endPoint.x) + Math.Abs(newNode.y - (int)endPoint.y));
                newNode.H = GetTieBreakerHeuristic(parentNode, newNode.H, startPoint, endPoint);
                newNode.F = newNode.G + newNode.H;

                // Check if this is a child from the starting node 
                // and see if it is not an allowed direction.
                if ((parentNode.x == (int)startPoint.x) && (parentNode.y == (int)startPoint.y * -1))
                {
                    foreach (GhostAI.Directions bannedDirection in bannedDirections)
                    {
                        switch (bannedDirection)
                        {
                            case(GhostAI.Directions.Up):
                                if (directions[i, 0] == 0 && directions[i, 1] == -1)
                                {
                                    IsInvalidDirection = true;
                                }
                                break;
                            case (GhostAI.Directions.Left):
                                if (directions[i, 0] == -1 && directions[i, 1] == 0)
                                {
                                    IsInvalidDirection = true;
                                }
                                break;
                            case(GhostAI.Directions.Down):
                                if (directions[i, 0] == 0 && directions[i, 1] == 1)
                                {
                                    IsInvalidDirection = true;
                                }
                                break;
                            case(GhostAI.Directions.Right):
                                if (directions[i, 0] == 1 && directions[i, 1] == 0)
                                {
                                    IsInvalidDirection = true;
                                }
                                break;
                        }
                        
                        if (IsInvalidDirection)
                        {
                            break;
                        }
                    }
                    /*for (int j = 0; j < bannedDirections.Count; j++)
                    {
                        if (directions[i, 0] == bannedDirections[j, 0] && directions[i, 1] == bannedDirections[j, 1])
                        {
                            IsInvalidDirection = true;
                            break;
                        }
                    }*/
                }

                if (IsInvalidDirection)
                {
                    continue;
                }

                // Check if the node is outside the boundaries of the grid.
                if ((newNode.x < 0) || (newNode.y < 0) || (newNode.x >= grid.GetUpperBound(0)) || (newNode.y >= grid.GetUpperBound(1)))
                    continue;

                // If the node is unwalkable.
                if (grid[newNode.x, newNode.y] == 0)
                {
                    continue;
                }

                // If the node is a Ghost House door and
                // the ghost is not inside the house.
                if (grid[newNode.x, newNode.y] == 2 && !IsInsideHouse)
                {
                    continue;
                }

                // Check if this node was found in openNodes list.
                for (int j = 0; j < openNodes.Count; j++)
                {
                    if ((newNode.x == openNodes[j].x) && (newNode.y == openNodes[j].y))
                    {
                        if (openNodes[j].G >= newNode.G)
                        {
                            openNodes[j] = newNode;
                        }
                        bFoundInList = true;
                        break;
                    }
                }

                // Check if this node was found in closedNodes list.
                if (!bFoundInList)
                {
                    for (int j = 0; j < closedNodes.Count; j++)
                    {
                        if ((newNode.x == closedNodes[j].x) && (newNode.y == closedNodes[j].y))
                        {
                            bFoundInList = true;
                            break;
                        }
                    }
                }

                if (!bFoundInList)
                {
                    openNodes.Add(newNode);
                }
            }

            for (int i = 0; i < openNodes.Count; i++)
            {
                if ((openNodes[i].F <= lowestNode.F) || (i == 0))
                {
                    lowestNode = openNodes[i];
                }
            }

            parentNode = lowestNode;
        }

        // If the Path was found, start from the end node
        // and start looking to the parents until we find the starting node.
        if (bPathFound)
        {
            Node fNode = closedNodes[closedNodes.Count - 1];
            for (int i = closedNodes.Count - 1; i >= 0; i--)
            {
                if ((fNode.Px == closedNodes[i].x) && (fNode.Py == closedNodes[i].y) || (i == closedNodes.Count - 1))
                {
                    fNode = closedNodes[i];
                }
                else
                {
                    closedNodes.RemoveAt(i);
                }
            }
        }
        else
        {
            // If the path was not found, look for the closest node
            // and start the path from there.
            for (int i = 0; i < closedNodes.Count; i++)
            {
                if ((closedNodes[i].H <= lowestNode.H) 
                    && (closedNodes[i].x != (int)startPoint.x) 
                    && (closedNodes[i].y != (int)startPoint.y))
                {
                    lowestNode = closedNodes[i];
                }
            }

            Node fNode = lowestNode;
            for (int i = closedNodes.Count - 1; i >= 0; i--)
            {
                if ((fNode.Px == closedNodes[i].x) && (fNode.Py == closedNodes[i].y))
                {
                    fNode = closedNodes[i];
                }
                else
                {
                    closedNodes.RemoveAt(i);
                }
            }
            closedNodes.Add(lowestNode);
            closedNodes.RemoveAt(0);
        }
    }

    public int GetTieBreakerHeuristic(Node parentNode, int nodeHeuristic, Vector2 startPoint, Vector2 endPoint)
    {
        int dx1, dx2, dy1, dy2, cross;

        dx1 = parentNode.x - (int)endPoint.x;
        dy1 = parentNode.y - (int)endPoint.y;
        dx2 = (int)startPoint.x - (int)endPoint.x;
        dy2 = (int)startPoint.y - (int)endPoint.y;
        cross = Math.Abs(dx1 * dy2 - dx2 * dy1); 

        return (int)(nodeHeuristic + cross * 0.01);
    }

    public void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        caller.PathFinderCompleted(closedNodes);
    }
}