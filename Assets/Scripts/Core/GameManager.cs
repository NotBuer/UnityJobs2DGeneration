using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [SerializeField]
    private List<Player> _connectedPlayersList;


    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { DestroyImmediate(Instance); }

        _connectedPlayersList = new();
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void AddNewPlayerConnected(Player player)
    {
        if (!player) return;
        _connectedPlayersList.Add(player);
    }
}
