using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DerpyGame.Model;
using DerpyGame.View;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DerpyGame.Controller
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Steampunk : Game
	{
		#region Decleration Section
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		private Player player;

		// Keyboard states used to determine key presses
		private KeyboardState currentKeyboardState;
		private KeyboardState previousKeyboardState;

		// Gamepad states used to determine button presses
		private GamePadState currentGamePadState;
		private GamePadState previousGamePadState;

		// A movement speed for the player
		private float playerMoveSpeed;

		// Image used to display the static background
		Texture2D mainBackground;

		// Parallaxing Layers
		Background bgLayer1;
		Background bgLayer2;

		// Enemies
		Texture2D enemyTexture;
		List<Enemy> enemies;
		Texture2D chickenTexture;
		List<Chicken> chickens;

        //Shield
        Texture2D overshieldTexture;
        Texture2D overshieldGenTexture;
        Texture2D overshieldDeathTexture;
        private OverShield shield;

		// The rate at which the enemies appear
		TimeSpan enemySpawnTime;
		TimeSpan chickenSpawnTime;
		TimeSpan previousEnemySpawnTime;
        TimeSpan previousChickenSpawnTime;
        TimeSpan previousShieldPress;
        TimeSpan shieldRate;

		// A random number generator
		Random random;

		Texture2D projectileTexture;
		List<Projectile> projectiles;

		// The rate of fire of the player laser
		TimeSpan fireTime;
		TimeSpan previousFireTime;

		Texture2D explosionTexture;
		List<Animation> explosions;

		//Number that holds the player score
		int score;
		// The font used to display UI elements
		SpriteFont font;

        int playerCKills;
        int totalChickens;

        SoundEffect laserSound;
        SoundEffect explosionSound;
        Song gameplayMusic;



		#endregion

		public Steampunk()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			// Initialize the player class
			player = new Player();
            shield = new OverShield();

			// Set a constant player move speed
			playerMoveSpeed = 25.0f;

			bgLayer1 = new Background();
			bgLayer2 = new Background();

			// Initialize the enemies list
			enemies = new List<Enemy>();
			chickens = new List<Chicken>();
            //shields = new List<OverShield>();

			// Set the time keepers to zero
			previousEnemySpawnTime = TimeSpan.Zero;
            previousChickenSpawnTime = TimeSpan.Zero;
            previousShieldPress = TimeSpan.Zero;

			// Used to determine how fast enemy respawns
			enemySpawnTime = TimeSpan.FromSeconds(.5f);
			chickenSpawnTime = TimeSpan.FromSeconds(1.0f);
            shieldRate = TimeSpan.FromSeconds(.1f);

			// Initialize our random number generator
			random = new Random();

			projectiles = new List<Projectile>();

			// Set the laser to fire every quarter second
			fireTime = TimeSpan.FromSeconds(.01f);

			explosions = new List<Animation>();

			//Set player's score to zero
			score = 0;
            totalChickens = 0;
            playerCKills = 0;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load the player resources
			Animation playerAnimation = new Animation();
            Animation shieldedAnimation = new Animation();
            Texture2D shieldedTexture = Content.Load<Texture2D>("Animation/ShieldedAnimation");
			Texture2D playerTexture = Content.Load<Texture2D>("Animation/shipAnimation");
			playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);
            shieldedAnimation.Initialize(shieldedTexture, Vector2.Zero, 150, 100, 8, 50, Color.White, 1f, true);

			Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
			+ GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation,shieldedAnimation, playerPosition);
           // player.Initialize(shieldedAnimation, playerPosition);

			bgLayer1.Initialize(Content, "Texture/bgLayer1", GraphicsDevice.Viewport.Width, -1);
			bgLayer2.Initialize(Content, "Texture/bgLayer2", GraphicsDevice.Viewport.Width, -2);
			enemyTexture = Content.Load<Texture2D>("Animation/mineAnimation");
			chickenTexture = Content.Load<Texture2D>("Animation/cuccoFly");
			projectileTexture = Content.Load<Texture2D>("Texture/laser");
			mainBackground = Content.Load<Texture2D>("Texture/mainbackground");
			explosionTexture = Content.Load<Texture2D>("Animation/explosion");

            gameplayMusic = Content.Load<Song>("Sound/gameMusic");
           laserSound = Content.Load <SoundEffect> ("Sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("Sound/explosion");

            overshieldTexture = Content.Load<Texture2D>("Texture/shield");
            overshieldGenTexture = Content.Load<Texture2D>("Animation/shieldGen");
            overshieldDeathTexture = Content.Load<Texture2D>("Animation/shieldDeath");

			// Load the score font
			font = Content.Load<SpriteFont>("Font/gameFont");

            PlayMusic(gameplayMusic);
		}

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);


            //Update the player
            UpdatePlayer(gameTime);

            // Update the parallaxing background
            bgLayer1.Update();
            bgLayer2.Update();

            // Update the enemies
            UpdateEnemies(gameTime);
            UpdateChickens(gameTime);

            // Update the collision
            UpdateCollision();

            // Update the projectiles
            UpdateProjectiles();

            // Update the explosions
            UpdateExplosions(gameTime);

            base.Update(gameTime);
        }
		private void PlayMusic(Song song)
		{
			// Due to the way the MediaPlayer plays music,
			// we have to catch the exception. Music will play when the game is not tethered
			try
			{
				// Play the music
				MediaPlayer.Play(song);

				// Loop the currently playing song
				MediaPlayer.IsRepeating = true;
			}
			catch { } //No Exception is handled so it is an empty/anonymous exception
		}

		private void UpdatePlayer(GameTime gameTime)
		{
			player.Update(gameTime);

			// Get Thumbstick Controls
			player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
			player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

			// Use the Keyboard / Dpad
			if (currentKeyboardState.IsKeyDown(Keys.Left) ||
			currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				player.Position.X -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Right) ||
			currentGamePadState.DPad.Right == ButtonState.Pressed)
			{
				player.Position.X += playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Up) ||
			currentGamePadState.DPad.Up == ButtonState.Pressed)
			{
				player.Position.Y -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Down) ||
			currentGamePadState.DPad.Down == ButtonState.Pressed)
			{
				player.Position.Y += playerMoveSpeed;
			}
            if (currentKeyboardState.IsKeyDown(Keys.RightShift) && gameTime.TotalGameTime - previousShieldPress > shieldRate)
			{
                previousShieldPress = gameTime.TotalGameTime;

				if (player.IsShielded)
				{
					player.IsShielded = false;
				}
				else if (!player.IsShielded)
				{
					player.IsShielded = true;
				}
			}

			// Make sure that the player does not go out of bounds
			player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

			// Fire only every interval we set as the fireTime
            if (gameTime.TotalGameTime - previousFireTime > fireTime && !player.IsShielded)
			{
				// Reset our current time
				previousFireTime = gameTime.TotalGameTime;

					AddProjectile(player.Position + new Vector2(player.Width / 2, 20));
                    //laserSound.Play();

					AddProjectile(player.Position + new Vector2(player.Width / 2, -20));
                    //laserSound.Play();

					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
				//lasovershoversherSound.Play();
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Height / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-100, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-100, 500));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-100, -500));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 20));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, -20));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
				//laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Height / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-500, 0));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-500, 500));
                //laserSound.Play();
					AddProjectile(player.Position + new Vector2(-500, -500));
               //laserSound.Play();
			}

			// reset score if player health goes to zero
			if (player.Health <= 0)
			{
				player.Health = 100;
				score = 0;
			}
		}

        //private void AddShield()
        //{
        //    Animation overshieldAnimation = new Animation();
        //    Animation overshieldDeathAnimation = new Animation();
        //    Animation overshieldGenAnimation = new Animation();

        //    overshieldAnimation.Initialize(overshieldTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);
        //    overshieldDeathAnimation.Initialize(overshieldGenTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);
        //    overshieldGenAnimation.Initialize(overshieldDeathTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

        //    Vector2 position = new Vector2(player.Position.X, player.Position.Y);

        //    OverShield shield = new OverShield();
        //    shield.Initialize(overshieldAnimation, position);

        //    shields.Add(shield);
        //}

   //     private void UpdateShield()
   //     {
			//// Use the Keyboard / Dpad
			//if (currentKeyboardState.IsKeyDown(Keys.Left) ||
			//currentGamePadState.DPad.Left == ButtonState.Pressed)
			//{
			//	shield.Position.X -= playerMoveSpeed;
			//}
			//if (currentKeyboardState.IsKeyDown(Keys.Right) ||
			//currentGamePadState.DPad.Right == ButtonState.Pressed)
			//{
			//	shield.Position.X += playerMoveSpeed;
			//}
			//if (currentKeyboardState.IsKeyDown(Keys.Up) ||
			//currentGamePadState.DPad.Up == ButtonState.Pressed)
			//{
			//	shield.Position.Y -= playerMoveSpeed;
			//}
			//if (currentKeyboardState.IsKeyDown(Keys.Down) ||
			//currentGamePadState.DPad.Down == ButtonState.Pressed)
			//{
			//	shield.Position.Y += playerMoveSpeed;
			//}
   //         if (currentKeyboardState.IsKeyDown(Keys.RightShift) ||
			//currentGamePadState.DPad.Down == ButtonState.Pressed)
			//{
   //             if(isShielded)
   //             {
   //                 isShielded = false;
   //             }
   //             else if(!isShielded)
   //             {
   //                 isShielded = true;
   //             }
			//}

			//// Make sure that the player does not go out of bounds
			//shield.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
			//shield.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

   //         if(isShielded)
   //         {
			//	    Animation overshieldAnimation = new Animation();
			//	    Animation overshieldDeathAnimation = new Animation();
			//	    Animation overshieldGenAnimation = new Animation();

			//	    overshieldAnimation.Initialize(overshieldTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);
			//	    overshieldDeathAnimation.Initialize(overshieldGenTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);
			//	    overshieldGenAnimation.Initialize(overshieldDeathTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

			//	    Vector2 position = new Vector2(player.Position.X, player.Position.Y);

			//	    shield.Initialize(overshieldAnimation, position);

   //             shield.Draw(spriteBatch);
			//}
        //}

		private void AddEnemy()
		{
			// Create the animation object
			Animation enemyAnimation = new Animation();

			// Initialize the animation with the correct animation information
			enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

			// Randomly generate the position of the enemy
			Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

			// Create an enemy
			Enemy enemy = new Enemy();

			// Initialize the enemy
			enemy.Initialize(enemyAnimation, position);

			// Add the enemy to the active enemies list
			enemies.Add(enemy);
		}

		private void AddChickens()
		{
			// Create the animation object
			Animation chickenAnimation = new Animation();

			// Initialize the animation with the correct animation information
			chickenAnimation.Initialize(chickenTexture, Vector2.Zero, chickenTexture.Width/2, chickenTexture.Height, 2, 50, Color.White, 2f, true);

			// Randomly generate the position of the enemy
            Vector2 position = new Vector2(random.Next(400, GraphicsDevice.Viewport.Width - 100), GraphicsDevice.Viewport.Height - 490);

			// Create an enemy
			Chicken chicken = new Chicken();

			// Initialize the enemy
			chicken.Initialize(chickenAnimation, position);

			// Add the enemy to the active enemies list
			chickens.Add(chicken);

            totalChickens++;

            //if(playerCKills >= 10)
            //{
            //    chickens.Add(chicken);
            //    totalChickens++;
            //}
		}

        private void AddChickens2(int index)
        {
            Animation chickenAnimation = new Animation();

            chickenAnimation.Initialize(chickenTexture, Vector2.Zero, chickenTexture.Width / 2, chickenTexture.Height, 2, 50, Color.White, 2f, true);

            Vector2 position = new Vector2(chickens[index].Position.X + 50,chickens[index].Position.Y + 50);
            Vector2 position1 = new Vector2(chickens[index].Position.X - 50, chickens[index].Position.Y - 50);
            Vector2 position2 = new Vector2(chickens[index].Position.X + 50, chickens[index].Position.Y - 50);
            Vector2 position3 = new Vector2(chickens[index].Position.X - 50, chickens[index].Position.Y + 50);

            Chicken chicken = new Chicken();
            Chicken chicken1 = new Chicken();
            Chicken chicken2 = new Chicken();
            Chicken chicken3 = new Chicken();

            chicken.Initialize(chickenAnimation, position);
			chicken1.Initialize(chickenAnimation, position1);
			chicken2.Initialize(chickenAnimation, position2);
			chicken3.Initialize(chickenAnimation, position3);

            chickens.Add(chicken);
            chickens.Add(chicken1);
            chickens.Add(chicken2);
            chickens.Add(chicken3);

        }
		private void UpdateEnemies(GameTime gameTime)
		{
			// Spawn a new enemy enemy every 1.5 seconds
			if (gameTime.TotalGameTime - previousEnemySpawnTime > enemySpawnTime)
			{
				previousEnemySpawnTime = gameTime.TotalGameTime;

				// Add an Enemy
				AddEnemy();
				


			}

			// Update the Enemies
			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				enemies[i].Update(gameTime);

				if (enemies[i].Active == false)
				{
					// If not active and health <= 0
					if (enemies[i].Health <= 0)
					{
						// Add an explosion
						AddExplosion(enemies[i].Position);
                        //explosionSound.Play();
						//Add to the player's score
						score += enemies[i].Score;
					}
					enemies.RemoveAt(i);
				}
			}
		}

		private void UpdateChickens(GameTime gameTime)
		{
			// Spawn a new enemy enemy every 1.5 seconds
			if (gameTime.TotalGameTime - previousChickenSpawnTime > chickenSpawnTime)
			{
				previousChickenSpawnTime = gameTime.TotalGameTime;

				// Add an Enemy
				AddChickens();



			}

			// Update the Enemies
			for (int i = chickens.Count - 1; i >= 0; i--)
			{
				chickens[i].Update(gameTime);

				if (chickens[i].Active == false)
				{
                    totalChickens--;
					// If not active and health <= 0
					if (chickens[i].Health <= 0)
					{
						
						// Add an explosion
						AddExplosion(chickens[i].Position);
                        //Add to the player's score
                        if (totalChickens < 100)
                        {
                            AddChickens2(i);
                            totalChickens += 4;
                        }

						score += chickens[i].Score;
                       
                        playerCKills++;
					}
					chickens.RemoveAt(i);
				}
			}
		}

		private void UpdateCollision()
		{
			// Use the Rectangle's built-in intersect function to 
			// determine if two objects are overlapping
			Rectangle rectanglePlayer;
			Rectangle rectangleMine;
			Rectangle rectangleChicken;
			Rectangle rectangleLaser;

			// Only create the rectangle once for the player
			rectanglePlayer = new Rectangle((int)player.Position.X,
			(int)player.Position.Y,
			player.Width,
			player.Height);

			// Do the collision between the player and the enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				rectangleMine = new Rectangle((int)enemies[i].Position.X,
				(int)enemies[i].Position.Y,
				enemies[i].Width,
				enemies[i].Height);

				// Determine if the two objects collided with each
				// other
                if (rectanglePlayer.Intersects(rectangleMine) && !player.IsShielded)
				{
					// Subtract the health from the player based on
					// the enemy damage
					player.Health -= enemies[i].Damage;

					// Since the enemy collided with the player
					// destroy it
					enemies[i].Health = 0;

					// If the player health is less than zero we died
					if (player.Health <= 0)
						player.Active = false;
				}
			}
			// Projectile vs Enemy Collision
			for (int i = 0; i < projectiles.Count; i++)
			{
				for (int j = 0; j < enemies.Count; j++)
				{
					// Create the rectangles we need to determine if we collided with each other
					rectangleLaser = new Rectangle((int)projectiles[i].Position.X -
					projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
					projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

					rectangleMine = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
					(int)enemies[j].Position.Y - enemies[j].Height / 2,
					enemies[j].Width, enemies[j].Height);

					// Determine if the two objects collided with each other
					if (rectangleLaser.Intersects(rectangleMine))
					{
						enemies[j].Health -= projectiles[i].Damage;
						projectiles[i].Active = false;
					}
				}
			}

			// Do the collision between the player and the enemies
			for (int i = 0; i < chickens.Count; i++)
			{
				rectangleChicken = new Rectangle((int)chickens[i].Position.X,
				(int)chickens[i].Position.Y,
				chickens[i].Width,
				chickens[i].Height);

				// Determine if the two objects collided with each
				// other
				if (rectanglePlayer.Intersects(rectangleChicken) && !player.IsShielded)
				{
					// Subtract the health from the player based on
					// the enemy damage
					player.Health -= chickens[i].Damage;

					// Since the enemy collided with the player
					// destroy it
					chickens[i].Health = 0;

					// If the player health is less than zero we died
					if (player.Health <= 0)
						player.Active = false;
                    playerCKills++;
				}
			}
			// Projectile vs Enemy Collision
			for (int i = 0; i < projectiles.Count; i++)
			{
				for (int j = 0; j < chickens.Count; j++)
				{
					// Create the rectangles we need to determine if we collided with each other
					rectangleLaser = new Rectangle((int)projectiles[i].Position.X -
					projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
					projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

					rectangleChicken = new Rectangle((int)chickens[j].Position.X - chickens[j].Width / 2,
					(int)chickens[j].Position.Y - chickens[j].Height / 2,
					chickens[j].Width, chickens[j].Height);

					// Determine if the two objects collided with each other
					if (rectangleLaser.Intersects(rectangleChicken))
					{
						chickens[j].Health -= projectiles[i].Damage;
						projectiles[i].Active = false;


					}
				}
			}
		}





		private void AddProjectile(Vector2 position)
		{
			Projectile projectile = new Projectile();
			projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
			projectiles.Add(projectile);
		}

		private void UpdateProjectiles()
		{
			// Update the Projectiles
			for (int i = projectiles.Count - 1; i >= 0; i--)
			{
				projectiles[i].Update();

				if (projectiles[i].Active == false)
				{
					projectiles.RemoveAt(i);
				}

			}
		}

		private void AddExplosion(Vector2 position)
		{
			Animation explosion = new Animation();
			explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
			explosions.Add(explosion);
		}

		private void UpdateExplosions(GameTime gameTime)
		{
			for (int i = explosions.Count - 1; i >= 0; i--)
			{
				explosions[i].Update(gameTime);
				if (explosions[i].Active == false)
				{
					explosions.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.Fuchsia);

			// Start drawing
			spriteBatch.Begin();

			spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

			// Draw the moving background
			bgLayer1.Draw(spriteBatch);
			bgLayer2.Draw(spriteBatch);

			// Draw the Enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].Draw(spriteBatch);
			}

			for (int i = 0; i<chickens.Count; i++)
			{
				chickens[i].Draw(spriteBatch);
			}

            // Draw the Player
            player.Draw(spriteBatch);

			// Draw the Projectiles
			for (int i = 0; i < projectiles.Count; i++)
			{
				projectiles[i].Draw(spriteBatch);
			}

			// Draw the explosions
			for (int i = 0; i < explosions.Count; i++)
			{
				explosions[i].Draw(spriteBatch);
			}

			// Draw the score
			spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
			// Draw the player health
			spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

			// Stop drawing
			spriteBatch.End();

			base.Draw(gameTime);
		}

	}
}
