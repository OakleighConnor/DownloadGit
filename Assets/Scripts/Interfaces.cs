using UnityEngine;

public interface IDamageable
{
    void Damage();
}

public interface IThrowable
{
    GameObject PickUp();
    void Throw();
}