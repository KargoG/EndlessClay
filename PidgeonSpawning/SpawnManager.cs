using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component manages the spawning of pigeons.
/// Only one object in the scene should have this component.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private int _minAmountOfShotPigeons = 1;
    [SerializeField] private int _maxAmountOfShotPigeons = 3;

    [SerializeField] private float _minAmountOfTimeBetweenShots = 0.7f;
    [SerializeField] private float _maxAmountOfTimeBetweenShots = 1.4f;

    // The different pigeons that can be spawned
    [SerializeField] private GameObject[] _pigeonPres;

    // The pigeon shooter in the scene
    private PositionedPigeonShooter[] _positionedPigeonShooters = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _positionedPigeonShooters = FindObjectsOfType<PositionedPigeonShooter>();


        Invoke("ShootPigeonBatch", 1f);
    }

    void ShootPigeonBatch()
    {
        int amountOfShotPigeons = Random.Range(_minAmountOfShotPigeons, _maxAmountOfShotPigeons + 1);

        for (int i = 0; i < amountOfShotPigeons; i++)
        {
            _positionedPigeonShooters[Random.Range(0, _positionedPigeonShooters.Length)].Shoot(_pigeonPres[Random.Range(0, _pigeonPres.Length)]);
        }

        // Invoke is used instead of Invoke repeating due to the changing time between shots
        Invoke("ShootPigeonBatch", Random.Range(_minAmountOfTimeBetweenShots, _maxAmountOfTimeBetweenShots));
    }
}
