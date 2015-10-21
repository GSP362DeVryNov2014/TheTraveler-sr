/*******************************************************************************
 *
 *  File Name: ArmorType.cs
 *
 *  Description: The enumeration for the the various armour types
 *
 *******************************************************************************/

namespace GSP.Items
{
    // The types of armour the game uses.
    // NOTE!!
    // Size must be the last item in the enum so that anything based the length of
    // the enum can be used as normal. It is best to add items to the left of Size
    // but after the current second-to-last item in the enum. For instance if the
    // list was {Platebody, Platelegs, Size}; you should enter the new item between
    // Platelegs and Size. Create name here and then define it under "SetItem" function
    // of the Item class.
    public enum ArmorType
    {
        Platebody,
        Chainmail,
        Platelegs,
        Chainlegs,
        PlateFullsuit,
        Robe,
        Skirt,
        ClothFullsuit,
        Tunic,
        Leggings,
        LeatherFullsuit,
        ChainFullSuit,
        Size
    } // end ArmorType
} // end GSP.Items
