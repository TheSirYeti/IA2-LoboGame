using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Collectable
{
    private void Start()
    {
        CollectableManager.instance.availablesTrees.Add(this);
    }

    public override void OnLooted()
    {
        base.OnLooted();

        if (!CollectableManager.instance.unavailablesTrees.Contains(this))
        {
            CollectableManager.instance.availablesTrees.Remove(this);
            CollectableManager.instance.unavailablesTrees.Add(this);
        }
    }

    public override void OnRespawn()
    {
        base.OnRespawn();
        if (!CollectableManager.instance.availablesTrees.Contains(this))
        {
            CollectableManager.instance.availablesTrees.Add(this);
            CollectableManager.instance.unavailablesTrees.Remove(this);
        }
    }
}
