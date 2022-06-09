using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component spawns and shoots pigeons
/// </summary>
public class PositionedPigeonShooter : MonoBehaviour
{
    // The speed range for the pigeons
    [SerializeField] private float _minSpeed = 7.5f;
    [SerializeField] private float _maxSpeed = 20f;

    // The angle in which the direction of the shot pigeons can vary, to not always shoot pigeons the same way
    [SerializeField] private float _shootingConeAngle = -45;

    /// <summary>
    /// This method draws some Editor Gizmos
    /// </summary>
    void OnDrawGizmos()
    {
        Vector3 conePoint = transform.forward * 5;
        Vector3 lastPoint = Vector3.zero;
        conePoint = Quaternion.AngleAxis(_shootingConeAngle, transform.right) * conePoint;
        
        for (int i = 0; i < 8; i++)
        {
            Gizmos.DrawLine(transform.position, transform.position + conePoint);
            Gizmos.DrawLine(transform.position + lastPoint, transform.position + conePoint);
            lastPoint = conePoint;
            
            conePoint = Quaternion.AngleAxis(45, transform.forward) * conePoint;
        }

    }

    /// <summary>
    /// This method shoots a pigeon based on the defined min and max speed, as well
    /// as the defined shooting angle.
    /// </summary>
    /// <param name="pigeonPre">The prefab of the pigeon that should be shot.</param>
    public void Shoot(GameObject pigeonPre)
    {
        Vector3 spawnPoint = transform.position;

        // Calculating a random flying direction based on the current forward and the given angle for variety
        Vector3 wantedForward = transform.forward;
        wantedForward = Quaternion.AngleAxis(_shootingConeAngle, new Vector3(0, Random.Range(-1f, 1f), Random.Range(-1f, 1f))) * wantedForward;

        // Spawn the pigeon with the decided direction and a randomized speed
        GameObject newPigeon = Instantiate(pigeonPre, spawnPoint, Quaternion.LookRotation(wantedForward));
        Pigeon pigeon = newPigeon.GetComponent<Pigeon>();
        newPigeon.transform.localScale *= pigeon.SizeScale;
        newPigeon.GetComponent<Rigidbody>().velocity = newPigeon.transform.forward * Random.Range(pigeon.MinSpeed, pigeon.MaxSpeed);
    }
}
