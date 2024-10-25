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
    /// Is the leg moving
    /// </summary>
    public bool Moving;

    // The step shouldn't be to be instantaneous, so we will use a Coroutine
    private IEnumerator MoveToHome()
    {
        Moving = true;

        // Initial conditions
        transform.GetPositionAndRotation(out Vector3 startPoint, out Quaternion startRot);
        homeTransform.GetPositionAndRotation(out Vector3 endPoint, out Quaternion endRot);

        // Time since step started
        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / moveDuration;

            // Interpolate position and rotation
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPoint, endPoint, normalizedTime),
                Quaternion.Slerp(startRot, endRot, normalizedTime)
            );

            // Wait for one frame
            yield return null;
        } while (timeElapsed < moveDuration);

        Moving = false;
    }

    private void Update()
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

