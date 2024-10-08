using Ink.Parsed;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MicrowaveMinigame : MonoBehaviour
{
    private bool GameStarted;
    private bool GamePaused;
    Rigidbody2D Wave;
    [SerializeField] float speed;
    [SerializeField] Rigidbody2D WaveShooter;
    public const string WaveBounceSound = "wave_bounce";
    public const string WaveTeleportSound = "wave_teleport";
    public const string WaveLoseSound = "wave_lose";
    public const string WaveWinSound = "wave_win";
    public const string WaveShootSound = "wave_shoot";
    public const string GunRotateSound = "gun_rotate";
    public const string GunLockSound = "gun_lock";
    public const string PlatformSelectSound = "platform_select";
    public const string PlatformPlaceSound = "platform_place";

    private bool PlayingRotateSound;

    private AudioManager audioMan;
    // Start is called before the first frame update
    void Start()
    {
        audioMan = FindObjectOfType<AudioManager>();
        Wave = GetComponent<Rigidbody2D>();
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !GameStarted)
        {
            PlaySound(WaveShootSound);
            StartGame();
        }
        else if (Input.GetKeyDown(KeyCode.R) && GameStarted)
        {
            ResetGame();
        }

        if (!GameStarted)
        {
            float RotationSpeed = 50;
            float angle = Mathf.Atan2(TowardsMouse(WaveShooter.position).y, TowardsMouse(WaveShooter.position).x) * Mathf.Rad2Deg;
            bool tempbool = false;
            if (angle > (WaveShooter.rotation) + 0.5f && WaveShooter.rotation < 90)
            {
                tempbool = true;
                WaveShooter.SetRotation(WaveShooter.rotation + (RotationSpeed * Time.deltaTime));
            }
            else if (angle < (WaveShooter.rotation) - 0.5f && WaveShooter.rotation > -90)
            {
                tempbool = true;
                WaveShooter.SetRotation(WaveShooter.rotation - (RotationSpeed * Time.deltaTime));
            }
            if (tempbool != PlayingRotateSound)
            {
                if (tempbool)
                {
                    PlaySingleSound(GunRotateSound);
                }
                else
                {
                    try
                    {
                        audioMan.PausePlayingSFX(GunRotateSound);
                    }    
                    catch
                    {
                        //nothin
                    }
                }
                PlayingRotateSound = tempbool;
            }
            Wave.SetRotation(WaveShooter.rotation + 90 + 180);
            Wave.position = new Vector2(-4.62f + Mathf.Cos(Mathf.Deg2Rad * WaveShooter.rotation) * 0.6528f, 1.8f + Mathf.Sin(Mathf.Deg2Rad * WaveShooter.rotation) * 0.6528f);
        }
    }

    public bool IsGameStarted()
    {
        return GameStarted;
    }
    private void StartGame()
    {
        GameStarted = true;
        //Wave.AddForce(TowardsMouse(Wave.position) * speed, ForceMode2D.Impulse);
        //Wave.AddForce(speed * new Vector2(WaveShooter.rotation.y, WaveShooter.position.x), ForceMode2D.Impulse);

        float ForceMagnitude = 10;

        Wave.AddForce(new Vector2(Mathf.Cos(WaveShooter.rotation * Mathf.Deg2Rad), Mathf.Sin(WaveShooter.rotation * Mathf.Deg2Rad)) * ForceMagnitude, ForceMode2D.Impulse);
        //Wave.SetRotation(WaveShooter.rotation + 180 + 90);

    }
    private void ResetGame()
    {
        GameStarted = false;
        Wave.position = new Vector2(-4f, 1.8f);
        Wave.SetRotation(0);
        Wave.velocity = new Vector2(0, 0);
        Wave.SetRotation(WaveShooter.rotation + 90 + 180);
        Wave.position = new Vector2(-4.62f + Mathf.Cos(Mathf.Deg2Rad * WaveShooter.rotation) * 0.6528f, 1.8f + Mathf.Sin(Mathf.Deg2Rad * WaveShooter.rotation) * 0.6528f);
    }
    private Vector2 TowardsMouse(Vector2 ObjectPosition)
    {
        // Get the mouse position in screen coordinates
        Vector2 MouseScreenPosition = Input.mousePosition;

        // Convert mouse position from screen coordinates to world coordinates
        Vector2 MouseWorldPosition = Camera.main.ScreenToWorldPoint(MouseScreenPosition);

        // Calculate the direction vector from the object to the mouse
        Vector2 Direction = MouseWorldPosition - ObjectPosition;

        // Optional: Normalize the direction vector to get a unit vector
        Vector2 NormalizedDirection = Direction.normalized;

        // Debugging: Draw a line in the scene view to visualize the direction
        Debug.DrawLine(ObjectPosition, MouseWorldPosition, Color.red);

        // Use the direction vector for something (e.g., move towards the mouse position)
        return NormalizedDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("pink reflector") || collision.gameObject.tag.Equals("blue reflector"))
        {
            PlaySound(WaveTeleportSound);
            return;
        }

        Debug.Log("collider " + collision.tag);
        if (collision.tag.Equals("goal") && GameStarted)
        {
            PlaySound(WaveWinSound);
            WinLevel();
        }
        if (collision.tag.Equals("death") && GameStarted)
        {
            PlaySound(WaveLoseSound);
            Debug.Log("death");
            ResetGame();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlaySound(WaveBounceSound);
        SetRotation();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        SetRotation();
    }

    private void SetRotation()
    {
        //float angle = Mathf.Atan2(TowardsMouse(WaveShooter.position).y, TowardsMouse(WaveShooter.position).x) * Mathf.Rad2Deg;
        try
        {
            float angle = WaveShooter.rotation;
            angle = Mathf.Atan2(Wave.velocity.y, Wave.velocity.x) * Mathf.Rad2Deg;
            //Debug.Log(angle * Mathf.Rad2Deg);

            Wave.SetRotation(angle + 90f);
        }
        catch
        {

        }
    }

    private void WinLevel()
    {
        FindObjectOfType<ChangeToScene>().ChangeScene();
    }

    public void PlaySound(string s)
    {
        try
        {
            audioMan.PlayMultipleSFX(s);
        }
        catch
        {

        }
    }
    public void PlaySingleSound(string s)
    {
        try
        {
            audioMan.PlaySFX(s);
        }
        catch
        {

        }
    }
}

