using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone
}

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager instance;

    public int woodAmount;
    public int stoneAmount;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void AddResource(ResourceType myType)
    {
        switch (myType)
        {
            case ResourceType.Wood:
                woodAmount++;
                break;
            case ResourceType.Stone:
                stoneAmount++;
                break;
            default:
                break;
        }
    }

    public void RemoveResource(ResourceType myType)
    {
        switch (myType)
        {
            case ResourceType.Wood:
                woodAmount--;
                break;
            case ResourceType.Stone:
                stoneAmount--;
                break;
            default:
                break;
        }
    }
}
