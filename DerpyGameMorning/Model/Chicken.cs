using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DerpyGame.View;

namespace DerpyGame.Model
{
	public class Chicken
	{
		// Animation representing the enemy
		private Animation chickenAnimation;
		public Animation ChickenAnimation
		{
			get { return chickenAnimation; }
			set { chickenAnimation = value; }
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

		// The hit points of the enemy, if this goes to zero the enemy dies
		private int health;
		public int Health
		{
			get { return health; }
			set { health = value; }
		}

		// The amount of damage the enemy inflicts on the player ship
		private int damage;
		public int Damage
		{
			get { return damage; }
			set { damage = value; }
		}

		// The amount of score the enemy will give to the player
		private int score;
		public int Score
		{
			get { return score; }
			set { score = value; }
		}

		// Get the width of the enemy ship
		public int Width
		{
			get { return ChickenAnimation.FrameWidth; }
		}

		// Get the height of the enemy ship
		public int Height
		{
			get { return ChickenAnimation.FrameHeight; }
		}

		// The speed at which the enemy moves
		float chickenMoveSpeedX;
        float chickenMoveSpeedY;

		public void Initialize(Animation animation, Vector2 position)
		{
			// Load the enemy ship texture
			ChickenAnimation = animation;

			// Set the position of the enemy
			Position = position;

			// We initialize the enemy to be active so it will be update in the game
			Active = true;


			// Set the health of the enemy
			Health = 100;

			// Set the amount of damage the enemy can do
			Damage = 9001;

			// Set how fast the enemy moves
			chickenMoveSpeedX = 5f;
            chickenMoveSpeedY = 5f;


			// Set the score value of the enemy
			Score = 500;

		}

		public void Update(GameTime gameTime)
		{
            Random random = new Random();
            int cRandom = random.Next(0, 1);

            //if(cRandom >= 1)
            //{
            //    Position.X -= chickenMoveSpeedX;
            //}
            //else
            //{
            //  Position.X = chickenMoveSpeedX;  
            //}

            Position.X -= chickenMoveSpeedX;			
            Position.Y += chickenMoveSpeedY;

			// Update the position of the Animation
				ChickenAnimation.Position = Position;

			// Update Animation
			ChickenAnimation.Update(gameTime);

			// If the enemy is past the screen or its health reaches 0 then deactivateit
			if (Position.X < -Width || Health <= 0)
			{
				// By setting the Active flag to false, the game will remove this objet fromthe
				// active game list
				Active = false;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			// Draw the animation
			ChickenAnimation.Draw(spriteBatch);
		}
	}
}
