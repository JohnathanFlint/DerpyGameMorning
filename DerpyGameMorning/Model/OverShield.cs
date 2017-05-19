using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DerpyGame.View;

namespace DerpyGame.Model
{
    public class OverShield
    {
		// Animation representing the enemy
		private Animation overshieldGenAnimation;
		public Animation OvershieldGenAnimation
		{
			get { return overshieldGenAnimation; }
			set { overshieldGenAnimation = value; }
		}
		// Animation representing the enemy
		private Animation overshieldAnimation;
		public Animation OvershieldAnimation
		{
			get { return overshieldAnimation; }
			set { overshieldAnimation = value; }
		}
		// Animation representing the enemy
		private Animation overshieldDeathAnimation;
		public Animation OvershieldDeathAnimation
		{
			get { return overshieldDeathAnimation; }
			set { overshieldDeathAnimation = value; }
		}

		// The position of the enemy ship relative to the top left corner of thescreen
		public Vector2 Position;

		// The state of the Enemy Ship
		private bool active;
		public bool Active
		{
			get { return active; }
			set { active = value; }
		}

		// Get the width of the enemy ship
		public int Width
		{
			get { return overshieldAnimation.FrameWidth; }
		}

		// Get the height of the enemy ship
		public int Height
		{
			get { return overshieldAnimation.FrameHeight; }
		}

		// The speed at which the enemy moves
		float overshieldMoveSpeedX;
		float overshieldMoveSpeedY;

		public void Initialize(Animation animation, Vector2 position)
		{
			// Load the enemy ship texture
			overshieldAnimation = animation;

			// Set the position of the enemy
			Position = position;

            // We initialize the enemy to be active so it will be update in the game
            Active = true;
	
		}

		public void Update(GameTime gameTime)
		{

			// Update the position of the Animation
			overshieldAnimation.Position = Position;

			// Update Animation
			overshieldAnimation.Update(gameTime);

		
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			// Draw the animation
			overshieldAnimation.Draw(spriteBatch);
		}
    }
}
