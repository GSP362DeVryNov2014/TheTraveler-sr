﻿/*******************************************************************************
 *
 *  File Name: GUIMovement.cs
 *
 *  Description: Old GUI for the movement
 *
 *******************************************************************************/
using GSP.Char;
using UnityEngine;

namespace GSP
{
    //TODO: Brent: Replace this with the new In-Game UI later; probably not in the same namespace
    /*******************************************************************************
     *
     * Name: GUIMovement
     * 
     * Description: Creates the GUI for the player's movement.
     * 
     *******************************************************************************/
    public class GUIMovement : MonoBehaviour
    {
		bool isInSelectPathToTakeState = false;                 // This value is rturned to the GameplayStateMachine, when false, it means ends turn
		int selectState = (int)GamePlayState.SelectPathToTake;  // This is the state we are concerned about
		GameplayStateMachine gameplayStateMachineScript;        // Script to access state machine 
		Movement movementScript;                                // The movement script reference
        Player playerScript;                                    // This is the current player's Player script
		GameObject audioSrc;                                    // The AudioSource

		Vector3 displacementVector; // value player moves relative to Space.World
		Vector3 origPlayerPosition; //if player cancels movement, player resets to this original poisition
		int initialTravelDist;      // initial dice roll
		int currTravelDist;         // dice roll left
		bool isMoving = false;      // if the player is moving, one dice roll value is taken off

		// Use this for initialization
		void Start()
        {
            gameplayStateMachineScript = GameObject.FindGameObjectWithTag("GamePlayStateMachineTag").GetComponent<GameplayStateMachine>();
            movementScript = GameObject.FindGameObjectWithTag("GUIMovementTag").GetComponent<Movement>();
            audioSrc = GameObject.FindGameObjectWithTag("AudioSourceTag");

		} // end Start

        //TODO: Brent: Replace OnGUI stuff with the new In-Game UI later
        // Initialise things sort of like a custom constructor
		public void InitThis(Player player, int travelDistance)
		{
			// player script
            playerScript = player;

			// turn just started
			isInSelectPathToTakeState = true;

			// store orig position
			origPlayerPosition = player.transform.position;

			// how much can the player move
			initialTravelDist = travelDistance;
			currTravelDist = initialTravelDist;

			// resetDisplacement Value
            displacementVector = Vector3.zero;

			// Generate the initial set of highlight objects
            Highlight.GenerateHighlight(origPlayerPosition, currTravelDist);
		} // end InitThis

        //TODO: Brent: Replace OnGUI stuff with the new In-Game UI later
        // The old style GUI functioned using an OnGUI function; Runs each frame
        void OnGUI()
		{
			// listen for change in state on the GameplayStateMachine
            if (gameplayStateMachineScript.GetState() != selectState)
			{
				isInSelectPathToTakeState = false;
			} // end if

			// if the GameplayStateMachine is in SelectPathToTake state, show gui
			if (isInSelectPathToTakeState)
			{
				GUIMovementPads();
			} // end if
		} // end OnGUI()

        //TODO: Brent: Replace OnGUI stuff with the new In-Game UI later
        // Configures the movement pads of the OnGUI UI system
        private void GUIMovementPads()
		{
            int width = 32;
            int height = 32;
            int gridXShift = -8;
            int gridYShift = -8;

			// Button color
			GUI.backgroundColor = Color.red;
			if(currTravelDist > 0)
			{
				// left
                if (GUI.Button(new Rect((Screen.width - (3 * width) + gridXShift), (Screen.height - (2 * height) + gridYShift), width, height), "<"))
				{
                    displacementVector = movementScript.MoveLeft(playerScript.gameObject.transform.position);
                    // Face the character to the west which is to the left
                    playerScript.Face(FacingDirection.West);
					MovePlayer();
                    // Clear the highlight objects
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
                    //TODO: Brent: Replace with AudioManager later
                    //Play walking sound
                    //audioSrc.GetComponent<AudioSource>().PlayOneShot(GSP.AudioReference.sfxWalking);
				} // end if
				// right
                if (GUI.Button(new Rect((Screen.width - (1 * width) + gridXShift), (Screen.height - (2 * height) + gridYShift), width, height), ">"))
				{
                    displacementVector = movementScript.MoveRight(playerScript.gameObject.transform.position);
                    // Face the character to the east which is to the right
                    playerScript.Face(FacingDirection.East);
					MovePlayer();
                    // Clear the highlight objects
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
                    //TODO: Brent: Replace with AudioManager later
                    //Play walking sound
                    //audioSrc.GetComponent<AudioSource>().PlayOneShot(GSP.AudioReference.sfxWalking);
				} // end if
				// cancel
                if (GUI.Button(new Rect((Screen.width - (2 * width) + gridXShift), (Screen.height - (2 * height) + gridYShift), width, height), "X"))
				{
                    // Face the character to the south; This is the default facing
                    playerScript.Face(FacingDirection.South);
					// Move Back To Orig Position
					CancelMove();
                    // Clear the highlight objects
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
                    //TODO: Brent: Replace with AudioManager later
                    //Play walking sound
                    //audioSrc.GetComponent<AudioSource>().PlayOneShot(GSP.AudioReference.sfxWalking);
				} // end if
				// up
                if (GUI.Button(new Rect((Screen.width - (2 * width) + gridXShift), (Screen.height - (3 * height) + gridYShift), width, height), "^"))
				{
                    displacementVector = movementScript.MoveUp(playerScript.gameObject.transform.position);
                    // Face the character to the north which is up
                    playerScript.Face(FacingDirection.North);
					MovePlayer();
                    // Clear the highlight objects
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
                    //TODO: Brent: Replace with AudioManager later
                    //Play walking sound
                    //audioSrc.GetComponent<AudioSource>().PlayOneShot(GSP.AudioReference.sfxWalking);
				} // end if
				// down
                if (GUI.Button(new Rect((Screen.width - (2 * width) + gridXShift), (Screen.height - (1 * height) + gridYShift), width, height), "v"))
				{
                    displacementVector = movementScript.MoveDown(playerScript.gameObject.transform.position);
                    // Face the character to the south which is down
                    playerScript.Face(FacingDirection.South);
					MovePlayer();
                    // Clear the highlight objects
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
                    //TODO: Brent: Replace with AudioManager later
                    //Play walking sound
                    //audioSrc.GetComponent<AudioSource>().PlayOneShot(GSP.AudioReference.sfxWalking);
				} // end if down button
			} // end if
			else
			{
                if (GUI.Button(new Rect((Screen.width - (2 * width) + gridXShift), (Screen.height - (2 * height) + gridYShift), width, height), "X"))
				{
                    // Face the character to the south; This is the default facing
                    playerScript.Face(FacingDirection.South);
					CancelMove();
                    // Clear the highlight objects.
                    Highlight.ClearHighlight();
                    // Recreate the highlights with the new values
                    Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
				} // end if
				// display travel distance is 0
                GUI.Box(new Rect((Screen.width - (3 * width) + gridXShift), (Screen.height - (3 * height) + gridYShift), 3 * width, height), "Out of Distance.");
            } // end else
		} // end GUIMovementPads

        // Moves the player
        void MovePlayer()
		{
            if (displacementVector == Vector3.zero)
			{
				isMoving = false;
				return;
			} // end if

			// player started moving take off a dice roll value
			if (!isMoving)
			{
				isMoving = true;
				currTravelDist = currTravelDist -1;
			}

			// move player
            playerScript.gameObject.transform.Translate(displacementVector, Space.World);
            // Update the entity's position
            playerScript.Position = playerScript.gameObject.transform.position;
			// reset
			displacementVector = Vector3.zero;
			isMoving = false;
		} // end MovePlayer

        // Cancels a move sending the player back to the original position
        private void CancelMove()
		{
            playerScript.gameObject.transform.position = origPlayerPosition;
            // Update the entity's position
            playerScript.Position = playerScript.gameObject.transform.position;
			currTravelDist = initialTravelDist;
			isMoving = false;
			displacementVector = Vector3.zero;
		} // end CancelMove

        // Gets the current travel distance remaining
        public int RemainingTravelDistance
        {
            get { return currTravelDist; }
        } // end RemainingTravelDistance
    } // end GUIMovement
} // end GSP