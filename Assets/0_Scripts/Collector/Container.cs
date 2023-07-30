using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public bool isEmpty;

    public GameObject myChild;

    private void Awake()
    {
        isEmpty = true;
        //Agrego los childs para despues poder apagarlos y prenderlos
        myChild = gameObject.transform.GetChild(0).gameObject;
    }


    public virtual void OnGetResource()
    {
        isEmpty = false;
        myChild.SetActive(true);
    }

    public virtual void OnTakenResource()
    {
        isEmpty = true;
        myChild.SetActive(false);
    }
}
