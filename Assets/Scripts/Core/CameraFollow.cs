/*******************************************************************************
 *
 *  File Name: CameraFollow.cs
 *
 *  Description: Makes the camera follow a targeted player.
 *
 *******************************************************************************/
using GSP.Core;
using UnityEngine;
using System.Collections;

namespace GSP.Core
{
    /*******************************************************************************
     *
     * Name: CameraFollow
     * 
     * Description: Follows a targeted player with the camera.
     * 
     *******************************************************************************/
    public class CameraFollow : MonoBehaviour
    {
        float offsetZ;              // The offset on the Z-axis
        public float offsetX;              // The offset on the X-axis
        Vector3 lastTargetPosition; // The last position of the target
        Vector3 currentVelocity;    // The current velocity of the following
        Vector3 lookAheadPos;       // The position to look in front of the target
        Transform target;           // The target to follow

        float damping;
        float lookAheadFactor;
        float lookAheadReturnSpeed;
        float lookAheadMoveThreshold;

        // Used for initialisation
        void Start()
        {
            // Initialise the movement variables
            damping = 0.4f;
            lookAheadFactor = 2.0f;
            lookAheadReturnSpeed = 2.5f;
            lookAheadMoveThreshold = 0.1f;

            // Check what map we are on
            if (Application.loadedLevelName == "area01")
            {
                // Set the X-axis offset
                offsetX = 7.3f;
            } // end if
            else
            {
                // Otherwise set the X-axis offset of the zoomed camera
                offsetX = 3.8f;
            } // end else

            // Make sure we are not a child
            transform.parent = null;
        }

        // Use this function to process updates each frame
        void Update()
        {
            GameObject obj = GameMaster.Instance.GetPlayerObject(GameMaster.Instance.Turn);

            if (obj != null)
            {
                // Get the player reference to follow the player
                target = obj.transform;

                // Initialise the last target position to the current target's position
                lastTargetPosition = target.position;

                // Set the Z offset
                offsetZ = transform.position.z - target.position.z;

                // Only update the look-ahead position if we are accelerating or have changed directions
                float moveDeltaX = target.position.x - lastTargetPosition.x;
                bool updateLookAheadTarget = Mathf.Abs(moveDeltaX) > lookAheadMoveThreshold;
                if (updateLookAheadTarget)
                {
                    // Update the look-ahead position
                    lookAheadPos = lookAheadFactor * Vector3.right;
                }
                else
                {
                    // Otherwise move towards the target of zero offsetted on the x-axis
                    Vector3 center = Vector3.zero;
                    center.x = offsetX;
                    lookAheadPos = Vector3.MoveTowards(lookAheadPos, center, Time.deltaTime * lookAheadReturnSpeed);
                }

                // Get the ahead target position
                Vector3 aheadTargetPosition = target.position + lookAheadPos + Vector3.forward * offsetZ;
                // Smoothly move towards the new position
                Vector3 newPosition = Vector3.SmoothDamp(transform.position, aheadTargetPosition, ref currentVelocity, damping);
                // Set the position to the new position
                transform.position = newPosition;
                // Set the last target position to the current position
                lastTargetPosition = target.position;
            }
        }
    }
} // end GSP.Core
