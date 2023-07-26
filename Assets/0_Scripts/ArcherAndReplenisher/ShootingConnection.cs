using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingConnection : MonoBehaviour
{
    [SerializeField] Archer _a;
    public void AnimEventShoot()
    {
        _a.Shooting();
    }
}
