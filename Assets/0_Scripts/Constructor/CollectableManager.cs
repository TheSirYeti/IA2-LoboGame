using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager instance;

    public List<Collectable> availablesTrees;
    public List<Collectable> availablesStones;

    public List<Collectable> unavailablesTrees;
    public List<Collectable> unavailablesStones;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
}
