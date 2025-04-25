using UnityEngine;

public interface IDamageable
{
    void Damage();
}

public interface IPickupable
{
    GameObject PickUp();
    void Throw();
}