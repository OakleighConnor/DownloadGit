using UnityEngine;

public interface IDamageable
{
    void Damage();
}

public interface IPickupable
{
    void PickUp(GameObject holdPos);
    void Throw();
}