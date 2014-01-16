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

    public GameController gameController;
    private float p = 1 / 1000;
    private int heuristic = 1;
    public byte[,] grid;
    private sbyte[,] directions = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
    private Vector2 startPoint, endPoint;
    private GhostAI caller;
    public Node lowestNode, parentNode;
    public List<Node> openNodes = new List<Node>();
    public List<Node> closedNodes = new List<Node>();
    public List<Node> pathList = new List<Node>();
    private bool bPathFound;
    private bool bFoundInList;
    private bool bPathFinished;
    // The arguments passed to this function are 
    // Vector2 startPoint, Vector2 endPoint, GameObject caller, byte grid
    public void findPath(object sender, DoWorkEventArgs e)
    {
        openNodes.Clear();
        closedNodes.Clear();

        BackgroundWorker worker = sender as BackgroundWorker;
        List<object> argumentList = e.Argument as List<object>;
        startPoint = (Vector2)argumentList[0];
        endPoint = (Vector2)argumentList[1];
        caller = (GhostAI)argumentList[2];
        grid = (byte[,])argumentList[3];


        //Set the parent and starting point from arguments
        parentNode.G = 0;
        parentNode.x = (int)startPoint.x;
        parentNode.y = (int)startPoint.y * -1;
        parentNode.Px = (int)startPoint.x;
        parentNode.Py = (int)startPoint.y;
        parentNode.H = heuristic * (Math.Abs(parentNode.x - (int)endPoint.x) + Math.Abs(parentNode.y - (int)endPoint.y));
        parentNode.F = parentNode.G + parentNode.H;

        openNodes.Add(parentNode);

        while (openNodes.Count > 0 || !bPathFound)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                break;
            }
            openNodes.Remove(parentNode);
            closedNodes.Add(parentNode);

            if (parentNode.x == endPoint.x && parentNode.y == endPoint.y)
            {
                bPathFound = true;
                break;
            }

            for (int i = 0; i < 4; i++ )
            {
                Node newNode;

                bFoundInList = false;

                newNode.x = parentNode.x + directions[i, 0];
                newNode.y = parentNode.y + directions[i, 1];
                newNode.Px = parentNode.x;
                newNode.Py = parentNode.y;
                newNode.G = parentNode.G + grid[newNode.x, newNode.y];
                newNode.H = heuristic * (Math.Abs(newNode.x - (int)endPoint.x) + Math.Abs(newNode.y - (int)endPoint.y));

                if (newNode.x < 0 || newNode.y < 0 || newNode.x >= grid.GetUpperBound(0) || newNode.y >= grid.GetUpperBound(1))
                    continue;

                //Tie breaker
                int dx1 = parentNode.x - (int)endPoint.x;
                int dy1 = parentNode.y - (int)endPoint.y;
                int dx2 = (int)startPoint.x - (int)endPoint.x;
                int dy2 = (int)startPoint.y - (int)endPoint.y;
                int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                newNode.H = (int)(newNode.H + cross * 0.001);

                newNode.F = newNode.G + newNode.H;

                //Check if this node was found in openNodes list
                for (int j = 0; j < openNodes.Count; j++)
                {
                    if (newNode.x == openNodes[j].x && newNode.y == openNodes[j].y)
                    {
                        if (openNodes[j].G >= newNode.G)
                        {
                            openNodes[j] = newNode;
                        }
                        bFoundInList = true;
                        break;
                    }
                }

                //Check if this node was found in closedNodes list
                if (!bFoundInList)
                {
                    for (int j = 0; j < closedNodes.Count; j++)
                    {
                        if (newNode.x == closedNodes[j].x && newNode.y == closedNodes[j].y)
                        {
                            bFoundInList = true;
                            break;
                        }
                    }
                }

                //If the node is unwalkable
                if (grid[newNode.x, newNode.y] == 0)
                {
                    continue;
                }

                if (!bFoundInList)
                {
                    openNodes.Add(newNode);
                }
            }

            for (int i = 0; i < openNodes.Count; i++ )
            {
                if (openNodes[i].F < lowestNode.F || i == 0)
                {
                    lowestNode = openNodes[i];
                }
            }

            parentNode = lowestNode;
        }

        Node fNode = closedNodes[closedNodes.Count - 1];
        for (int i = closedNodes.Count - 1; i >= 0; i-- )
        {
            if (fNode.Px == closedNodes[i].x && fNode.Py == closedNodes[i].y || i == closedNodes.Count - 1)
            {
                fNode = closedNodes[i];
            }
            else
                closedNodes.RemoveAt(i);
        }
    }

    public void completed(object sender, RunWorkerCompletedEventArgs e)
    {
        gameController.PathFinderCompleted(closedNodes, caller);
    }
}