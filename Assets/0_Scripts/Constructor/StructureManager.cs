using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public static StructureManager instance;

    public List<Structure> availablesStructures;
    public List<Structure> unavailablesStructures;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

}
