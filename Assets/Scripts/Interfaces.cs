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
public interface ICollectable
{
    void Collect(PlayerScoreManager psm);
}