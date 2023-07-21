using UnityEngine;

public interface IEntity
{
    public Vector3 Position { get; }
    public float Health { get; set; }
    public GameObject myGameObject { get; }
    public bool IsEnemy { get; }
}
