using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeckoController : MonoBehaviour
{
    /// <summary>
    /// Tracking target
    /// </summary>
    [SerializeField] Transform target;
    #region Head
    [Header("Head Tracking")]
    /// <summary>
    /// Gecko's neck
    /// </summary>
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;
    #endregion

    #region Eyes
    [Header("Eyes Tracking")]
    [SerializeField] Transform leftEyeBone;
    [SerializeField] Transform rightEyeBone;
    [SerializeField] float eyeTrackingSpeed;
    [SerializeField] float leftEyeMaxYRotation;
    [SerializeField] float leftEyeMinYRotation;
    [SerializeField] float rightEyeMaxYRotation;
    [SerializeField] float rightEyeMinYRotation;
    #endregion

    #region Root
    [Header("Root motion")]
    [SerializeField] float turnSpeed;
    [SerializeField] float moveSpeed;
    // How fast we will reach the above speeds
    [SerializeField] float turnAcceleration;
    [SerializeField] float moveAcceleration;
    /// <summary>
    /// Try to stay in this range from the target
    /// </summary>
    [SerializeField] float minDistToTarget;
    /// <summary>
    /// Try to stay in this range from the target
    /// </summary>
    [SerializeField] float maxDistToTarget;
    /// <summary>
    /// Turning angle threshold
    /// </summary>
    [SerializeField] float maxAngToTarget;

    /// <summary>
    /// World space velocity
    /// </summary>
    Vector3 currentVelocity;
    float currentAngularVelocity;
    #endregion

    #region Legs
    [Header("Legs")]
    [SerializeField] LegStepper frontLeftLegStepper;
    [SerializeField] LegStepper frontRightLegStepper;
    [SerializeField] LegStepper backLeftLegStepper;
    [SerializeField] LegStepper backRightLegStepper;
    #endregion

    /// <summary>
    /// Allows other systems to update the environment first <br/>
    /// The animation system will adapt to it before the frame is drawn
    /// </summary>
    private void LateUpdate()
    {
        RootMotionUpdate();
        HeadTrackingUpdate(); // Eyes are children of the head, so we want to make sure the head is updated first
        EyeTrackingUpdate();
    }

    private void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    private void HeadTrackingUpdate()
    {
        // Store the current head rotation
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset the head rotation so world to local space transformation will use the head's "zero" rotation.
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        // Transform a direction from world space to local space
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward,
            targetLocalLookDir,
            Mathf.Deg2Rad * headMaxTurnAngle,
            0 // length is 0 because no one cares
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply frame rate independent damping function smoothing
        headBone.localRotation = Quaternion.Slerp(
            currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
    }

    private void EyeTrackingUpdate()
    {
        // Head position is used here (so no cross eyed gecko)
        Quaternion targetEyeRotation = Quaternion.LookRotation(
            target.position - headBone.position,
            transform.up
        );

        leftEyeBone.rotation = Quaternion.Slerp(
            leftEyeBone.rotation,
            targetEyeRotation,
            1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime)
        );

        rightEyeBone.rotation = Quaternion.Slerp(
            rightEyeBone.rotation,
            targetEyeRotation,
            1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime)
        );


        float leftEyeCurrentYRotation = leftEyeBone.localEulerAngles.y;
        float rightEyeCurrentYRotation = rightEyeBone.localEulerAngles.y;

        // Map the rotation to -180..+180 degrees range
        if (leftEyeCurrentYRotation > 180)
            leftEyeCurrentYRotation -= 360;

        if (rightEyeCurrentYRotation > 180)
            rightEyeCurrentYRotation -= 360;


        // Clamp the Y axis rotation
        float leftEyeClampedYRotation = Mathf.Clamp(
            leftEyeCurrentYRotation,
            leftEyeMinYRotation,
            leftEyeMaxYRotation
        );

        float rightEyeClampedYRotation = Mathf.Clamp(
            rightEyeCurrentYRotation,
            rightEyeMinYRotation,
            rightEyeMaxYRotation
        );


        // Apply the clamped Y rotation without changing the X and Z rotations
        leftEyeBone.localEulerAngles = new Vector3(
            leftEyeBone.localEulerAngles.x,
            leftEyeClampedYRotation,
            leftEyeBone.localEulerAngles.z
        );
        rightEyeBone.localEulerAngles = new Vector3(
            rightEyeBone.localEulerAngles.x,
            rightEyeClampedYRotation,
            rightEyeBone.localEulerAngles.z
        );
    }

    /// <summary>
    /// Only allow diagonal leg pairs to step together
    /// </summary>
    IEnumerator LegUpdateCoroutine()
    {
        while (true)
        {
            // Try moving one diagonal pair of legs
            do
            {
                frontLeftLegStepper.TryMove();
                backRightLegStepper.TryMove();
                // Wait a frame
                yield return null;
                // Stay in this loop while either leg is moving
            } while (backRightLegStepper.Moving || frontLeftLegStepper.Moving);

            // Do the same thing for the other diagonal pair
            do
            {
                frontRightLegStepper.TryMove();
                backLeftLegStepper.TryMove();
                yield return null;
            } while (backLeftLegStepper.Moving || frontRightLegStepper.Moving);
        }
    }

    /// <summary>
    /// Rotate gecko's body toward the target
    /// </summary>
    void RootMotionUpdate()
    {
        #region Rotation
        Vector3 towardTarget = target.position - transform.position;
        // Project the direction toward the target to the local XZ plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        // Get the angle from the gecko's forward direction to the direction toward toward our target
        // Here we get the signed angle around the up vector so we know which direction to turn in
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

        float targetAngularVelocity = 0;

        // If we are within the max angle (i.e. approximately facing the target)
        // leave the target angular velocity at zero
        if (Mathf.Abs(angToTarget) > maxAngToTarget)
        {
            // Angles in Unity are clockwise, so a positive angle here means to our right
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            // Invert angular speed if target is to our left
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }

        // Use the smoothing function to gradually change the velocity
        currentAngularVelocity = Mathf.Lerp(
            currentAngularVelocity,
            targetAngularVelocity,
            1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
        );

        // Rotate the transform around the Y axis in world space,
        // making sure to multiply by delta time to get a consistent angular velocity
        transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);
        #endregion

        #region Movement
        Vector3 targetVelocity = Vector3.zero;

        // Don't move if we're facing away from the target, just rotate in place
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // If we're too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * towardTargetProjected.normalized;
            }
            // If we're too close, reverse the direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -towardTargetProjected.normalized;
            }
        }

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
        );

        // Apply the velocity
        transform.position += currentVelocity * Time.deltaTime;
        #endregion
    }
}
