using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public ResourceType typeOfResource;

    public float respawnTime;

    public MeshRenderer mesh;

    public Vector3 myPosition
    {
        get
        {
            return transform.position;
        }
    }

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            OnLooted();
    }

    public virtual void OnLooted()
    {
        mesh.enabled = false;
        Invoke("OnRespawn", respawnTime);
    }

    public virtual void OnRespawn()
    {
        mesh.enabled = true;
    }
}
