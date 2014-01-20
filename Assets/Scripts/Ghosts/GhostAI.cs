using UnityEngine;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

public class GhostAI : MonoBehaviour {

    #region Variables
    public Color DebugPathColor;
    public bool DebugPath;
    private Animator anim;
    protected GameController gameController;
    protected PathFind pathFind = new PathFind();
    protected BackgroundWorker pathFinder = new BackgroundWorker();
    protected bool bWorking;
    
    #endregion

    // Use this for initialization
    public void Start()
    {

        anim = GetComponent<Animator>();

        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }

        InitializePathFinder();
    }

    void OnApplicationQuit()
    {
        pathFinder.CancelAsync();
    }

	// Update is called once per frame
	public void Update ()
    {
        updateVulnParameter();
        
        if (DebugPath)
        {
            DebugPathFind();
        }
        
	}

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

    protected void RequestPathFind(Vector2 startPoint, Vector2 endPoint, GhostAI caller, sbyte[,] bannedDirections, bool IsInsideHouse)
    {
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

    // Sets Animator Vulnerability parameter
    public void setVulnerability(bool vuln)
    {
        anim.SetBool("Vulnerable", vuln);
    }

    // Sets Animator VulnerabilityEnding parameter
    public void setVulnerabilityEnd(bool vulnEnd)
    {
        anim.SetBool("VulnerableEnding", vulnEnd);
    }

    // Sets Animator Respawning parameter and disable collision
    public void setRespawning(bool respawn)
    {
        anim.SetBool("Respawning", respawn);
    }

    // Sets Animator to back to the default animation
    public void returnToDefault()
    {
        anim.Play("Run Left");
    }

    public void DebugPathFind()
    {
        for (int i = 0; i < pathFind.closedNodes.Count; i++)
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

    }
}