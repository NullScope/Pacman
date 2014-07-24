using UnityEngine;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System;

public class GhostAI : MonoBehaviour {

    #region Enums
    public enum Modes { Scatter, Chase, Frightened, Respawning }
    public enum HouseModes { None, Entering, Leaving, Idle }
    public enum Directions { Up, Left, Down, Right }
    #endregion

    #region Variables
    public Pacman player;

    public Color DebugPathColor;
    public bool DebugPath;

    private Animator anim;

    protected Modes currentMode;
    protected HouseModes houseMode;
    protected Modes currentGlobalMode;
    public Modes startMode;
    public HouseModes startHouseMode;

    protected Directions currentDirection;
    protected Directions currentIdleDirection;
    protected Directions nextDirection;
    public Directions startDirection;

    protected Vector2 targetTile;
    public Vector2 tile;
    public Vector2 scatterTile;
    public Vector2 respawnTile;
    public Vector2 houseTile;

    public GameController gameController;

    protected bool isMoving;
    private bool isFirstMove;
    public bool isVulnerable;
    public bool isOutside;
    public bool inTunnel;
    protected int dotCount;
    public int dotLimit;
    public bool isWarping;
    private float moveSpeed;
    public float maxSpeed;
    public float[] defaultSpeedPercentages;
    public float[] tunnelSpeedPercentages;
    public float[] frightSpeedPercentages;
    #endregion

    // Use this for initialization
    public void Start()
    {
        isFirstMove = true;
        currentMode = startMode;
        houseMode = startHouseMode;
        nextDirection = startDirection;
        anim = GetComponent<Animator>();

        UpdateTile();

        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }

        if (player == null)
        {
            player = gameController.player;
        }


        //InitializePathFinder();
    }

    /**DEPRECATED**/
    /*private void InitializePathFinder()
    {
        pathFind.gameController = gameController;
        pathFinder.WorkerSupportsCancellation = true;
        // Attach event handlers to the BackgroundWorker object.
        pathFinder.DoWork +=
            new System.ComponentModel.DoWorkEventHandler(pathFind.DoWork);
        pathFinder.RunWorkerCompleted +=
            new System.ComponentModel.RunWorkerCompletedEventHandler(pathFind.WorkerCompleted);
    }*/

	// Update is called once per frame
	public void Update ()
    {
        if (gameController.isPaused || !player.isAlive)
        {
            return;
        }

        UpdateVulnParameter();
        UpdateTile();
        UpdatePosition();
        UpdateDotCount();
        UpdateCollision();
        DebugPathFind();
	}

    public void UpdatePosition() 
    {
        if (isMoving)
        {
            return;
        }

        if (houseMode == HouseModes.None)
        {
            currentDirection = nextDirection;
            nextDirection = FindNextDirection(transform.position, currentDirection, currentMode);
            StartCoroutine(move(currentMode));
        }
        else
        {
            StartCoroutine(HouseMove(houseMode));
        }
    }

    public void UpdateDotCount()
    {
        if (gameController.bGlobalCounter)
        {
            return;
        }

        if (dotCount >= dotLimit && !isOutside && houseMode == HouseModes.Idle)
        {
            leaveGhostHouse();
        }
    }

    public void UpdateCollision()
    {
        // if the tile is no the same tile as pacman OR
        // if the the ghost is respawning don't check for collision
        if (currentMode == Modes.Respawning || tile != player.tile)
        {
            return;
        }
        
        // Check if the ghost is in fright mode.
        if (currentMode == Modes.Frightened)
        {
            gameController.StartCoroutine(gameController.Pause(1f));
            Death();
        }
        else
        {
            if (houseMode == HouseModes.None && player.isAlive)
            {
                gameController.StartCoroutine(gameController.Pause(1f));
                gameController.blinky.StopAllCoroutines();
                gameController.pinky.StopAllCoroutines();
                gameController.inky.StopAllCoroutines();
                gameController.clyde.StopAllCoroutines();
                player.Death();
            }
        }
    }

    public void leaveGhostHouse()
    {
        StopAllCoroutines();
        isMoving = false;
        transform.position = houseTile;
        SetHouseMode(HouseModes.Leaving);
    }

    public void enterGhostHouse()
    {
        StopAllCoroutines();
        isMoving = false;
        SetHouseMode(HouseModes.Entering);
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
                    endPosition = new Vector2(startPosition.x, startPosition.y + 0.5f);
                else
                    endPosition = new Vector2(startPosition.x, startPosition.y + 1f);
                break;

            case(Directions.Left):
                if (isFirstMove)
                    endPosition = new Vector2(startPosition.x - 0.5f, startPosition.y);
                else
                    endPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                break;

            case(Directions.Down):
                if (isFirstMove)
                    endPosition = new Vector2(startPosition.x, startPosition.y - 0.5f);
                else
                    endPosition = new Vector2(startPosition.x, startPosition.y - 1f);
                break;

            case(Directions.Right):
                if (isFirstMove)
                    endPosition = new Vector2(startPosition.x + 0.5f, startPosition.y);
                else
                    endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                break;
        }

        // If the tile the ghost wants to go to is a Tunnel tile, set inTunnel flag

        if (!isWarping)
        {
            if (gameController.GetPacTile((int)endPosition.x, (int)endPosition.y * -1).cost == 3)
            {
                inTunnel = true;
            }
            else
            {
                inTunnel = false;
            }
        }

        UpdateMoveSpeed();

        while (t < 1f)
        {
            if (isFirstMove)
                t += (moveSpeed * 2) * Time.smoothDeltaTime;
            else
                t += moveSpeed * Time.smoothDeltaTime;

            transform.position = Vector2.Lerp(startPosition, endPosition, t);

            if (t >= 1f)
                break;
            else
                yield return 1;
        }

        if (isWarping)
        {
            // Warp is on the left.
            if (transform.position.x < 0)
            {
                transform.position = new Vector2(gameController.pacTiles.GetLength(0)+0.5f, transform.position.y);
            }
            else
            {   
                // Warp is on the right.
                if (transform.position.x >= gameController.pacTiles.GetLength(0))
                {
                    transform.position = new Vector2(-0.5f, transform.position.y);
                }
            }

            // Warp is on top.
            if (transform.position.y > 0)
            {
                transform.position = new Vector2(transform.position.x, gameController.pacTiles.GetLength(1)+0.5f);
            }
            else
            {
                // Warp is on bottom.
                if (transform.position.y >= gameController.pacTiles.GetLength(1))
                {
                    transform.position = new Vector2(transform.position.x, 0.5f);
                }
            }

            isWarping = false;
        }

        if (currentMode == Modes.Respawning && tile == new Vector2(respawnTile.x, Math.Abs(respawnTile.y)))
        {
            enterGhostHouse();
        }

        isMoving = false;
        isFirstMove = false;
        yield break;
    }

    public IEnumerator HouseMove(HouseModes mode)
    {
        float t = 0;
        var endPosition = new Vector2();
        var startPosition = new Vector2();
        isMoving = true;
        startPosition = transform.position;

        setDirection((int)currentDirection);

        switch (mode)
        {
            case(HouseModes.Idle):

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

            case(HouseModes.Leaving):
                
                // If it is on the Left Side.
                if (tile.x - (respawnTile.x + 1) < 0)
                {
                    endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                }
                else
                {
                    // If it is on the Right Side
                    if (tile.x - (respawnTile.x + 1) > 0)
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
                            if (isVulnerable)
                                setMode(Modes.Frightened);
                            else
                                setMode(currentGlobalMode);

                            SetHouseMode(HouseModes.None);
                            isOutside = true;
                            nextDirection = startDirection;
                        }
                    }
                }
                break;

            case(HouseModes.Entering):

                if (new Vector2(tile.x, -1 * tile.y) == respawnTile)
                {
                    endPosition = new Vector2(startPosition.x + 0.5f, startPosition.y);
                }
                else
                {
                    if (startPosition.y != houseTile.y)
                    {
                        endPosition = new Vector2(startPosition.x, startPosition.y - 1f);
                    }
                    else
                    {
                        // If it is on the Left Side.
                        if (startPosition.x - houseTile.x < 0)
                        {
                            endPosition = new Vector2(startPosition.x + 1f, startPosition.y);
                        }
                        else
                        {
                            // If it is on the Right Side
                            if (startPosition.x - houseTile.x > 0)
                            {
                                endPosition = new Vector2(startPosition.x - 1f, startPosition.y);
                            }
                            else
                            {
                                SetHouseMode(HouseModes.Idle);
                                Respawn();
                            }
                        }
                    }
                }
                break;
        }

        if (endPosition == Vector2.zero)
        {
            isMoving = false;
            yield break;
        }

        UpdateMoveSpeed();

        while (t < 1f)
        {
            t += moveSpeed * Time.smoothDeltaTime;

            transform.position = Vector2.Lerp(startPosition, endPosition, t);

            if (t >= 1f)
                break;
            else
                yield return 1;
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

        // Special check for warp zones
        if ((tile.x <= 0 && currentDirection == Directions.Left) || (tile.x >= gameController.pacTiles.GetLength(0) - 2 && currentDirection == Directions.Right) || (tile.y <= 0 && currentDirection == Directions.Up) || (tile.y >= gameController.pacTiles.GetLength(1) - 2 && currentDirection == Directions.Down))
        {
            isWarping = true;
            return currentDirection;
        }

        // If the ghost is out of the map, continue on the same direction
        if (tile.x < 0 || tile.x >= gameController.pacTiles.GetLength(0) || tile.y < 0 || tile.y >= gameController.pacTiles.GetLength(1))
        {
            return currentDirection;
        }

        // Check if the endPosition Tile is an intersection.
        if (gameController.GetPacTile((int)nextPosition.x, (int)Math.Abs(nextPosition.y)).allowedDirections.Count > 0)
        {
            // If the ghost is currently Frightened, 
            // remove the inverted direction and randomize.
            if (mode == Modes.Frightened)
            {
                var tempAllowedDirections = new List<Directions> { };
                Directions invertedDirection = Directions.Up;
                Directions frightDirection = Directions.Up;
                tempAllowedDirections = gameController.GetPacTile((int)nextPosition.x, (int)Math.Abs(nextPosition.y)).allowedDirections;

                switch (direction)
                {
                    case(Directions.Up):
                        invertedDirection = Directions.Down;
                        break;
                    case(Directions.Left):
                        invertedDirection = Directions.Right;
                        break;
                    case(Directions.Down):
                        invertedDirection = Directions.Up;
                        break;
                    case(Directions.Right):
                        invertedDirection = Directions.Left;
                        break;
                }

                frightDirection = invertedDirection;

                while (frightDirection == invertedDirection)
                {
                    frightDirection = tempAllowedDirections[UnityEngine.Random.Range(0, tempAllowedDirections.Count)];
                }

                return frightDirection;
            }
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

                /*if (DebugPath)
                {
                    print(lowestEuclidean + " | " + tempPosition + " | " + allowedDirections[i] + " | " + Euclidean(tempPosition, targetedTile));
                    print("Direction Chosen: " + nextDirection + " INDEX: " + i + " COUNT: " + (allowedDirections.Count - 1));
                }*/
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

    private void UpdateTile()
    {
        tile = new Vector2((int)transform.position.x, (int)Math.Abs(transform.position.y));
    }

    void UpdateVulnParameter()
    {
        AnimatorStateInfo nextState = anim.GetNextAnimatorStateInfo(0);
        if (!nextState.Equals(null) && nextState.IsName("Base Layer.Vulnerable"))
        {
            setVulnerability(false);
        }
    }

    void UpdateMoveSpeed()
    {
        // The last index will be used after the level surpasses the array length. 

        if (isVulnerable)
        {
            if (gameController.level > frightSpeedPercentages.Length - 1)
                moveSpeed = maxSpeed * (frightSpeedPercentages[frightSpeedPercentages.Length - 1] / 100);
            else
                moveSpeed = maxSpeed * (frightSpeedPercentages[gameController.level] / 100);
        }
        else
        {
            if (currentMode == Modes.Respawning && isOutside)
            {
                moveSpeed = maxSpeed * 2;
            }
            else
            {
                if (inTunnel)
                {
                    if (gameController.level > tunnelSpeedPercentages.Length - 1)
                        moveSpeed = maxSpeed * (tunnelSpeedPercentages[tunnelSpeedPercentages.Length - 1] / 100);
                    else
                        moveSpeed = maxSpeed * (tunnelSpeedPercentages[gameController.level] / 100);
                }
                else
                {
                    if (gameController.level > defaultSpeedPercentages.Length - 1)
                        moveSpeed = maxSpeed * (defaultSpeedPercentages[defaultSpeedPercentages.Length - 1] / 100);
                    else
                        moveSpeed = maxSpeed * (defaultSpeedPercentages[gameController.level] / 100);
                }
            }
        }
    }

    public void Death()
    {
        anim.SetBool("Respawning", true);
        setMode(Modes.Respawning);
        isVulnerable = false;
    }

    public void Respawn()
    {
        anim.SetBool("Respawning", false);
        anim.SetBool("Vulnerable", false);
        anim.SetBool("VulnerableEnding", false);
        isVulnerable = false;
        isFirstMove = true;
        isMoving = false;
    }

    public void setMode(Modes newMode)
    {
        if(currentMode == Modes.Respawning && isOutside)
        {
            return;
        }

        switch (currentDirection)
        {
            case (Directions.Up):
                nextDirection = Directions.Down;
                break;
            case (Directions.Left):
                nextDirection = Directions.Right;
                break;
            case (Directions.Down):
                nextDirection = Directions.Up;
                break;
            case (Directions.Right):
                nextDirection = Directions.Left;
                break;
        }
        
        currentMode = newMode;
    }

    public void SetHouseMode(HouseModes newHouseMode)
    {
        houseMode = newHouseMode;
    }

    public void setGlobalMode(Modes newMode)
    {
        // If currentMode Respawning, set new mode
        if (currentMode != Modes.Respawning && currentMode != Modes.Frightened)
        {
            setMode(newMode);
            currentGlobalMode = newMode;
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
        
        if (vulnEnd)
        {
            anim.SetBool("Vulnerable", false);
        }
   
        if (changeMode)
        {
            isVulnerable = false;
            setMode(currentGlobalMode);
        }
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
            case(Modes.Frightened):
                break;
            case(Modes.Respawning):
                debugTile = respawnTile;
                break;
            default:
                debugTile = scatterTile;
                break;
        }
        
        // Current Center Tile.
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y), new Vector3(tile.x + 1, -1 * tile.y), new Color(130, 0, 255, 255), 0.0f, false);
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
}