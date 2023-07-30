using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Collectable
{
    private void Start()
    {
        CollectableManager.instance.availablesStones.Add(this);
    }

    public override void OnLooted()
    {
        base.OnLooted();

        if (!CollectableManager.instance.unavailablesStones.Contains(this))
        {
            CollectableManager.instance.availablesStones.Remove(this);
            CollectableManager.instance.unavailablesStones.Add(this);
        }
    }

    public override void OnRespawn()
    {
        base.OnRespawn();

        if (!CollectableManager.instance.availablesStones.Contains(this))
        {
            CollectableManager.instance.availablesStones.Add(this);
            CollectableManager.instance.unavailablesStones.Remove(this);
        }
    }
}
