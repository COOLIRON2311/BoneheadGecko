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

    }
}
