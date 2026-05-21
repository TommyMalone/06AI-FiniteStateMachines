using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class GameEnvironment
{
    private static GameEnvironment instance;
    private List<GameObject> checkpoints = new List<GameObject>();
    public  List<GameObject> Checkpoints
    {
        get { return checkpoints; }
    } 
    
    public static GameEnvironment Singleton
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEnvironment();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
            }

            return instance;
        }
    }
}
