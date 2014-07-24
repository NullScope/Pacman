using UnityEngine;
using System.Collections;

public class GameLevel : MonoBehaviour
{
    public int level;
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
