using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Plane Stats")]
    [Tooltip("How much the throttle ramps up or down")]
    public float throttleIncrement = 0.1f;
    [Tooltip("Maximum engine thrust at 100% throttle")]
    public float maxThrust = 200f;
    [Tooltip("How responsive the plane is")]
    public float responsiveness = 10f;
    [Tooltip("Wing area of the plane, in metres squared")]
    public float wingArea = 22.6f;
    [Tooltip("Air density multipler")]
    public float airDensity = 0.001f;
    [Tooltip("How the plane reacts when throttled above 100%")]
    public overThrottleReaction throttleReaction = overThrottleReaction.wep;
    public enum overThrottleReaction
    {
        wep,
        overtenpercent,
    }

    private float throttle; // Throttle percentage
    private float roll;
    private float pitch;
    private float yaw;

    // Value to tweak responsiveness to suit plane's weight
    private float responseModifier
    {
        get
        {
            return (rb.mass / 10f) * ((800 - rb.velocity.magnitude) / 100) / 5 * responsiveness;
        }
    }

    // Lift
    private float lift
    {
        get
        {
            return (airDensityMultiplier * rb.velocity.magnitude) / 2 * wingArea / 1000;
        }
    }

    // Value to tweak speed based on air density
    private float airDensityMultiplier
    {
        get
        {
            if (transform.position.y < 7000)
            {
                return ((1/airDensity + transform.position.y) * airDensity);
            }
            else
            {
                return 8;
            }
        }
    }

    Rigidbody rb;
    [SerializeField] TMPro.TextMeshProUGUI hud;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInputs()
    {
        // Get axis
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");


        // Handle throttle value and clamp it to a value between 0% and 100%
        if (throttle < 100f)
        {
            if (Input.GetKey(KeyCode.Space)) throttle += throttleIncrement;
            else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrement;
            throttle = Mathf.Clamp(throttle, 0f, 100f);
        } else
        {
            throttle = 100f;
            if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrement;
            if (Input.GetKey(KeyCode.Space)) throttle = 110f;
        }
    }

    private void Update()
    {
        HandleInputs();
        UpdateHUD();
    }

    private void FixedUpdate()
    {
        // Apply forces
        rb.AddForce(-transform.forward * maxThrust * throttle * airDensityMultiplier);
        rb.AddTorque(transform.up * (yaw * 10) * responseModifier);
        rb.AddTorque(transform.right * (pitch * 5) * responseModifier);
        rb.AddTorque(transform.forward * (roll * 5) * responseModifier);

        rb.AddForce(transform.up * rb.velocity.magnitude * lift);
    }

    private void UpdateHUD()
    {
        if (throttle <= 100f) {
            hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        } else
        {
            if (throttleReaction == overThrottleReaction.wep)
            {
                hud.text = "Throttle: <color=#ff0000ff>WEP</color>\n";
            } else
            {
                hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
            }
        }
        hud.text += "IAS: " + (rb.velocity.magnitude).ToString("F0") + "mph\n";
        hud.text += "Altitude: " + (transform.position.y).ToString("F0") + "m";
    }
}
