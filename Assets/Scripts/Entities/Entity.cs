﻿using UnityEngine;
using System.Collections;
using GSP.Char;

namespace GSP.Entities
{
	public abstract class Entity : System.IDisposable
	{
		int			m_ID;			// Holds the enity's ID.
		Vector2		m_position;		// Holds the entity's position.

		EntityType	m_type;			// Holds the entity's type.
		GameObject	m_gameObject;	// Holds the game objecy the entity is attached to.

		// Constructor.
		public Entity(int ID, GameObject gameObject)
		{
			m_ID = ID;
			m_gameObject = gameObject;
			// Set the type to none initially.
			m_type = EntityType.ENT_NONE;
		}

		// Gets and protected sets the entity's type
		public EntityType Type { get; protected set; }

		// Gets and sets the entity's position. (not always used)
		public Vector2 Position
		{
			get
			{
				return m_position;
			}

			set
			{
				m_position = value;
			}
		}

		// Gets the ID of the entity.
		public int ID
		{ 
			get
			{
				return m_ID;
			}
		}

		#region IDisposable Members
		
		// Public dispose method that will call the internal dispose method.
		public void Dispose()
		{
			// Dispose the entity object.
			Dispose(true);
			
			// Now since we've done the cleanup already, there is nothing left
			// for tge finalizer to do. So tell the GC not to call it later.
			System.GC.SuppressFinalize(this);
		}
		
		// Internal dispose method. It executes in two distinct scenarios. 
		// If disposing equals true, the method has been called directly 
		// or indirectly by a user's code. Managed and unmanaged resources 
		// can be disposed. 
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed. 
		private void Dispose(bool disposing)
		{
			// Only proceed if we're disposing directly.
			if (disposing)
			{
				// TODO: Destroy the entity's game object.
				// Switch over the entity types.
				switch (m_type)
				{
					case EntityType.ENT_MERCHANT:
					{
						// Get the player script attached to the game object.
						var script = m_gameObject.GetComponent<Player>();

						// Tell the script to destroy its gameobject.
						script.DestroyGO();

						break;
					}
					case EntityType.ENT_PORTER:
					{
						// Get the ally script attached to the game object.
						var script = m_gameObject.GetComponent<Ally>();
						
						// Tell the script to destroy its gameobject.
						script.DestroyGO();
						
						break;
					}
					case EntityType.ENT_MERCINARY:
					{
						goto case EntityType.ENT_PORTER;
					}
					case EntityType.ENT_BANDIT:
					{
						// Get the enemy script attached to the game object.
						var script = m_gameObject.GetComponent<Enemy>();
						
						// Tell the script to destroy its gameobject.
						script.DestroyGO();
						
						break;
					}
					case EntityType.ENT_MIMIC:
					{
						goto case EntityType.ENT_BANDIT;
					}
				}
			}
		}
		
		#endregion
	}
}