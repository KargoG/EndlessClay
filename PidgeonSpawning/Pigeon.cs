using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// A component handling all relevant behaviour for a pigeon
/// </summary>
public class Pigeon : MonoBehaviour
{
    [SerializeField] private int _pointWorth = 100;
    [SerializeField] private Vector2 _minMaxSpeed = new Vector2(50f, 70f);
    [SerializeField] private float _sizeScale = 1f;

    public float SizeScale
    {
        get { return _sizeScale; }
    }
    public float MinSpeed
    {
        get { return _minMaxSpeed.x; }
    }
    public float MaxSpeed
    {
        get { return _minMaxSpeed.y; }
    }
    public int PointWorth { get{ return _pointWorth; } }

	// Prefab for an indicator to show the pigeons position if it is outside the view
    [SerializeField] private GameObject _positionIndicatorPre = null;
	
	// Instance of an indicator to show the pigeons position if it is outside the view
    private GameObject _positionIndicatorInstance = null;

    [SerializeField] private GameObject _destructionEffectPre = null;

    private GameObject _UI = null;

    [SerializeField]
    private Sound _clip;

    void Start()
    {
        _UI = FindObjectOfType<Canvas>().gameObject;
    }


    void Update()
    {
		// if the pigeon moves too far below the player it gets removed
        if (transform.position.y < -15)
        {
            Destroy(gameObject);
            if (_positionIndicatorInstance)
                Destroy(_positionIndicatorInstance);
            return;
        }

        HandleIndicator();
    }

	/// <summary>
    /// This method updates the indicator that shows a pigeons position if it is outside the players view.
    /// </summary>
    void HandleIndicator()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        // Remap viewport to -1 to 1 scale
        viewportPos.x = (viewportPos.x - 0.5f) * 2f;
        viewportPos.y = (viewportPos.y - 0.5f) * 2f;

        // get the pigeons position in relation to the viewport
        bool aboveScreen = viewportPos.y > 1;
        bool belowScreen = viewportPos.y < -1;
        bool rightOffScreen = viewportPos.x > 1;
        bool leftOffScreen = viewportPos.x < -1;

        if (aboveScreen || belowScreen || rightOffScreen || leftOffScreen)
        {
            float rotation = 0;
            if (_positionIndicatorInstance == null)
            {
                _positionIndicatorInstance = Instantiate(_positionIndicatorPre, _UI.transform);
            }
            
            // The position and rotation of the indicator is set based on its position relative to the viewport
            if (aboveScreen)
            {
                if (rightOffScreen)
                {
                    if (viewportPos.y > viewportPos.x)
                    {
                        MapToY(ref viewportPos, false, false);
                        rotation = 180;
                    }
                    else
                    {
                        MapToX(ref viewportPos, false, false);
                        rotation = 90;
                    }
                }
                else if (leftOffScreen)
                {
                    if (viewportPos.y > -viewportPos.x)
                    {
                        MapToY(ref viewportPos, true, false);
                        rotation = 180;
                    }
                    else
                    {
                        MapToX(ref viewportPos, true, false);
                        rotation = 270;
                    }
                }
            }
            else if(belowScreen)
            {
                if (rightOffScreen)
                {
                    if (viewportPos.y < -viewportPos.x)
                    {
                        MapToY(ref viewportPos, false, true);
                    }
                    else
                    {
                        MapToX(ref viewportPos, false, true);
                        rotation = 90;
                    }
                }
                else if (leftOffScreen)
                {
                    if (viewportPos.y < viewportPos.x)
                    {
                        MapToY(ref viewportPos, true, true);
                    }
                    else
                    {
                        MapToX(ref viewportPos, true, true);
                        rotation = 270;
                    }
                }
            }
            else if (rightOffScreen)
            {
                MapToX(ref viewportPos, false, viewportPos.y < 0);
                rotation = 90;
            }
            else
            {
                MapToX(ref viewportPos, true, viewportPos.y < 0);
                rotation = 270;
            }

            // map to 0 - 1
            viewportPos.x = (viewportPos.x + 1) / 2;
            viewportPos.y = (viewportPos.y + 1) / 2;

            _positionIndicatorInstance.transform.position = Camera.main.ViewportToScreenPoint(viewportPos);
            _positionIndicatorInstance.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        else
        {
            // if the pigeon is inside the viewport we dont need the indicator
            if(_positionIndicatorInstance)
                Destroy(_positionIndicatorInstance);
        }
    }

    /// <summary>
    /// This method rezises a vector to a point where the x coordinate get set to 1 or -1. 
    /// </summary>
    /// <param name="vectorToMap">The vector to resize</param>
    /// <param name="reverseX">Should x be inversed?</param>
    /// <param name="reverseY">Should y be inversed?</param>
    void MapToX(ref Vector3 vectorToMap, bool reverseX, bool reverseY)
    {
        vectorToMap.y = ((vectorToMap.y * (reverseX ? -1 : 1)) / (vectorToMap.x * (reverseY ? -1 : 1))) * (reverseY ? -1 : 1);
        vectorToMap.x = reverseX ? -1 : 1;
    }

    /// <summary>
    /// This method rezises a vector to a point where the y coordinate get set to 1 or -1. 
    /// </summary>
    /// <param name="vectorToMap">The vector to resize</param>
    /// <param name="reverseX">Should x be inversed?</param>
    /// <param name="reverseY">Should y be inversed?</param>
    void MapToY(ref Vector3 vectorToMap, bool reverseX, bool reverseY)
    {
        vectorToMap.x = ((vectorToMap.x * (reverseX ? -1 : 1)) / (vectorToMap.y * (reverseY ? -1 : 1))) * (reverseX ? -1 : 1);
        vectorToMap.y = reverseY ? -1 : 1;
    }

    private void PlayHitSound()
    {
        _clip.Play();
    }

    public void Destroy()
    {
        PlayHitSound();
        Instantiate(_destructionEffectPre, transform.position, Quaternion.identity);
        Destroy(gameObject);
        if (_positionIndicatorInstance)
            Destroy(_positionIndicatorInstance);
    }
}
