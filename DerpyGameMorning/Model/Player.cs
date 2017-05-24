﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DerpyGame.View;
using DerpyGame.Controller;
namespace DerpyGame.Model
{
	public class Player
	{
		// Animation representing the player
		private Animation playerAnimation;
		public Animation PlayerAnimation
		{
			get { return playerAnimation; }
			set { playerAnimation = value; }
		}

        private Animation shieldedAnimation;
        public Animation ShieldedAnimation
        {
            get { return shieldedAnimation; }
            set { shieldedAnimation = value; }
        }
        public bool IsShielded;
		// Position of the Player relative to the upper left side of the screen

		public Vector2 Position;

		// State of the player
		private bool active;
		public bool Active
		{
			get { return active; }
			set { active = value; }
		}

		// Amount of hit points that player has
		private int health;
		public int Health
		{
			get { return health; }
			set { health = value; }
		}

		// Get the width of the player ship
		public int Width
		{
		get { return PlayerAnimation.FrameWidth; }
		}

		// Get the height of the player ship
		public int Height
		{
			get { return PlayerAnimation.FrameHeight; }
		}

		public void Initialize(Animation animation,Animation shield, Vector2 position)
		{
            IsShielded = false;
            ShieldedAnimation = shield;
			PlayerAnimation = animation;

			// Set the starting position of the player around the middle of the screen and to the back
			Position = position;

			// Set the player to be active
			Active = true;

			// Set the player health
			Health = 100;
		}

		// Update the player animation
		public void Update(GameTime gameTime)
		{
            if (!IsShielded)
            {
                PlayerAnimation.Position = Position;
                PlayerAnimation.Update(gameTime);
            }
            else
            {
                ShieldedAnimation.Position = Position;
                ShieldedAnimation.Update(gameTime);
            }
		}	

		// Draw the player
		public void Draw(SpriteBatch spriteBatch)
		{
            if(IsShielded)
            {
                ShieldedAnimation.Draw(spriteBatch);
            }
            else
            {
               PlayerAnimation.Draw(spriteBatch);
            }
		}

		public Player()
		{
			
		}
	}
}
