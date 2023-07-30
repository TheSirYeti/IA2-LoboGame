using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneContainer : Container
{
    void Start()
    {
        ContainerManager.instance.freeStoneContainers.Add(this);
    }

    public override void OnGetResource()
    {

        if (!isEmpty)
            return;

        ContainerManager.instance.takenStoneContainers.Add(this);
        ResourcesManager.instance.AddResource(ResourceType.Stone);
        ContainerManager.instance.freeStoneContainers.Remove(this);

        base.OnGetResource();

    }

    public override void OnTakenResource()
    {
      

        if (isEmpty)
            return;

        ContainerManager.instance.takenStoneContainers.Remove(this);
        ResourcesManager.instance.RemoveResource(ResourceType.Stone);
        ContainerManager.instance.freeStoneContainers.Add(this);

        base.OnTakenResource();

    }
}
