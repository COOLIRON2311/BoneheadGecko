using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeckoController : MonoBehaviour
{
    /// <summary>
    /// Tracking target
    /// </summary>
    [SerializeField] Transform target;
    /// <summary>
    /// Gecko's neck
    /// </summary>
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;

    [SerializeField] Transform leftEyeBone;
    [SerializeField] Transform rightEyeBone;
    [SerializeField] float eyeTrackingSpeed;
    [SerializeField] float leftEyeMaxYRotation;
    [SerializeField] float leftEyeMinYRotation;
    [SerializeField] float rightEyeMaxYRotation;
    [SerializeField] float rightEyeMinYRotation;

    /// <summary>
    /// Allows other systems to update the environment first <br/>
    /// The animation system will adapt to it before the frame is drawn
    /// </summary>
    private void LateUpdate()
    {
        HeadTrackingUpdate(); // Eyes are children of the head, so we want to make sure the head is updated first
        EyeTrackingUpdate();
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
}
