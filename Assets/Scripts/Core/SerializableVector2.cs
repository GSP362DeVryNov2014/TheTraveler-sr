/*******************************************************************************
 *
 *  File Name: SerializableVector2.cs
 *
 *  Description: A serialisable version of the Vector2 struct.
 *
 *******************************************************************************/
using System;
using UnityEngine;

namespace GSP.Core
{
    /*******************************************************************************
     *
     * Name: SerializableVector2
     * 
     * Description: Since unity doesn't flag the Vector32as serializable, we need to
     *              create our own version. This one will automatically convert
     *              between Vector2 and SerializableVector2.
     * 
     *******************************************************************************/
    [Serializable]
    public struct SerializableVector2
    {
        // x component
        public float x;

        // y component
        public float y;

        // Constructor that used to build a SerializableVector3 object
        public SerializableVector2(float rX, float rY)
        {
            x = rX;
            y = rY;
        } // end SerializableVector2

        // Returns a string representation of the object
        public override string ToString()
        {
            return String.Format("[{0}, {1}]", x, y);
        } // end ToString

        // Automatic conversion from SerializableVector2 to Vector2
        public static implicit operator Vector2(SerializableVector2 rValue)
        {
            return new Vector2(rValue.x, rValue.y);
        } // end Vector2

        // Automatic conversion from Vector2 to SerializableVector2
        public static implicit operator SerializableVector2(Vector2 rValue)
        {
            return new SerializableVector2(rValue.x, rValue.y);
        } // end SerializableVector2
    } // end SerializableVector2
} // end GSP.Core
