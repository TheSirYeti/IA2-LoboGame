using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class Node : MonoBehaviour
{
    public float viewRadius;
    public List<Node> neighbors = new List<Node>();
    public bool shouldDebug;

    public int cost = 1;

    private void Start()
    {
        ResetNeighbours(null);
    }

    public void ResetNeighbours(object[] parameters)
    {
        neighbors.Clear();
        
        if(shouldDebug)
            Debug.Log("Debug activado. ");
        
        foreach (Node node in NodeManager.instance.nodes)
        {
            if(shouldDebug)
                Debug.Log("Checking " + node.gameObject.name);
            
            if (Vector3.Distance(node.transform.position, transform.position) <= viewRadius && node != this)
            {
                if(shouldDebug)
                    Debug.Log("Looking at " + node.gameObject.name);
                
                Vector3 dir = node.transform.position - transform.position;
                if (!Physics.Raycast(transform.position, dir, dir.magnitude, NodeManager.instance.wallMask))
                {
                    if(shouldDebug)
                        Debug.Log("Added " + node.gameObject.name);
                    neighbors.Add(node);
                }
            }
        }
    }
    
    public List<Node> GetNeighbors()
    {
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);

        foreach (var node in neighbors)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}