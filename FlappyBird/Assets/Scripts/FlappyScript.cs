﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Spritesheet for Flappy Bird found here: http://www.spriters-resource.com/mobile_phone/flappybird/sheet/59537/
/// Audio for Flappy Bird found here: https://www.sounds-resource.com/mobile/flappybird/sound/5309/
/// </summary>
public class FlappyScript : MonoBehaviour
{
    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip;
    public Sprite GetReadySprite;
    public float RotateUpSpeed = 1, RotateDownSpeed = 1;
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3;
    public float XSpeed = 1;
    Vector3 birdRotation = Vector3.zero;

    FlappyYAxisTravelState flappyYAxisTravelState;

    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    // Update is called once per frame
    void Update()
    {
        // Handle back key in Windows Phone
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Use a switch statement to handle different game states
        switch (GameStateManager.GameState)
        {
            case GameState.Intro:
                HandleIntroState();
                break;

            case GameState.Playing:
                HandlePlayingState();
                break;

            case GameState.Dead:
                HandleDeadState();
                break;

            default:
                Debug.LogWarning("Unhandled GameState!");
                break;
        }
    }

    // Handle logic for the Intro state
    void HandleIntroState()
    {
        MoveBirdOnXAxis();

        if (WasTouchedOrClicked())
        {
            BoostOnYAxis();
            GameStateManager.GameState = GameState.Playing;
            IntroGUI.SetActive(false);
            ScoreManagerScript.Score = 0;
        }
    }

    // Handle logic for the Playing state
    void HandlePlayingState()
    {
        MoveBirdOnXAxis();

        if (WasTouchedOrClicked())
        {
            BoostOnYAxis();
        }
    }

    // Handle logic for the Dead state
    void HandleDeadState()
    {
        Vector2 contactPoint = Vector2.zero;

        if (Input.touchCount > 0)
            contactPoint = Input.touches[0].position;

        if (Input.GetMouseButtonDown(0))
            contactPoint = Input.mousePosition;

        // Check if user wants to restart the game
        if (restartButtonGameCollider == Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(contactPoint)))
        {
            GameStateManager.GameState = GameState.Intro;
            Application.LoadLevel(Application.loadedLevelName);
        }
    }

    void FixedUpdate()
    {
        // Just jump up and down on intro screen
        if (GameStateManager.GameState == GameState.Intro)
        {
            if (GetComponent<Rigidbody2D>().linearVelocity.y < -1) // When the speed drops, give a boost
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime)); // Lots of play and stop
                                                        // and play and stop etc to find this value, feel free to modify
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
        }
    }

    bool WasTouchedOrClicked()
    {
        if (Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
            return true;
        else
            return false;
    }

    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void BoostOnYAxis()
    {
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }

    /// <summary>
    /// when the flappy goes up, it'll rotate up to 45 degrees. when it falls, rotation will be -90 degrees min
    /// </summary>
    private void FixFlappyRotation()
    {
        if (GetComponent<Rigidbody2D>().linearVelocity.y > 0) flappyYAxisTravelState = FlappyYAxisTravelState.GoingUp;
        else flappyYAxisTravelState = FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case FlappyYAxisTravelState.GoingUp:
                degreesToAdd = 6 * RotateUpSpeed;
                break;
            case FlappyYAxisTravelState.GoingDown:
                degreesToAdd = -3 * RotateDownSpeed;
                break;
            default:
                break;
        }
        // Solution with negative eulerAngles found here: http://answers.unity3d.com/questions/445191/negative-eular-angles.html

        // Clamp the values so that -90<rotation<45 *always*
        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    /// <summary>
    /// check for collision with pipes
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Pipeblank") // Pipeblank is an empty gameobject with a collider between the two pipes
            {
                GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
                ScoreManagerScript.Score++;
            }
            else if (col.gameObject.tag == "Pipe")
            {
                FlappyDies();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Floor")
            {
                FlappyDies();
            }
        }
    }

    void FlappyDies()
    {
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
    }
}
