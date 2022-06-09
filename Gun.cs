using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// This component manages all behaviour related to the gun.
/// </summary>
public class Gun : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _screenCenter;
    private ScoreHandler _scoreHandler;
    private TrackedPoseDriver _poseDriver;

    // A prefab for an effect played when shooting
    [SerializeField] private GameObject _shotEffectPre;

    // A transform defining where the shot effect should be instantiated
    [SerializeField] private Transform _effectAnchor;

    // The time between shots
    [SerializeField] private float _waitingTime = 0.2f;

    // The size of a bullet
    [SerializeField] private float _shotRadius = 0.5f;

    [SerializeField] Sound _gunShotSound;
    [SerializeField] private GameObject _addedScorePre = null;

    private float _timeOfLastShot;
    private float _scoreMult = 1f;
    // The time limit between shots for the multiplier to reset
    private float _timeLimitMult = 4f;
    // the elapsed time since the last shot
    private float _timerMult = 0f;

    private Vector3 _lastCamPos;
    private Vector3 _lastCamForward;


    [SerializeField] private GameObject _crosshair = null;
    // The cameras rotation speed needed for the maximal crosshair offset
    [SerializeField] private float _rotationSpeedForOffset = 10;
    [SerializeField] private float _minRotationSpeedForOffset = 10;
    // The maximal crosshair offset when moving
    [SerializeField] private float _maxOffset = 0.1f;

    void Start()
    {
        _scoreHandler = FindObjectOfType<ScoreHandler>();
        _camera = Camera.main;
        _screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        _timerMult = 0;
        _poseDriver = FindObjectOfType<TrackedPoseDriver>();
        _lastCamPos = Camera.main.transform.position;
        _lastCamForward = Camera.main.transform.forward;
    }

#if UNITY_EDITOR

    [SerializeField] private float _maxPreviewDistance = 10f;
    [SerializeField] private float _timeForPreview = 10f;
    private float _accTime = 0;

    /// <summary>
    /// This method draws some editor gizmos for easier debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        _accTime += Time.deltaTime;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Camera.main.transform.position + Camera.main.transform.forward * _maxPreviewDistance * (((_accTime % _timeForPreview) + 1) / _timeForPreview), _shotRadius);
    }
#endif

    private void Update()
    {
        if (_scoreMult > 1f)
            _timerMult += Time.deltaTime;

        // if too much time passed since the last shot we reset the multiplier
        if(_timerMult >= _timeLimitMult)
        {
            _timerMult = 0f;
            _scoreMult = 1f;
            _scoreHandler.UpdateMult(_scoreMult);
        }

        UpdateCrosshair();
    }

    /// <summary>
    /// This method adjusts the crosshair position, making it sway a bit
    /// when moving the camera/ the gun.
    /// </summary>
    private void UpdateCrosshair()
    {
        Vector3 currentPos = Camera.main.transform.position;
        Vector3 currentForward = Camera.main.transform.forward;

        // The rotation of the camera is calculated based on the camera position of the current and the previous frame
        float rotationDelta = Vector3.Angle(_lastCamForward, currentForward);
        Vector3 rotationVector = Vector3.Cross(_lastCamForward, currentForward);
        Vector3 offsetVector = new Vector3(rotationVector.y, -rotationVector.x, 0);

        float rotationSpeed = rotationDelta / Time.deltaTime;
        rotationSpeed = Mathf.Clamp(rotationSpeed, 0, _rotationSpeedForOffset);

        // we calculate the desired crosshair offset based on the calculated rotation speed and the defined max values
        float wantedOffset = ((rotationSpeed / _rotationSpeedForOffset)) * _maxOffset;
        if (rotationSpeed < _minRotationSpeedForOffset)
            wantedOffset = 0;

        _crosshair.transform.position = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f) + -offsetVector.normalized * wantedOffset);

        _lastCamPos = currentPos;
        _lastCamForward = currentForward;
    }

    public void Shoot()
    {
        // Check if we are still on cooldown
        if (Time.time < _timeOfLastShot + _waitingTime)
            return;

        Instantiate(_shotEffectPre, _effectAnchor.position, _effectAnchor.rotation);

        // Checking for hit pigeons with a spherecast
        Ray shot = _camera.ScreenPointToRay(_screenCenter);
        RaycastHit hit;
        if (Physics.SphereCast(shot, _shotRadius, out hit))
        {
            if (hit.collider.CompareTag("Pigeon"))
            {
                // on hit we update the score
                _scoreHandler.AddPoints((int)(hit.collider.GetComponent<Pigeon>().PointWorth * _scoreMult));
                TextMeshPro scoreText = Instantiate(_addedScorePre, hit.collider.transform.position, Camera.main.transform.rotation).GetComponent<TextMeshPro>();
                scoreText.SetText("" + hit.collider.GetComponent<Pigeon>().PointWorth * _scoreMult);

                // The hit pigeon gets destroyed
                Pigeon pigeon = hit.transform.gameObject.GetComponent<Pigeon>();
                pigeon.Destroy();

                // The multiplayer for shots hit in succession gets updated
                if(_scoreMult < 2f)
                {
                    _scoreMult += 0.05f;
                    _scoreHandler.UpdateMult(_scoreMult);
                }
                _timerMult = 0f;
            }
            else
            {
                // if we don't hit anything the multiplier gets reset
                _scoreMult = 1f;
                _scoreHandler.UpdateMult(_scoreMult);
            }
        }
        else
        {
            // if we don't hit anything the multiplier gets reset
            _scoreMult = 1f;
            _scoreHandler.UpdateMult(_scoreMult);
        }

        _timeOfLastShot = Time.time;
        _gunShotSound.Play();
    }
}
