﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SpatialGrid : MonoBehaviour
{
    public static SpatialGrid instance;
    
    #region Variables
    
    public float x;
    public float z;
    public float cellWidth;
    public float cellHeight;
    public int width;
    public int height;
    
    private Dictionary<IEntity, Tuple<int, int>> lastPositions;
    private HashSet<IEntity>[,] buckets;


    readonly public Tuple<int, int> Outside = Tuple.Create(-1, -1);

    //Una colección vacía a devolver en las queries si no hay nada que devolver
    readonly public IEntity[] Empty = new IEntity[0];
    #endregion

    #region FUNCIONES
    private void Awake()
    {
        instance = this;
        
        lastPositions = new Dictionary<IEntity, Tuple<int, int>>();
        buckets = new HashSet<IEntity>[width, height];
        
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                buckets[i, j] = new HashSet<IEntity>();
        
        var ents = RecursiveWalker(transform)
            .Select(x => x.GetComponent<IEntity>())
            .Where(x => x != null);

        Debug.Log("ENTS TOTAL: " + ents.Count());
        
        foreach (var e in ents)
        {
            e.OnMove += UpdateEntity;
            UpdateEntity(e);
        }
    }

    public void AddEntity(IEntity entity)
    {
        entity.OnMove += UpdateEntity;
        UpdateEntity(entity);
    }

    public void UpdateEntity(IEntity entity)
    {
        var lastPos = lastPositions.ContainsKey(entity) ? lastPositions[entity] : Outside;
        var currentPos = GetPositionInGrid(entity.myGameObject.transform.position);
        
        if (lastPos.Equals(currentPos))
            return;
        
        if (IsInsideGrid(lastPos))
            buckets[lastPos.Item1, lastPos.Item2].Remove(entity);
        
        if (IsInsideGrid(currentPos))
        {
            buckets[currentPos.Item1, currentPos.Item2].Add(entity);
            lastPositions[entity] = currentPos;
        }
        else
            lastPositions.Remove(entity);
    }

    public IEnumerable<IEntity> Query(Vector3 aabbFrom, Vector3 aabbTo, Func<Vector3, bool> filterByPosition)
    {
        var from = new Vector3(Mathf.Min(aabbFrom.x, aabbTo.x), 0, Mathf.Min(aabbFrom.z, aabbTo.z));
        var to = new Vector3(Mathf.Max(aabbFrom.x, aabbTo.x), 0, Mathf.Max(aabbFrom.z, aabbTo.z));

        var fromCoord = GetPositionInGrid(from);
        var toCoord = GetPositionInGrid(to);
        
        fromCoord = Tuple.Create(Utility.Clampi(fromCoord.Item1, 0, width), Utility.Clampi(fromCoord.Item2, 0, height));
        toCoord = Tuple.Create(Utility.Clampi(toCoord.Item1, 0, width), Utility.Clampi(toCoord.Item2, 0, height));

        if (!IsInsideGrid(fromCoord) && !IsInsideGrid(toCoord))
            return Empty;
        
        var cols = Generate(fromCoord.Item1, x => x + 1)
            .TakeWhile(x => x < width && x <= toCoord.Item1);

        var rows = Generate(fromCoord.Item2, y => y + 1)
            .TakeWhile(y => y < height && y <= toCoord.Item2);

        var cells = cols.SelectMany(
            col => rows.Select(
                row => Tuple.Create(col, row)
            )
        );
        
        return cells
            .SelectMany(cell => buckets[cell.Item1, cell.Item2])
            .Where(e =>
                from.x <= e.Position.x && e.Position.x <= to.x &&
                from.z <= e.Position.z && e.Position.z <= to.z
            ).Where(x => filterByPosition(x.Position));
    }

    public Tuple<int, int> GetPositionInGrid(Vector3 pos)
    {
        return Tuple.Create(Mathf.FloorToInt((pos.x - x) / cellWidth),
                            Mathf.FloorToInt((pos.z - z) / cellHeight));
    }

    public bool IsInsideGrid(Tuple<int, int> position)
    {
        return 0 <= position.Item1 && position.Item1 < width &&
               0 <= position.Item2 && position.Item2 < height;
    }

    void OnDestroy()
    {
        var ents = RecursiveWalker(transform).Select(x => x.GetComponent<IEntity>()).Where(x => x != null);
        foreach (var e in ents)
            e.OnMove -= UpdateEntity;
    }

    #endregion
    
    #region GENERATORS
    private static IEnumerable<Transform> RecursiveWalker(Transform parent)
    {
        foreach (Transform child in parent)
        {
            foreach (Transform grandchild in RecursiveWalker(child))
                yield return grandchild;
            yield return child;
        }
    }

    IEnumerable<T> Generate<T>(T seed, Func<T, T> mutate)
    {
        T accum = seed;
        while (true)
        {
            yield return accum;
            accum = mutate(accum);
        }
    }

    #endregion

    #region GRAPHIC REPRESENTATION
    public bool AreGizmosShutDown;
    public bool activatedGrid;
    public bool showLogs = true;
    private void OnDrawGizmos()
    {
        var rows = Generate(z, curr => curr + cellHeight)
                .Select(row => Tuple.Create(new Vector3(x, 0, row),
                                            new Vector3(x + cellWidth * width, 0, row)));
        

        var cols = Generate(x, curr => curr + cellWidth)
                   .Select(col => Tuple.Create(new Vector3(col, 0, z), new Vector3(col, 0, z + cellHeight * height)));

        var allLines = rows.Take(width + 1).Concat(cols.Take(height + 1));

        foreach (var elem in allLines)
        {
            Gizmos.DrawLine(elem.Item1, elem.Item2);
        }

        if (buckets == null || AreGizmosShutDown) return;

        var originalCol = GUI.color;
        GUI.color = Color.red;
        
        if (!activatedGrid)
        {
            IEnumerable<IEntity> allElems = Enumerable.Empty<IEntity>();
            foreach(var elem in buckets)
                allElems = allElems.Concat(elem);

            int connections = 0;
            foreach (var ent in allElems)
            {
                foreach(var neighbour in allElems.Where(x => x != ent))
                {
                    Gizmos.DrawLine(ent.Position, neighbour.Position);
                    connections++;
                }
                if(showLogs)
                    Debug.Log("tengo " + connections + " conexiones por individuo");
                connections = 0;
            }
        }

        GUI.color = originalCol;
        showLogs = false;
    }
    #endregion
    
    #region CUSTOM FUNC

    //IA2-P2
    
    public IEnumerable<HashSet<IEntity>> GetHashValues()
    {
        foreach (var ent in buckets)
        {
            yield return ent;
        }
    }


    public HashSet<IEntity> GetBucket(Tuple<int, int> index)
    {
        if (IsInsideGrid(index))
            return buckets[index.Item1, index.Item2];
        
        return default;
    }
    #endregion
    
    
}
