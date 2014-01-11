using UnityEngine;
using System.Collections;

public class GhostAI : MonoBehaviour {

    private Animator anim;
	
	// Use this for initialization
    public void Start()
    {
        anim = GetComponent<Animator>(); 
    }

	// Update is called once per frame
	public void Update () 
    {
        updateVulnParameter();
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
}