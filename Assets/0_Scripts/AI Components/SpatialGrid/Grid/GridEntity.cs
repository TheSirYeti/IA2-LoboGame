using System;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    [Header("Grid values")]
    public bool onGrid;
    public event Action<GridEntity> OnMove = delegate {};

    void Update() 
    {
        OnMove(this);
    }
}
