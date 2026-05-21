using System.Collections.Generic;
using System.Linq;
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
    private List<GameObject> safeSpots = new List<GameObject>();
    public  List<GameObject> SafeSpots
    {
        get { return safeSpots; }
    } 
    
    public static GameEnvironment Singleton
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEnvironment();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));

                instance.checkpoints = instance.checkpoints.OrderBy(waypoint => waypoint.name).ToList();
                
                instance.SafeSpots.AddRange(GameObject.FindGameObjectsWithTag("Safe"));
            }

            return instance;
        }
    }
}
