﻿using UnityEngine;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System;

public class GhostAI : MonoBehaviour {

    #region Enums
    public enum Modes { Scatter, Chase, Frightened, Respawning, Entering, Leaving, Idle }
    public enum Directions { Up, Left, Down, Right }
    #endregion

    #region Variables
    public Color DebugPathColor;
    public bool DebugPath;

    private Animator anim;

    public Modes currentMode;
    protected Modes currentGlobalMode;
    public Modes startMode;

    protected Directions currentDirection;
    protected Directions currentIdleDirection;
    protected Directions nextDirection;
    public Directions startDirection;

    protected Vector2 targetTile;
    protected Vector2 tile;
    public Vector2 scatterTile;
    public Vector2 respawnTile;
    public Vector2 houseTile;

    protected GameController gameController;

    /**DEPRECATED**/
    protected PathFind pathFind = new PathFind();
    protected BackgroundWorker pathFinder = new BackgroundWorker();
    protected List<PathFind.Node> path = new List<PathFind.Node>();

    protected bool bWorking;
    protected bool isMoving;
    private bool isFirstMove;
    public bool isVulnerable;
    public bool isOutside;
    public int dotCount;
    public int dotLimit;
    public float moveSpeed;
    #endregion

    // Use this for initialization
    public void Start()
    {
        isFirstMove = true;
        currentMode = startMode;
        nextDirection = startDirection;
        anim = GetComponent<Animator>();
        
        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }
        InitializePathFinder();
    }

    /**DEPRECATED**/
    private void InitializePathFinder()
    {
        pathFind.gameController = gameController;
        pathFinder.WorkerSupportsCancellation = true;
        // Attach event handlers to the BackgroundWorker object.
        pathFinder.DoWork +=
            new System.ComponentModel.DoWorkEventHandler(pathFind.DoWork);
        pathFinder.RunWorkerCompleted +=
            new System.ComponentModel.RunWorkerCompletedEventHandler(pathFind.WorkerCompleted);
    }

	// Update is called once per frame
	public void Update ()
    {
        updateVulnParameter();
        updateTile();
        updatePosition();
        updateDotCount();
        DebugPathFind();
	}

    public void updatePosition() 
    {
        if (isMoving)
        {
            return;
        }

        if (currentMode != Modes.Idle && currentMode != Modes.Entering && currentMode != Modes.Leaving)
        {
            currentDirection = nextDirection;
            nextDirection = FindNextDirection(transform.position, currentDirection, currentMode);
            StartCoroutine(move(currentMode));
        }
        else
        {
            StartCoroutine(HouseMove(currentMode));
        }
    }

    public void updateDotCount()
    {
        if (gameController.bGlobalCounter)
        {
            return;
        }

        if (dotCount >= dotLimit && !isOutside)
        {
            leaveGhostHouse();
        }
    }

    public void leaveGhostHouse()
    {
        StopAllCoroutines();
        isMoving = false;
        transform.position = houseTile;
        setMode(Modes.Leaving);
        isOutside = true;
    }

    public void enterGhostHouse()
    {
        StopAllCoroutines();
        isMoving = false;
        transform.position = respawnTile;
        setMode(Modes.Entering);
        isOutside = false;
    }

    public IEnumerator move(Modes mode)
    {
        float t = 0;
        Vector2 endPosition = new Vector2();
        Vector2 startPosition;
        isMoving = true;
        startPosition = transform.position;
        setDirection((int)currentDirection);
        // Set the new square to move to.
        switch (currentDirection)
        {
            case(Directions.Up):
                if (isFirstMove)
                {
                    endPosition = new Vector2(startPosition.x, startPosition.y + 0.5f);
                }
                else
                {
                    endPosition = new Vector2(startPosition.x, startPosition.y + 1f);
                }
                break;
            case(Directions.Left):
                if (isFirstMove)
                {
                    endPosition = new Vector2(startPosition.x - 0.5f, startPosition.y);
                }
                else
                {
                    endPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                }
                break;
            case(Directions.Down):
                if (isFirstMove)
                {
                    endPosition = new Vector2(startPosition.x, startPosition.y - 0.5f);
                }
                else
                {
                    endPosition = new Vector2(startPosition.x, startPosition.y - 1f);
                }
                break;
            case(Directions.Right):
                if (isFirstMove)
                {
                    endPosition = new Vector2(startPosition.x + 0.5f, startPosition.y);
                }
                else
                {
                    endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                }
                break;
        }

        if (isFirstMove)
        {
            isFirstMove = false;
        }

        while (t < 1f)
        {
            t += moveSpeed * Time.deltaTime;

            transform.position = Vector2.Lerp(startPosition, endPosition, t);
            yield return 0;
        }
        isMoving = false;
        yield break;
    }

    public IEnumerator HouseMove(Modes mode)
    {
        float t = 0;
        Vector2 endPosition = new Vector2();
        Vector2 startPosition;
        isMoving = true;
        startPosition = transform.position;


        switch (mode)
        {
            case(Modes.Idle):

                setDirection((int)currentIdleDirection);
                switch (currentIdleDirection)
                {
                    case(Directions.Up):
                        endPosition = new Vector2(startPosition.x, startPosition.y + 0.5f);
                        break;
                    case(Directions.Down):
                        endPosition = new Vector2(startPosition.x, startPosition.y - 0.5f);
                        break;
                }
                // If it can't go up anymore.
                if (endPosition.y == houseTile.y + 0.5f)
                {
                    currentIdleDirection = Directions.Down;
                }

                // If it can't go down anymore.
                if (endPosition.y == houseTile.y - 0.5f)
                {
                    currentIdleDirection = Directions.Up;
                }
                break;

            case(Modes.Leaving):
                setDirection((int)currentDirection);
                // If it is on the Left Side.
                if (startPosition.x - (respawnTile.x + 1) < 0)
                {
                    endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                }
                else
                {
                    // If it is on the Right Side
                    if (startPosition.x - (respawnTile.x + 1) > 0)
                    {
                        endPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                    }
                    else
                    {
                        // If it is just below the respawn point
                        endPosition = new Vector2(startPosition.x, startPosition.y + 1f);

                        // If the end position equals the respawn position,
                        // switch to the current game mode (Scatter/Chase/Frightened)
                        if (endPosition.y == respawnTile.y - 0.5f)
                        {
                            setMode(currentGlobalMode);
                        }
                    }
                }
                currentDirection = VectorToDirection(startPosition, endPosition);
                break;

            case(Modes.Entering):
                setDirection((int)currentDirection);

                // If it needs to align Y axis to the House Tile.
                if (startPosition.y - houseTile.y > 0)
                {
                    endPosition = new Vector2(startPosition.x, startPosition.y - 1f);
                }
                else
                {
                    // If it already aligned with the Y axis
                    if (startPosition.y - houseTile.y == 0)
                    {
                        // If it needs to go the Left side.
                        if (startPosition.x - houseTile.x > 0)
                        {
                            endPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                        }
                        else
                        {
                            // If it needs to go the Left side.
                            if (startPosition.x - houseTile.x < 0)
                            {
                                endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                            }
                            else
                            {
                                setMode(Modes.Idle);
                            }
                        }
                    }
                }
                currentDirection = VectorToDirection(startPosition, endPosition);
                break;
        }

        if (endPosition == Vector2.zero)
        {
            isMoving = false;
            yield break;
        }

        while (t < 1f)
        {
            t += moveSpeed * Time.deltaTime;

            transform.position = Vector2.Lerp(startPosition, endPosition, t);
            yield return 0;
        }

        isMoving = false;
        yield break;
    }

    public static double Euclidean(Vector2 p1, Vector2 p2)
    {
        return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
    }

    public Directions VectorToDirection(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 direction = startPoint - endPoint;

        // If Direction is Left.
        if (direction.x == 1f)
        {
            return Directions.Left;
        }

        // If Direction is Right.
        if (direction.x == -1f)
        {
            return Directions.Right;
        }

        // If Direction is Down.
        if (direction.y == 1f)
        {
            return Directions.Down;
        }

        // If Direction is Up
        if (direction.y == -1f)
        {
            return Directions.Up;
        }

        return Directions.Up;
    }

    protected Directions FindNextDirection(Vector2 startPosition, Directions direction, Modes mode)
    {
        var targetedTile = new Vector2();
        var nextPosition = new Vector2();
        var directions = new sbyte[4, 2] { { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 0 } };
        var allowedDirections = new List<Directions> { };
        var newDirection = new Directions();
        double lowestEuclidean = -1;

        // Set the new targeted tile according to the current mode.
        switch (mode)
        {
            case (Modes.Chase):
                targetedTile = targetTile;
                break;
            case (Modes.Scatter):
                targetedTile = scatterTile;
                break;
            case (Modes.Frightened):
                break;
            case (Modes.Respawning):
                targetedTile = respawnTile;
                break;
        }

        // Set the new square to move to.
        switch (currentDirection)
        {
            case (Directions.Up):
                if (isFirstMove)
                {
                    nextPosition = new Vector2(startPosition.x, startPosition.y + 0.5f);
                }
                else
                {
                    nextPosition = new Vector2(startPosition.x, startPosition.y + 1f);
                }
                break;
            case (Directions.Left):
                if (isFirstMove)
                {
                    nextPosition = new Vector2(startPosition.x - 0.5f, startPosition.y);
                }
                else
                {
                    nextPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                }
                break;
            case (Directions.Down):
                if (isFirstMove)
                {
                    nextPosition = new Vector2(startPosition.x, startPosition.y - 0.5f);
                }
                else
                {
                    nextPosition = new Vector2(startPosition.x, startPosition.y - 1f);
                }
                break;
            case (Directions.Right):
                if (isFirstMove)
                {
                    nextPosition = new Vector2(startPosition.x + 0.5f, startPosition.y);
                }
                else
                {
                    nextPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                }
                break;
        }

        // Check if the endPosition Tile is an intersection.
        if (gameController.GetPacTile((int)nextPosition.x, (int)Math.Abs(nextPosition.y)).allowedDirections.Count > 0)
        {

            allowedDirections = gameController.GetPacTile((int)nextPosition.x, (int)Math.Abs(nextPosition.y)).allowedDirections;
            // Check the lowest Euclidean on all allowed Directions
            // We start from the end so that the priority becomes Up, Left, Down, Right
            for (int i = allowedDirections.Count - 1; i >= 0; i--)
            {
                var tempPosition = new Vector2();
                switch (allowedDirections[i])
                {
                    case (Directions.Up):
                        // If the current direction is Up, it can't go Down.
                        if (currentDirection != Directions.Down)
                        {
                            tempPosition = new Vector2(nextPosition.x, nextPosition.y + 1);
                            if (Euclidean(tempPosition, targetedTile) <= lowestEuclidean || lowestEuclidean == -1)
                            {
                                lowestEuclidean = Euclidean(tempPosition, targetedTile);
                                newDirection = Directions.Up;
                            }
                        }
                        break;

                    case (Directions.Left):
                        // If the current direction is Right, it can't go Left.
                        if (currentDirection != Directions.Right)
                        {
                            tempPosition = new Vector2(nextPosition.x - 1, nextPosition.y);
                            if (Euclidean(tempPosition, targetedTile) <= lowestEuclidean || lowestEuclidean == -1)
                            {
                                lowestEuclidean = Euclidean(tempPosition, targetedTile);
                                newDirection = Directions.Left;
                            }
                        }
                        break;

                    case (Directions.Down):
                        // If the current direction is Up, it can't go Down.
                        if (currentDirection != Directions.Up)
                        {
                            tempPosition = new Vector2(nextPosition.x, nextPosition.y - 1);
                            if (Euclidean(tempPosition, targetedTile) <= lowestEuclidean || lowestEuclidean == -1)
                            {
                                lowestEuclidean = Euclidean(tempPosition, targetedTile);
                                newDirection = Directions.Down;
                            }
                        }
                        break;

                    case (Directions.Right):
                        // If the current direction is Left, it can't go Right.
                        if (currentDirection != Directions.Left)
                        {
                            tempPosition = new Vector2(nextPosition.x + 1, nextPosition.y);
                            if (Euclidean(tempPosition, targetedTile) <= lowestEuclidean || lowestEuclidean == -1)
                            {
                                lowestEuclidean = Euclidean(tempPosition, targetedTile);
                                newDirection = Directions.Right;
                            }
                        }
                        break;
                }
                if (DebugPath)
                {
                    print(lowestEuclidean + " | " + tempPosition + " | " + allowedDirections[i] + " | " + Euclidean(tempPosition, targetedTile));
                    print("Direction Chosen: " + nextDirection + " INDEX: " + i + " COUNT: " + (allowedDirections.Count - 1));
                }
            }
        }
        else
        {
            // If not, follow the only possible path (Without going backwards).
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                var tempPosition = new Vector2(nextPosition.x + directions[i, 0], nextPosition.y + directions[i, 1]);
                PacTile tempTile = gameController.GetPacTile((int)tempPosition.x, (int)(-1 * tempPosition.y));

                switch (VectorToDirection(nextPosition, tempPosition))
                {
                    case (Directions.Up):
                        if (tempTile.cost != 0 && currentDirection != Directions.Down)
                        {
                            newDirection = Directions.Up;
                        }
                        break;

                    case (Directions.Left):
                        if (tempTile.cost != 0 && currentDirection != Directions.Right)
                        {
                            newDirection = Directions.Left;
                        }
                        break;

                    case (Directions.Down):
                        if (tempTile.cost != 0 && tempTile.cost != 2 && currentDirection != Directions.Up)
                        {
                            newDirection = Directions.Down;
                        }
                        break;

                    case (Directions.Right):
                        if (tempTile.cost != 0 && currentDirection != Directions.Left)
                        {
                            newDirection = Directions.Right;
                        }
                        break;
                }
            }
        }
        return newDirection;
    }

    private void updateTile()
    {
        tile = new Vector2((int)transform.position.x, (int)Math.Abs(transform.position.y));
    }

    protected void RequestPathFind(Vector2 startPoint, Vector2 endPoint, GhostAI caller, List<Directions> bannedDirections, bool IsInsideHouse)
    {
        //pathFind.findPath(startPoint, endPoint, gameController.Tiles, bannedDirections, IsInsideHouse);
        List<object> arguments = new List<object>();
        arguments.Add(startPoint);
        arguments.Add(endPoint);
        arguments.Add(caller);
        arguments.Add(gameController.Tiles);
        arguments.Add(bannedDirections);
        arguments.Add(IsInsideHouse);
        pathFinder.RunWorkerAsync(arguments);
    }

    public void PathFinderCompleted(List<PathFind.Node> closedNodes)
    {
        path = closedNodes;
        bWorking = true;
    }

    void updateVulnParameter()
    {
        AnimatorStateInfo nextState = anim.GetNextAnimatorStateInfo(0);
        if (!nextState.Equals(null) && nextState.IsName("Base Layer.Vulnerable"))
        {
            setVulnerability(false);
        }
    }

    public void Death()
    {
        setRespawning(true);
        gameObject.layer = 13;
    }

    public void Respawn()
    {
        setRespawning(false);
        gameObject.layer = 9;
    }

    public void setMode(Modes newMode)
    {
        // If currentMode is not Idle, Entering or Leaving, revert direction
        if (currentMode != Modes.Idle && currentMode != Modes.Entering && currentMode != Modes.Leaving)
        {
            switch (currentDirection)
            {
                case (Directions.Up):
                    currentDirection = Directions.Down;
                    break;
                case (Directions.Left):
                    currentDirection = Directions.Right;
                    break;
                case (Directions.Down):
                    currentDirection = Directions.Up;
                    break;
                case (Directions.Right):
                    currentDirection = Directions.Left;
                    break;
            }
        }
        if (newMode != Modes.Leaving && currentMode != Modes.Idle && currentMode != Modes.Entering)
        {
            currentMode = newMode;
        }
        else
        {
            if (newMode == Modes.Leaving && currentMode == Modes.Idle)
            {
                currentMode = newMode;
            }
        }
        
    }

    public void setGlobalMode(Modes newMode)
    {
        // If currentMode is not Idle, Entering, Leaving or Respawning, set new mode
        if (currentMode != Modes.Idle && currentMode != Modes.Entering && currentMode != Modes.Leaving)
        {
            setMode(newMode);
        }
        else
        {
            currentGlobalMode = newMode;
        }
        
    }

    public void IncreaseDotCount()
    {
        dotCount++;
    }

    // Sets Direction parameter
    public void setDirection(int direction)
    {
        anim.SetInteger("Direction", direction);
    }

    // Sets Vulnerability parameter
    public void setVulnerability(bool vuln)
    {
        anim.SetBool("Vulnerable", vuln);

        if (vuln)
        {
            isVulnerable = vuln;
            setMode(Modes.Frightened);
        }
    }

    // Sets Animator VulnerabilityEnding parameter
    public void setVulnerabilityEnd(bool vulnEnd, bool changeMode)
    {
        anim.SetBool("VulnerableEnding", vulnEnd);
        
        if (changeMode)
        {
            isVulnerable = false;
            setMode(currentGlobalMode);
        }
    }

    // Sets Animator Respawning parameter and disable collision
    public void setRespawning(bool respawn)
    {
        anim.SetBool("Respawning", respawn);
        setMode(Modes.Respawning);
    }

    // Sets Animator to back to the default animation
    public void returnToDefault()
    {
        anim.Play("Run Left");
    }

    public void DebugPathFind()
    {
        if (!DebugPath)
        {
            return;
        }
        var debugTile = new Vector2();

        switch (currentMode)
        {
            case(Modes.Chase):
                debugTile = targetTile;
                break;
            case(Modes.Scatter):
                debugTile = scatterTile;
                break;
            case(Modes.Frightened):
                break;
            case(Modes.Respawning):
                debugTile = respawnTile;
                break;
        }
        
        // Current Center Tile.
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y), new Vector3(tile.x + 1, -1 * tile.y), new Color(130,0,255,255), 0.0f, false);
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y - 1), new Vector3(tile.x + 1, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.0f, false);
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y), new Vector3(tile.x, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.1f, false);
        Debug.DrawLine(new Vector3(tile.x + 1, -1 * tile.y), new Vector3(tile.x + 1, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.0f, false);
        // Current Target Tile
        Debug.DrawLine(new Vector3(debugTile.x, debugTile.y), new Vector3(debugTile.x + 1, debugTile.y), DebugPathColor, 0.0f, false);
        Debug.DrawLine(new Vector3(debugTile.x, debugTile.y - 1), new Vector3(debugTile.x + 1, debugTile.y - 1), DebugPathColor, 0.0f, false);
        Debug.DrawLine(new Vector3(debugTile.x, debugTile.y), new Vector3(debugTile.x, debugTile.y - 1), DebugPathColor, 0.0f, false);
        Debug.DrawLine(new Vector3(debugTile.x + 1, debugTile.y), new Vector3(debugTile.x + 1, debugTile.y - 1), DebugPathColor, 0.0f, false);
        
        /*for (int i = 0; i < pathFind.closedNodes.Count; i++)
        {
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x + 1, pathFind.closedNodes[i].y * -1), DebugPathColor, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, -1 * pathFind.closedNodes[i].y - 1), new Vector3(pathFind.closedNodes[i].x + 1, -1 * pathFind.closedNodes[i].y - 1), DebugPathColor, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x, -1 * pathFind.closedNodes[i].y - 1), DebugPathColor, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.closedNodes[i].x + 1, pathFind.closedNodes[i].y * -1), new Vector3(pathFind.closedNodes[i].x + 1, -1 * pathFind.closedNodes[i].y - 1), DebugPathColor, 0.1f, false);
        }

        for (int i = 0; i < pathFind.openNodes.Count; i++)
        {
            Debug.DrawLine(new Vector3(pathFind.openNodes[i].x, pathFind.openNodes[i].y * -1), new Vector3(pathFind.openNodes[i].x + 1, -1 * pathFind.openNodes[i].y - 1), Color.blue, 0.1f, false);
            Debug.DrawLine(new Vector3(pathFind.openNodes[i].x + 1, pathFind.openNodes[i].y * -1), new Vector3(pathFind.openNodes[i].x, -1 * pathFind.openNodes[i].y - 1), Color.blue, 0.1f, false);
        }

        Debug.DrawLine(new Vector3(pathFind.lowestNode.x, pathFind.lowestNode.y * -1), new Vector3(pathFind.lowestNode.x + 1, pathFind.lowestNode.y * -1), Color.blue, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.lowestNode.x, -1 * pathFind.lowestNode.y - 1), new Vector3(pathFind.lowestNode.x + 1, -1 * pathFind.lowestNode.y - 1), Color.blue, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.lowestNode.x, pathFind.lowestNode.y * -1), new Vector3(pathFind.lowestNode.x, -1 * pathFind.lowestNode.y - 1), Color.blue, 0.1f, false);
        Debug.DrawLine(new Vector3(pathFind.lowestNode.x + 1, pathFind.lowestNode.y * -1), new Vector3(pathFind.lowestNode.x + 1, -1 * pathFind.lowestNode.y - 1), Color.blue, 0.1f, false);
        */
    }

    void OnApplicationQuit()
    {
        pathFinder.CancelAsync();
    }
}