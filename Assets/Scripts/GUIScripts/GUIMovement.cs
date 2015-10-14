/*******************************************************************************
 *
 *  File Name: GUIMovement.cs
 *
 *  Description: Old GUI for the movement
 *
 *******************************************************************************/
using GSP.Char;
using GSP.Core;
using GSP.Tiles;
using UnityEngine;
using UnityEngine.UI;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: GUIMovement
     * 
     * Description: Creates the GUI for the player's movement.
     * 
     *******************************************************************************/
    public class GUIMovement : MonoBehaviour
    {
        Player playerScript;        // This is the current player's Player script
		Animator animator;			// The player's animator
		Vector3 displacementVector; // value player moves relative to Space.World
		Vector3 origPlayerPosition; // If player cancels movement, player resets to this original poisition
		int initialTravelDist;      // Initial dice roll
		int currTravelDist;         // Dice roll left
		bool isMoving;              // If the player is moving, one dice roll value is taken off
        bool isPlayerAI;            // Whether the player is an AI
		Button upButton;			// Up movement button		
		Button downButton;			// Down movement button
		Button leftButton;			// Left movement button
		Button rightButton;			// Right movement button
		Button cancelButton;		// Cancel movement button

		// Used for initialisation
        void Awake()
        {
            isMoving = false;
            isPlayerAI = false;
        } // end Awake
        
        // Get component references
		void Start()
		{
			// Get HUD Arrows
            upButton = GameObject.Find("CurrentPlayer/UpButton").GetComponent<Button>();
            downButton = GameObject.Find("CurrentPlayer/DownButton").GetComponent<Button>();
            leftButton = GameObject.Find("CurrentPlayer/LeftButton").GetComponent<Button>();
            rightButton = GameObject.Find("CurrentPlayer/RightButton").GetComponent<Button>();
            cancelButton = GameObject.Find("CurrentPlayer/CancelButton").GetComponent<Button>();
			
			// Make sure they are disabled
			DisableButtons ();
		} //end Start

        // Initialise things sort of like a custom constructor
		public void InitThis(Player player, int travelDistance, bool isAI)
		{
            // Set whether the player is an AI
            isPlayerAI = isAI;
            
            // Make sure the player isn't AI before enabling the buttons
            if (!isAI)
            {
                // Make sure buttons are enabled
                EnableButtons();
            } // end if

			// Player script
            playerScript = player;

			// Player animator
			animator = GameMaster.Instance.GetPlayerAnimator (GameMaster.Instance.Turn);

			// Store original position
			origPlayerPosition = player.transform.position;

			// How much can the player move
			initialTravelDist = travelDistance;
			currTravelDist = initialTravelDist;

			// Initialize displacement vector
			displacementVector = Vector3.zero;

			// Generate the initial set of highlight objects
            Highlight.GenerateHighlight(origPlayerPosition, currTravelDist);
		} // end InitThis

		public void MoveUp()
		{
			//If we're not at the top end of the map, allow movement
			if(playerScript.gameObject.transform.position.y < TileUtils.MinHeight)
			{
				displacementVector = new Vector3(0, TileUtils.PlayerMoveDistance, 0);
			} //end if
			else
			{
				displacementVector = Vector3.zero;
			} //end else

			//Play walking sound
			AudioManager.Instance.PlayWalk ();

			// Move player and play walking animation
			animator.SetBool("Idle_N", true);
			animator.SetBool("Idle_S", false);
			animator.SetBool("Idle_W", false);
			animator.SetBool("Idle_E", false);
			animator.SetTrigger ("Walk_N");
			MovePlayer ();

			// Clear the highlight objects
			Highlight.ClearHighlight();

			// Recreate the highlights with the new values
			Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
		} //end MoveUp

		public void MoveDown()
		{
			//If we're not at the bottom end of the map, allow movement
			if(playerScript.gameObject.transform.position.y > TileUtils.MaxHeight)
			{
				displacementVector = new Vector3(0, -TileUtils.PlayerMoveDistance, 0);
			} //end if
			else
			{
				displacementVector = Vector3.zero;
			} //end else

			//Play walking sound
			AudioManager.Instance.PlayWalk ();
			
			// Move player and play walking animation
			animator.SetBool("Idle_N", false);
			animator.SetBool("Idle_S", true);
			animator.SetBool("Idle_W", false);
			animator.SetBool("Idle_E", false);
			animator.SetTrigger ("Walk_S");
			MovePlayer ();

			// Clear the highlight objects
			Highlight.ClearHighlight();

			// Recreate the highlights with the new values
			Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
		} //end MoveDown

		public void MoveLeft()
		{
			//If we're not at the left end of the map, allow movement
			if(playerScript.gameObject.transform.position.x > TileUtils.MinWidth)
			{
				displacementVector = new Vector3(-TileUtils.PlayerMoveDistance, 0, 0);
			} //end if
			else
			{
				displacementVector = Vector3.zero;
			} //end else

			//Play walking sound
			AudioManager.Instance.PlayWalk ();
			
			// Move player and play walking animation
			animator.SetBool("Idle_N", false);
			animator.SetBool("Idle_S", false);
			animator.SetBool("Idle_W", true);
			animator.SetBool("Idle_E", false);
			animator.SetTrigger ("Walk_W");
			MovePlayer ();

			// Clear the highlight objects
			Highlight.ClearHighlight();

			// Recreate the highlights with the new values
			Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
		} //end MoveLeft

		public void MoveRight()
		{
			// If we're not at the right end of the map, allow movement
			if(playerScript.gameObject.transform.position.x < TileUtils.MaxWidth)
			{
				displacementVector = new Vector3(TileUtils.PlayerMoveDistance, 0, 0);
			} //end if
			else
			{
				displacementVector = Vector3.zero;
			} //end else

			//Play walking sound
			AudioManager.Instance.PlayWalk ();
			
			// Move player and play walking animation
			animator.SetBool("Idle_N", false);
			animator.SetBool("Idle_S", false);
			animator.SetBool("Idle_W", false);
			animator.SetBool("Idle_E", true);
			animator.SetTrigger ("Walk_E");
			MovePlayer ();

			// Clear the highlight objects
			Highlight.ClearHighlight();

			// Recreate the highlights with the new values
			Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);
		} //end MoveRight

		// Cancels a move sending the player back to the original position
		public void CancelMove()
		{
            // Make sure the player isn't AI before enabling the buttons
            if (!isPlayerAI)
            {
                // Make sure buttons are enabled
                EnableButtons();
            } // end if

			// Face the character to the south; This is the default facing
			animator.SetBool("Idle_N", false);
			animator.SetBool("Idle_S", true);
			animator.SetBool("Idle_W", false);
			animator.SetBool("Idle_E", false);

			// Return player to original position
			playerScript.gameObject.transform.position = origPlayerPosition;

			// Update the entity's position
			playerScript.Position = playerScript.gameObject.transform.position;

			// Return to original distance
			currTravelDist = initialTravelDist;

			// Make sure player is not moving
			isMoving = false;
			displacementVector = Vector3.zero;

			// Clear the highlight objects
			Highlight.ClearHighlight();

			// Recreate the highlights with the new values
			Highlight.GenerateHighlight(playerScript.gameObject.transform.position, currTravelDist);

			//Play walking sound
			AudioManager.Instance.PlayWalk ();
		} // end CancelMove

        // Moves the player
        void MovePlayer()
		{
			// If no move is possible, end the function
            if ((displacementVector == Vector3.zero) || (currTravelDist <= 0))
			{
				isMoving = false;
				return;
			} // end if

			// If player is not already moving, allow them to move and remove a movement unit
			if (!isMoving)
			{
				isMoving = true;
				currTravelDist = currTravelDist -1;
			} //end if

			// Move player
            playerScript.gameObject.transform.Translate(displacementVector, Space.World);

            // Update the entity's position
            playerScript.Position = playerScript.gameObject.transform.position;

			// Reset movement to none and return player to not moving state
			displacementVector = Vector3.zero;
			isMoving = false;

			//See if player still has distance to travel
			TravelDistanceLeft ();
		} // end MovePlayer

		// Disable buttons if out of travel distance
		public void TravelDistanceLeft()
		{
            //If out of distance to move, disable movement arrows
			if(currTravelDist <= 0)
			{
				DisableButtons();
				
                // Make sure the player isn't AI before enabling the cancel button
                if (!isPlayerAI)
                {
                    cancelButton.interactable = true;
                } // end if !isPlayerAI
			} //end if
			else
			{
                // Make sure the player isn't AI before enabling the buttons
                if (!isPlayerAI)
                {
                    // Make sure buttons are enabled
                    EnableButtons();
                } // end if
			} //end else
		} //end TravelDistanceLeft

		// Disables buttons upon turn end
		public void DisableButtons()
		{
			upButton.interactable = false;
			downButton.interactable = false;
			leftButton.interactable = false;
			rightButton.interactable = false;
			cancelButton.interactable = false;
		} //end DisableButtons

		// Enables buttons upon turn start
		public void EnableButtons()
		{
			upButton.interactable = true;
			downButton.interactable = true;
			leftButton.interactable = true;
			rightButton.interactable = true;
			cancelButton.interactable = true;
		} //end EnableButtons

		// Gets the current travel distance remaining
        public int RemainingTravelDistance
        {
            get { return currTravelDist; }
        } // end RemainingTravelDistance
    } // end GUIMovement
} // end GSP