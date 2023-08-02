using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;

public abstract class BaseEnemy : MonoBehaviour, IEntity
{
    [Header("Base Properties")] 
    [SerializeField] protected float _hp;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float searchRange;

    [Header("Pathfinding Properties")] 
    [SerializeField] protected List<Node> currentPath;
    protected int currentNode = 0;
    protected float minDistanceToNode = 2f;

    public Vector3 Position { get; }

    public float Health
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
        }
    }

    public GameObject myGameObject
    {
        get
        {
            return gameObject;
        }
    }

    public bool IsEnemy
    {
        get
        {
            return true;
        }
    }

    public IEntity FindNearestTarget(Vector3 position)
    {
        var objectsInRange = Physics.OverlapSphere(transform.position, searchRange);

        var finalEntity = objectsInRange.Aggregate(FList.Create<IEntity>(), (flist, listObject) =>
        {
            listObject.TryGetComponent(out IEntity entity);

            flist = entity != null ? flist + entity : flist;
            return flist;
        }).Where(x => !x.IsEnemy)
            .OrderBy(x => Vector3.Distance(x.Position, transform.position))
            .FirstOrDefault();

        return finalEntity;

    }
    
    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    private bool isInGrid = false;
    public bool onGrid
    {
        get
        {
            return isInGrid;
        }
        set
        {
            isInGrid = value;
        }
    }

    public event Action<IEntity> OnMove = delegate { };

    public void CalculatePathfinding(Node startingNode, Node goalNode)
    {
        currentPath = new List<Node>();
        StartCoroutine(ConstructPathAStar(startingNode, goalNode));
    }
    
    IEnumerator ConstructPathThetaStar(Node startingNode, Node goalNode)
    {
        Stopwatch stopwatch = new Stopwatch();
        float timeSlice = 1f / 60f;
        
        if (currentPath != null)
        {
            stopwatch.Start();
            currentPath.Reverse();
            int index = 0;

            while (index <= currentPath.Count - 1)
            {
                int indexNextNext = index + 2;
                if (indexNextNext > currentPath.Count - 1) break;
                if (InSight(currentPath[index].transform.position, currentPath[indexNextNext].transform.position))
                    currentPath.Remove(currentPath[index + 1]);
                else index++;
                if (stopwatch.ElapsedMilliseconds >= timeSlice)
                {
                    stopwatch.Restart();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        yield return null;
    }

    public bool InSight(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;

        return !Physics.Raycast(start, dir, dir.magnitude, NodeManager.instance.wallMask);
    }

    IEnumerator ConstructPathAStar(Node startingNode, Node goalNode)
    {
        Stopwatch stopwatch = new Stopwatch();
        float timeSlice = 1f / 60f;
        
        if (startingNode == null || goalNode == null)
            yield break;

        PriorityQueue frontier = new PriorityQueue();
        frontier.Put(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();

        cameFrom.Add(startingNode, null);
        costSoFar.Add(startingNode, 0);
        
        stopwatch.Start();
        while (frontier.Count() > 0)
        {
            Node current = frontier.Get();

            if (current == goalNode)
            {
                currentPath = new List<Node>();
                Node nodeToAdd = current;

                while (nodeToAdd != null)
                {
                    currentPath.Add(nodeToAdd);
                    nodeToAdd = cameFrom[nodeToAdd];
                }

                StartCoroutine(ConstructPathThetaStar(startingNode, goalNode));
                break;
            }

            
            foreach (var next in current.GetNeighbors())
            {
                int newCost = costSoFar[current] + next.cost;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    if (costSoFar.ContainsKey(next))
                    {
                        costSoFar[next] = newCost;
                        cameFrom[next] = current;
                    }
                    else
                    {
                        cameFrom.Add(next, current);
                        costSoFar.Add(next, newCost);
                    }

                    float priority = newCost + Heuristic(next.transform.position, goalNode.transform.position);
                    frontier.Put(next, priority);
                }
            }

            if (stopwatch.ElapsedMilliseconds >= timeSlice)
            {
                yield return null;
                stopwatch.Restart();
            }
        }
    }

    float Heuristic(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    public abstract void Attack();
}
