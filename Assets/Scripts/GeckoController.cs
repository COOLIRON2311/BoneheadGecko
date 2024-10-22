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

    /// <summary>
    /// Allows other systems to update the environment first <br/>
    /// The animation system will adapt to it before the frame is drawn
    /// </summary>
    private void LateUpdate()
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
}
