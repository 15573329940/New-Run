using UnityEngine;

public interface IPool
{
    void SpawnObject();
    void SpawnObject(Transform user);
    void RecycleObject();
}
