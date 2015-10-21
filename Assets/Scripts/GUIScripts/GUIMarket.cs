using UnityEngine;
using System.Collections;
using GSP.Core;
using GSP.Items.Inventories;
using GSP.Char;

namespace GSP
{
    public class GUIMarket : MonoBehaviour
    {
        // Used for initialisation
        void Awake()
        {
            // Load the players
            GameMaster.Instance.LoadPlayers(true);

            // Disable the ally's inventory
            GameObject.Find("Canvas").transform.Find("AllyInventory").gameObject.SetActive(false);

            // Set the colours
            GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>().SetPlayer(GameMaster.Instance.Turn);
            GameObject.Find("Canvas").transform.Find("Market").GetComponent<Market>().SetPlayer(GameMaster.Instance.Turn);
        } // end Awake

        // Used for initialisation
        void Start()
        {
            // The inventories are open
            GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>().IsOpen = true;
            GameObject.Find("Canvas").transform.Find("Market").GetComponent<Market>().IsOpen = true;
        } // end Start

        public void LeaveMarket()
        {
            // enable the ally's inventory
            GameObject.Find("Canvas").transform.Find("AllyInventory").gameObject.SetActive(true);

            // Get the player's script
            Player player = GameMaster.Instance.GetPlayerScript(GameMaster.Instance.Turn);

            // Check if the player is an AI and at the market
            if (player.IsAI && player.IsAtMarket)
            {
                // Tell the AI it's not at the market anymore
                player.IsAtMarket = false;
            } // end if
            
            // Save the players
            GameMaster.Instance.SavePlayers();

            // Go to the next turn
            GameMaster.Instance.NextTurn();

            // Load the level we was on before
            GameMaster.Instance.LoadLevel(GameMaster.Instance.BattleMap.ToString());
        } // end LeaveMarket
    }
}
