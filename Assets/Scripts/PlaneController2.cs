using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CriticalComponent
{
    public enum ComponentType
    {
        engine,
        fueltank,
        pilot,
    }
    public ComponentType type;
    public Vector3 position;
    public Vector3 size;
}


public class PlaneController2 : MonoBehaviour
{
    private Rigidbody Rigidbody;
    public float maxThrust = 200.0f;
    public float airbrakeDrag;
    public float flapsDrag;
    [SerializeField]
    float gLimit;
    [SerializeField]
    float gLimitPitch;
    [Header("Lift")]
    [SerializeField]
    private AnimationCurve liftAOACurve;
    [SerializeField]
    private float liftPower = 10.0f;
    [SerializeField]
    private AnimationCurve inducedDragCurve;
    [SerializeField]
    float rudderPower;
    [SerializeField]
    AnimationCurve rudderAOACurve;
    [SerializeField]
    AnimationCurve rudderinducedDragCurve;
    [SerializeField]
    private float inducedDragPower = 10.0f;
    [SerializeField]
    private float flapsLiftPower = 1.0f;
    [SerializeField]
    private float flapsAOABias = 0.0f;

    [Header("Drag")]
    [SerializeField]
    private AnimationCurve dragForward;
    [SerializeField]
    private AnimationCurve dragBack;
    [SerializeField]
    private AnimationCurve dragLeft;
    [SerializeField]
    private AnimationCurve dragRight;
    [SerializeField]
    private AnimationCurve dragTop;
    [SerializeField]
    private AnimationCurve dragBottom;

    [Header("Steering")]
    [SerializeField]
    private Vector3 turnSpeed;
    [SerializeField]
    private Vector3 turnAcceleration;
    [SerializeField]
    private AnimationCurve steeringCurve;

    [Header("Misc")]
    [SerializeField]
    private TMPro.TextMeshProUGUI hud;
    [SerializeField]
    private Animation engineOnAnimation;
    [SerializeField]
    private CriticalComponent[] criticalComponents;

    private void Awake()
    {
       Rigidbody = GetComponent<Rigidbody>();
    }

    private Vector3 Velocity;
    private Vector3 lastVelocity;
    private Vector3 LocalVelocity;
    private Vector3 LocalAngularVelocity;
    private Vector3 LocalGForce;
    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    private bool AirbrakeDeployed;
    private bool FlapsDeployed;
    private float AngleOfAttack;
    private float AngleOfAttackYaw;
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        HandleInputs();
        CalculateState(dt);
        CalculateAngleOfAttack();
        CalculateGForce(dt);
        
        UpdateThrust();
        UpdateSteering(dt);
        UpdateDrag();
        UpdateLift();
        UpdateHUD();
    }

    private void HandleInputs()
    {
        // Get axis
        roll = (Input.GetKey("a") ? 1 : 0) - (Input.GetKey("d") ? 1 : 0);
        pitch = (Input.GetKey("w") ? 1 : 0) - (Input.GetKey("s") ? 1 : 0);
        yaw = (Input.GetKey("e") ? 1 : 0) - (Input.GetKey("q") ? 1 : 0);
        var throttleChange = (Input.GetKey(KeyCode.LeftShift) ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0);


        // Handle throttle value and clamp it to a value between 0% and 100%
        if (throttle < 100f)
        {
            throttle += throttleChange;
            throttle = Mathf.Clamp(throttle, 0f, 100f);
        }
        else
        {
            throttle = 100f;
            if (throttleChange == -1) throttle += throttleChange;
            if (throttleChange == 1) throttle = 110f;
        }
    }

    void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        Velocity = Rigidbody.velocity;
        LocalVelocity = invRotation * Velocity; // transform world velocity to local rotation
        LocalAngularVelocity = invRotation * Rigidbody.angularVelocity;
    }

    void CalculateAngleOfAttack()
    {
        if (LocalVelocity.sqrMagnitude < 0.1f)
        {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    void CalculateGForce(float dt)
    {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        var acceleration = (Velocity - lastVelocity) / dt;
        LocalGForce = invRotation * acceleration;
        lastVelocity = Velocity;
    }

    Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity)
    {
        return Vector3.Cross(angularVelocity, velocity);
    }

    Vector3 CalculateGForceLimit(Vector3 input)
    {
        return Scale6(input,
            gLimit, gLimitPitch,
            gLimit, gLimit,
            gLimit, gLimit
        ) * 9.81f;
    }

    float CalculateGLimiter(Vector3 controlInput, Vector3 maxAngularVelocity)
    {
        if (controlInput.magnitude < 0.01f)
        {
            return 1;
        }

        //if the player gives input with magnitude less than 1, scale up their input so that magnitude == 1
        var maxInput = controlInput.normalized;

        var limit = CalculateGForceLimit(maxInput);
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), LocalVelocity);

        if (maxGForce.magnitude > limit.magnitude)
        {
            //example:
            //maxGForce = 16G, limit = 8G
            //so this is 8 / 16 or 0.5
            return limit.magnitude / maxGForce.magnitude;
        }

        return 1;
    }

    Vector3 CalculateLift(float aoa, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve idcurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);
        var v2 = liftVelocity.sqrMagnitude;

        // lift = velocity^2 * coefficient * liftPower
        var liftCoefficient = aoaCurve.Evaluate(aoa * Mathf.Rad2Deg);
        var liftForce = v2 * liftCoefficient * liftPower;

        // lift is always perpendicular to velocity
        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        // induced drag varies with square of lift coefficient
        var dragForce = liftCoefficient * liftCoefficient * this.inducedDragPower * idcurve.Evaluate(Mathf.Max(0, LocalVelocity.z));
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce;

        return lift + inducedDrag;

    }

    float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration)
    {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }

    void UpdateLift()
    {
        if (LocalVelocity.sqrMagnitude < 1f) return;

        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;

        var liftForce = CalculateLift(AngleOfAttack + (flapsAOABias * Mathf.Deg2Rad), Vector3.right, liftPower + flapsLiftPower, liftAOACurve, inducedDragCurve);

        var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderinducedDragCurve);

        Rigidbody.AddRelativeForce(liftForce);
        Rigidbody.AddRelativeForce(yawForce);
    }

    void UpdateThrust()
    {
        Rigidbody.AddRelativeForce(throttle * maxThrust * Vector3.forward);
        engineOnAnimation.Play("spin");
    }

    void UpdateDrag()
    {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude;

        float airbrakeDrag = AirbrakeDeployed ? this.airbrakeDrag : 0;
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0;

        // calculate coefficient of drag depending on direction
        var coefficiant = Scale6(
        lv.normalized,
        dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)),
        dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)),
        dragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag + flapsDrag, dragBack.Evaluate(Mathf.Abs(lv.z))
            );

        var drag = coefficiant.magnitude * lv2 * -lv.normalized;

        Rigidbody.AddRelativeForce(drag);
    }

    void UpdateSteering(float dt)
    {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed);
        var controlInput = new Vector3(pitch, yaw, roll);

        var gForceScaling = CalculateGLimiter(controlInput, turnSpeed * Mathf.Deg2Rad * steeringPower);

        var targetAV = Vector3.Scale(controlInput, turnSpeed * steeringPower * gForceScaling);
        var av = LocalAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(
            CalculateSteering(dt, av.x, targetAV.x, turnAcceleration.x * steeringPower),
            CalculateSteering(dt, av.y, targetAV.y, turnAcceleration.y * steeringPower),
            CalculateSteering(dt, av.z, targetAV.z, turnAcceleration.z * steeringPower)
        );

        Rigidbody.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);
    }

    private void UpdateHUD()
    {

        hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        hud.text += "IAS: " + (Rigidbody.velocity.magnitude).ToString("F0") + "mph\n";
        hud.text += "Altitude: " + (transform.position.y).ToString("F0") + "m\n";
        hud.text += "G: " + ((CalculateGForce(LocalAngularVelocity, LocalVelocity).magnitude / 9.80665) + 1).ToString("F0") + "G\n";
    }

    //similar to Vector3.Scale, but has separate factor negative values on each axis
    static Vector3 Scale6(
        Vector3 value,
        float posX, float negX,
        float posY, float negY,
        float posZ, float negZ
    )
    {
        Vector3 result = value;

        if (result.x > 0)
        {
            result.x *= posX;
        }
        else if (result.x < 0)
        {
            result.x *= negX;
        }

        if (result.y > 0)
        {
            result.y *= posY;
        }
        else if (result.y < 0)
        {
            result.y *= negY;
        }

        if (result.z > 0)
        {
            result.z *= posZ;
        }
        else if (result.z < 0)
        {
            result.z *= negZ;
        }

        return result;
    }
}
