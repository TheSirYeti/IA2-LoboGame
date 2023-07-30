using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] Text _woodText;
    [SerializeField] Text _stoneText;

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
        UpdateTexts();
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
        UpdateTexts();
    }

    void UpdateTexts()
    {
        _woodText.text = woodAmount.ToString();
        _stoneText.text = stoneAmount.ToString();
    }
}
