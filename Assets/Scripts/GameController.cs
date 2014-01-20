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
            new System.ComponentModel.DoWorkEventHandler(pathFind.DoWork);
        pathFinder.RunWorkerCompleted +=
            new System.ComponentModel.RunWorkerCompletedEventHandler(pathFind.WorkerCompleted);
    }

    private void RequestPathFind(Vector2 startPoint, Vector2 endPoint, GhostAI caller, int direction)
    {
        List<object> arguments = new List<object>();
        arguments.Add(startPoint);
        arguments.Add(endPoint);
        arguments.Add(caller);
        arguments.Add(Tiles);
        arguments.Add(new sbyte[2,2]{{1,0}, {0,-1}});
        pathFinder.RunWorkerAsync(arguments);
    }

    public void PathFinderCompleted(List<PathFind.Node> path, GhostAI caller)
    {
        bCompleted = true;
        print("COMPLETED PATH FINDING " + path.Count + " NODES");
    }
}