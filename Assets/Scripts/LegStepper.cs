using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegStepper : MonoBehaviour
{
    /// <summary>
    /// Position and rotation we want to stay in range of
    /// </summary>
    [SerializeField] Transform homeTransform;
    /// <summary>
    /// Desired distance from home
    /// </summary>
    [SerializeField] float wantStepAtDistance;
    /// <summary>
    /// How long a step takes to complete
    /// </summary>
    [SerializeField] float moveDuration;
    /// <summary>
    /// Fraction of the max distance from home we want to overshoot by
    /// </summary>
    [SerializeField] float stepOvershootFraction;

    /// <summary>
    /// Is the leg moving
    /// </summary>
    public bool Moving;

    // The step shouldn't be to be instantaneous, so we will use a Coroutine
    private IEnumerator MoveToHome()
    {
        Moving = true;

        // Initial conditions
        transform.GetPositionAndRotation(out Vector3 startPoint, out Quaternion startRot);

        Quaternion endRot = homeTransform.rotation;

        // Directional vector from the foot to the home position
        Vector3 towardHome = homeTransform.position - transform.position;
        // Total distance to overshoot by
        float overshootDistance = wantStepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        // Restrict the overshoot vector to be level with the ground
        // by projecting it on the world XZ plane.
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPoint = homeTransform.position + overshootVector;

        // We want to pass through the center point
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // But also lift off, so we move it up by half the step distance (arbitrarily)
        centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

        // Time since step started
        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / moveDuration;
            normalizedTime = Easing.Cubic.InOut(normalizedTime);

            // Interpolate position and rotation using quadratic bezier curve
            transform.SetPositionAndRotation(
                Vector3.Lerp(
                    Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                    Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                    normalizedTime
                ),
                Quaternion.Slerp(startRot, endRot, normalizedTime)
            );

            // Wait for one frame
            yield return null;
        } while (timeElapsed < moveDuration);

        Moving = false;
    }

    public void TryMove()
    {
        // Don't start another move if already moving
        if (Moving)
            return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);

        // If we are too far off in position or rotation
        if (distFromHome > wantStepAtDistance)
        {
            // Start the step coroutine
            StartCoroutine(MoveToHome());
        }
    }
}

// FEET ?!?!?!?!?
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%%%&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%@@@@@@&&@&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#/(%&&@@@@@@@@@@%%%%&@&&&&%&@@@@@@@@@
// @@@@@@@@@@@@@@@@@&&%%%%%%%&@@@@@@@&&&&&&&&&&&&&&@@@@@@@@@%#%&&&&&&@@@&%&@@@@@@@@
// @@@@@@@@@@@@@@@@%#&&&&&&&&%&@@@@&%%%%%%##%%&@@@@@@&%%%&&&#&&&&%%%&&@&@%%@@@@@@@@
// @@@@@@@@@@@@@@@&#%&&%%%%&&&@@@@@@&&%%%%%%%%%%&&@@@%//(#%%&%&&&&&&&&&&&#%@@@@@@@@
// @@@@@@@@@@@@@@@&##&&%%&&&@&##%############(((((#(((((((##%@@&@@&&&&%%%&@@@@@@@@@
// @@@@@@@@@@@@@@@@&##%&&&&@&%#%%%%####((##&@@@&##(((////(((#%(&@@&%%&&@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@&&&@@@@@@@@@@@@&%%&@@@@@@@@@@&#((///((#%%&@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%////((##%&&@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%(//((((#%%&@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@&%%@@@@@@@@@@@@@@@@@@@@@@&%(//(#(((##%&@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@&%###%%%%%&&&&&%%%%##%%%#####%#########%%&@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@%#######(((((##((((/////((//(((######(((#%&@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@%%(((((////**/((((////******((//((#%#####%&&@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@%#(((((///////(/////***********/((#%%%%&&&&&@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@&%##(((((((((((((((((//////(((##%%%%%#&@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@&&%&&&%%%&%%%%&&&&&&%%%#%%%%%%%%#(%@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@//,*/**.*...*/*./(%#%%%%%%#(%@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@&%&&%((&@&/#&&@%*/&&%%&&%(#&%&%((&@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@%%&%/*&@&&&&&&&&&&&&&&&&#/&&&%##%&@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@&%%&&&//*/(//(@%(((%/**/@&&&%###%&@@@%%%%%%&&@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@%##%&#(###%%%&%&&&&&&&&&&&&%%#(((##%&@@@@%#######%&@@@@@@@@
// @@@@@@@@@@@@@@@@@@&#####%&&#((((#####((##%%########(##&&&%%%&@%##((((%@@@@&%%%%&
// @@@@@@@@@@@@@@@@@%(((##%%%&@@&&&%%%%##########%%%&&&&&@&%###%@&##%&%(%@@&#((####
// @@@@@@@@@@@@@@@@%((((##%%%&@@@@@%&@@@@&&&&&&&@@@@@@@&@%##((#%@@@@@@@@@%%#(((((((

