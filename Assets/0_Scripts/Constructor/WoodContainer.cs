using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodContainer : Container
{
    private void Start()
    {
        ContainerManager.instance.freeWoodContainers.Add(this);
    }

    public override void OnGetResource()
    {
        if (!isEmpty)
            return;

        ContainerManager.instance.takenWoodContainers.Add(this);
        ContainerManager.instance.freeWoodContainers.Remove(this);
        ResourcesManager.instance.AddResource(ResourceType.Wood);

        base.OnGetResource();
    }

    public override void OnTakenResource()
    {
        if (isEmpty)
            return;

        ContainerManager.instance.takenWoodContainers.Remove(this);
        ResourcesManager.instance.RemoveResource(ResourceType.Wood);
        ContainerManager.instance.freeWoodContainers.Add(this);

        base.OnTakenResource();
    }
}
