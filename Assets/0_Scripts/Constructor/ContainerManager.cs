using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ContainerManager : MonoBehaviour
{
    public static ContainerManager instance;

    public List<WoodContainer> freeWoodContainers;
    public List<StoneContainer> freeStoneContainers;

    public List<StoneContainer> takenStoneContainers;
    public List<WoodContainer> takenWoodContainers;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
}
