using System;
using UnityEngine;
using System.Collections;

public class Pacman : MonoBehaviour {

    #region Variables
    public GameController gameController;
    private Animator anim;

    [HideInInspector]
    public Vector2 tile;

    private bool isAlive = true;
    public bool firstMove;

    public float moveSpeed;

    private bool isMoving = false;
    private Vector2 input;
    #endregion

	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();

        updatePosition();

        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }
	}

    // Update is called once per frame
    void Update()
    {
        if (isAlive == true)
        {
            updateAxis();
            if (!isMoving && input != Vector2.zero)
            {
                StartCoroutine(move());
            }
        }

        // Always stop Running animation after Pacman moves a tile.
        if (input == Vector2.zero)
        {
            anim.SetBool("Running", false);
        }
	}

    //Kills Pacman and plays death animation
    public void Death()
    {
        isAlive = false;
        transform.Rotate(new Vector3(0, 0, -transform.eulerAngles.z), Space.World);
        anim.SetBool("Running", false);
        anim.SetBool("Dead", true);
        input = Vector2.zero;
        gameController.PacmanDeath();
    }

    //Sets Pacman position to respawn
    void Respawn()
    {
        transform.position = GameObject.Find("Pacman Respawn").transform.position;
        isAlive = true;
        firstMove = true;
        anim.SetBool("Dead", false);
    }

    void playDeathBeep()
    {

    }

    public IEnumerator move()
    {
        float newZ, t = 0;
        Vector2 endPosition, startPosition;
        isMoving = true;

        startPosition = transform.position;
        
        // If its the first time Pacman moving from the respawn point,
        // Pacman needs to move 0.5f instead of 1.0f because of the respawn location
        if (firstMove){
            endPosition = new Vector2(startPosition.x + System.Math.Sign(input.x) - 0.5f * input.x,
                                      startPosition.y + System.Math.Sign(input.y));
        }else{
            endPosition = new Vector2(startPosition.x + System.Math.Sign(input.x),
                                      startPosition.y + System.Math.Sign(input.y));
        }

        if (input.x != 0)
        {
            newZ = input.x * 180;
            if (newZ == -180) newZ = 0;
        }else
        {
            newZ = input.y * -90;
        }

        while (t < 1f)
        {
            if (firstMove)
            {
                t += (moveSpeed * 2) * Time.deltaTime;
            }else{
                t += moveSpeed * Time.deltaTime;
            }
            
            transform.position = Vector2.Lerp(startPosition, endPosition, t);

            updatePosition();
            updateAxis();

            //Rotate Pacman to face the current direction
            transform.Rotate(new Vector3(0, 0, -transform.eulerAngles.z), Space.World);
            transform.Rotate(new Vector3(0, 0, newZ), Space.World);

            anim.SetBool("Running", true);
            yield return 0;
        }
        t = 0;
        isMoving = false;
        firstMove = false;
        yield break;
    }

    //Check if the player pressed any axis button, ensure it's a valid move and update input accordingly
    //also check if Pacman auto-movement has hit a wall
    private void updateAxis()
    {
        if (!isMoving)
        {
            if (Input.GetAxisRaw("Horizontal") != 0
                && gameController.getTile(tile.x + Input.GetAxisRaw("Horizontal"), tile.y) != 0
                && gameController.getTile(tile.x + Input.GetAxisRaw("Horizontal"), tile.y) != 2)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = 0;
            }

            if (Input.GetAxisRaw("Vertical") != 0
                && gameController.getTile(tile.x, tile.y - Input.GetAxisRaw("Vertical")) != 0
                && gameController.getTile(tile.x, tile.y - Input.GetAxisRaw("Vertical")) != 2)
            {
                input.x = 0;
                input.y = Input.GetAxisRaw("Vertical");
            }

            if (input.x != 0
                && (gameController.getTile(tile.x + input.x, tile.y) == 0
                || gameController.getTile(tile.x + input.x, tile.y) == 2))
            {
                input.x = 0;
            }

            if (input.y != 0
                && (gameController.getTile(tile.x, tile.y - input.y) == 0
                || gameController.getTile(tile.x, tile.y - input.y) == 2))
            {
                input.y = 0;
            }
        }
    }

    //For some reason tile coordinates do not round down if the coordinates are above 0.5
    //So to fix it, always add/remove 0.25 (depending on direction) and round down
    private void updatePosition()
    {
        //tileX = Mathf.FloorToInt(Vector2.Lerp(startPosition, endPosition, t).x - 0.25f);
        //tileY = Mathf.FloorToInt(-1 * (Vector2.Lerp(startPosition, endPosition, t).y - 0.25f));
        tile = new Vector2((int)transform.position.x, (int)Math.Abs(transform.position.y));
    }

}
