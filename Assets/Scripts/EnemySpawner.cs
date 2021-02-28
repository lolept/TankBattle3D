using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemySpawner : MonoBehaviour
{

    public GameObject Enemy;
    public int Place;
    public int Count;
    public float TimeToSpawn;
    void Start()
    {
        TimeToSpawn = 60f;
        Count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        TimeToSpawn -= Time.deltaTime;
        if (Count == 0 || TimeToSpawn <= 0)
        {
            Place = UnityEngine.Random.Range(0, 4);
            Spawn(Place);
        }
    }

    public void Spawn(int a)
    {
        switch (a)
        {
            case 0:
                Instantiate(Enemy, new Vector3(10.98f, 0, 10.7f), Quaternion.identity);
                Count++;
                TimeToSpawn = 60f;
                break;
            case 1:
                Instantiate(Enemy, new Vector3(480.7f, 0, 481.32f), Quaternion.identity);
                Count++;
                TimeToSpawn = 60f;
                break;
            case 2:
                Instantiate(Enemy, new Vector3(10.98f, 0, 481.32f), Quaternion.identity);
                Count++;
                TimeToSpawn = 60f;
                break;
            case 3:
                Instantiate(Enemy, new Vector3(480.7f, 0, 10.7f), Quaternion.identity);
                Count++;
                TimeToSpawn = 60f;
                break;
        }
    }
}
