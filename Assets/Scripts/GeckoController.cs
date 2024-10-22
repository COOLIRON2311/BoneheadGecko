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

    /// <summary>
    /// Allows other systems to update the environment first <br/>
    /// The animation system will adapt to it before the frame is drawn
    /// </summary>
    private void LateUpdate()
    {
        // vector pointing at the target from the head’s current position
        Vector3 towardObjectFromHead = target.position - headBone.position;

        // This method takes a Forward and Up direction, and outputs a rotation which,
        // when applied to a transform, orients the transform so that its Z axis faces
        // in the Forward direction, and its Y axis faces Up.
        headBone.rotation = Quaternion.LookRotation(towardObjectFromHead, transform.up);
    }
}
