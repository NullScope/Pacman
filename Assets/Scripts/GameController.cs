using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

public class GameController : MonoBehaviour
{

    #region Variables
    private int score;
    private PathFind pathFind = new PathFind();
    private BackgroundWorker pathFinder = new BackgroundWorker();
    private byte[,] tiles = new byte[28, 36];
    public Pacman player;
    public GhostAI Blinky, Clyde, Inky, Pinky;
    public int lifes;
    public float PowerPellet;
    private float PPtimeLeft;
    private bool bPowerPellet;
    private bool bCompleted;
    public bool bWorking;
    #endregion

    #region Properties
    public byte[,] Tiles
    {
        get
        {
            return tiles;
        }
       
    }

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
        }
    }
    #endregion

    // Use this for initialization
	void Start () {
        
        InitializePathFinder();
        
	}

    void OnApplicationQuit()
    {
        pathFinder.CancelAsync();
    }

    // Update is called once per frame
    void Update()
    {
        if (Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(Blinky.transform.position, new Vector2(26, 32), Blinky);
            bWorking = true;
        }
        for (int i = 0; i < pathFind.closedNodes.Count; i++)
        {
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x + 1, pathFind.closedNodes[i].y * -1), Color.green, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, -1 * pathFind.closedNodes[i].y - 1), new Vector3(pathFind.closedNodes[i].x + 1, -1 * pathFind.closedNodes[i].y - 1), Color.green, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x, -1 * pathFind.closedNodes[i].y - 1), Color.green, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x + 1, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x + 1, -1 * pathFind.closedNodes[i].y - 1), Color.green, 0.1f, false);

        }

        for (int i = 0; i < pathFind.openNodes.Count; i++)
        {
            Debug.DrawLine(new Vector3(pathFind.openNodes[i].x, pathFind.openNodes[i].y * -1), new Vector3(pathFind.openNodes[i].x + 1, -1 * pathFind.openNodes[i].y - 1), Color.blue, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.openNodes[i].x + 1, pathFind.openNodes[i].y * -1), new Vector3(pathFind.openNodes[i].x, -1 * pathFind.openNodes[i].y - 1), Color.blue, 0.1f, false);
        }

        Debug.DrawLine(new Vector3(pathFind.parentNode.x, pathFind.parentNode.y * -1), new Vector3(pathFind.parentNode.x + 1, pathFind.parentNode.y * -1), Color.red, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.parentNode.x, -1 * pathFind.parentNode.y - 1), new Vector3(pathFind.parentNode.x + 1, -1 * pathFind.parentNode.y - 1), Color.red, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.parentNode.x, pathFind.parentNode.y * -1), new Vector3(pathFind.parentNode.x, -1 * pathFind.parentNode.y - 1), Color.red, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.parentNode.x + 1, pathFind.parentNode.y * -1), new Vector3(pathFind.parentNode.x + 1, -1 * pathFind.parentNode.y - 1), Color.red, 0.1f, false);

        
        //print(player.tileX + " | " + player.tileY);
        updatePowerPellet();
	}

    public void CollisionWithGhost(GhostAI ghost)
    {
        if (bPowerPellet)
        {
            ghost.Death();
        }
        else
        {
            player.Death();
        }
    }

    void updatePowerPellet()
    {
        if (bPowerPellet == true)
        {

            PPtimeLeft -= Time.deltaTime;

            if (PPtimeLeft <= PowerPellet * 0.2)
            {
                setGhostsVulnEnd(true);
            }

            if (PPtimeLeft <= 0)
            {
                endPowerPellet();

            }
        }
    }

    public void startPowerPellet()
    {
        // Set bPowerPellet to true so Update can countdown the time left
        bPowerPellet = true;
        PPtimeLeft = PowerPellet;
        setGhostsVuln(true);
    }

    public void endPowerPellet()
    {
        // Set bPowerPellet to false so Update stops counting the time left
        bPowerPellet = false;
        setGhostsVulnEnd(false);
    }

    // Sets all ghosts vulnerability
    public void setGhostsVuln(bool vuln)
    {
        Blinky.setVulnerability(vuln);
        Clyde.setVulnerability(vuln);
        Inky.setVulnerability(vuln);
        Pinky.setVulnerability(vuln);
    }

    // Sets all ghosts vulnerabilityEnding
    public void setGhostsVulnEnd(bool vulnEnd)
    {
        Blinky.setVulnerabilityEnd(vulnEnd);
        Clyde.setVulnerabilityEnd(vulnEnd);
        Inky.setVulnerabilityEnd(vulnEnd);
        Pinky.setVulnerabilityEnd(vulnEnd);
    }

    public void returnGhostsToDefault()
    {
        Blinky.returnToDefault();
        Clyde.returnToDefault();
        Inky.returnToDefault();
        Pinky.returnToDefault();
    }

    public void addTile(int x, int y, byte cost)
    {
        tiles[x, y] = cost;
    }

    public byte getTile(int x, int y)
    {
        return tiles[x, y];
    }

    // Path Finder
    private void InitializePathFinder()
    {
        pathFind.gameController = this;
        pathFinder.WorkerSupportsCancellation = true;
        // Attach event handlers to the BackgroundWorker object.
        pathFinder.DoWork +=
            new System.ComponentModel.DoWorkEventHandler(pathFind.findPath);
        pathFinder.RunWorkerCompleted +=
            new System.ComponentModel.RunWorkerCompletedEventHandler(pathFind.completed);
    }

    private void RequestPathFind(Vector2 startPoint, Vector2 endPoint, GhostAI caller)
    {
        List<object> arguments = new List<object>();
        arguments.Add(startPoint);
        arguments.Add(endPoint);
        arguments.Add(caller);
        arguments.Add(Tiles);
        pathFinder.RunWorkerAsync(arguments);
    }

    public void PathFinderCompleted(List<PathFind.Node> path, GhostAI caller)
    {
        bCompleted = true;
        print("COMPLETED PATH FINDING " + path.Count + " NODES");
    }

    public void printOpenList(List<PathFind.Node> openList)
    {

    }
}