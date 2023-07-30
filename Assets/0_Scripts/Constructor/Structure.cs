using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Structure : MonoBehaviour
{
    List<MeshRenderer> myMeshes;

    private void Awake()
    {
        //Consigo las mesh para poder apagarlas mientras no esten construidas
        myMeshes = GetComponentsInChildren<MeshRenderer>().ToList();

        StructureManager.instance.availablesStructures.Add(this);

        GetComponent<BoxCollider>().enabled = false;

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = false;
        }
    }

    public void OnBuild()
    {
        StructureManager.instance.availablesStructures.Remove(this);
        StructureManager.instance.unavailablesStructures.Add(this);

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = true;
        }

        GetComponent<BoxCollider>().enabled = true;
    }

    public void OnDestroyed()
    {
        StructureManager.instance.availablesStructures.Add(this);
        StructureManager.instance.unavailablesStructures.Remove(this);

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = false;
        }

        GetComponent<BoxCollider>().enabled = false;
    }
}
