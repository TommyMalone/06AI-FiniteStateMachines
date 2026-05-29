using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class GameEnvironment
{
    private static GameEnvironment _instance;
    public  List<GameObject> Checkpoints { get; private set; } = new List<GameObject>();

    public  List<GameObject> SafeSpots { get; } = new List<GameObject>();

    public static GameEnvironment Singleton
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameEnvironment();
                _instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));

                _instance.Checkpoints = _instance.Checkpoints.OrderBy(waypoint => waypoint.name).ToList();
                
                _instance.SafeSpots.AddRange(GameObject.FindGameObjectsWithTag("Safe"));
            }

            return _instance;
        }
    }
}
