using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SnakeMonoGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Rendering helpers
        Texture2D pixel;
        SpriteFont font; // optional - add DefaultFont.spritefont to Content to enable text

        // Game grid & timing
        const int TILE_SIZE = 20;
        const int GRID_WIDTH = 30;
        const int GRID_HEIGHT = 20;
        float moveDelay = 0.12f;
        float moveTimer = 0f;

        // Game state
        List<Point> snake;
        Point direction;    // grid direction: (-1,0),(1,0),(0,-1),(0,1)
        Point food;
        bool gameOver = false;
        int score = 0;

        // Random generator
        Random rng;

        // Input state to prevent multiple direction changes between moves
        KeyboardState prevKeyboard;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set window size to match grid
            graphics.PreferredBackBufferWidth = GRID_WIDTH * TILE_SIZE;
            graphics.PreferredBackBufferHeight = GRID_HEIGHT * TILE_SIZE;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            rng = new Random();

            // Initialize snake in the middle of the grid (3 segments)
            snake = new List<Point>
            {
                new Point(GRID_WIDTH / 2, GRID_HEIGHT / 2),
                new Point(GRID_WIDTH / 2 - 1, GRID_HEIGHT / 2),
                new Point(GRID_WIDTH / 2 - 2, GRID_HEIGHT / 2)
            };

            direction = new Point(1, 0); // moving right initially
            SpawnFood();
            score = 0;
            gameOver = false;
            moveTimer = 0f;
            prevKeyboard = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // 1x1 white pixel used to draw rectangles for snake & food
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Try loading font; if not present, font remains null and text won't be drawn.
            try
            {
                font = Content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                font = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit request
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            var kb = Keyboard.GetState();

            // If game over, allow restart with Enter
            if (gameOver)
            {
                if (IsKeyPressed(kb, Keys.Enter))
                {
                    ResetGame();
                }

                prevKeyboard = kb;
                base.Update(gameTime);
                return;
            }

            // Read input and queue direction changes.
            // Only allow a single direction change per move tick (prevents multiple turns before movement).
            if (IsKeyPressed(kb, Keys.Up) && direction != new Point(0, 1))
            {
                direction = new Point(0, -1);
            }
            else if (IsKeyPressed(kb, Keys.Down) && direction != new Point(0, -1))
            {
                direction = new Point(0, 1);
            }
            else if (IsKeyPressed(kb, Keys.Left) && direction != new Point(1, 0))
            {
                direction = new Point(-1, 0);
            }
            else if (IsKeyPressed(kb, Keys.Right) && direction != new Point(-1, 0))
            {
                direction = new Point(1, 0);
            }

            // Advance timer and move when appropriate
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= moveDelay)
            {
                moveTimer -= moveDelay;
                Step();
            }

            prevKeyboard = kb;
            base.Update(gameTime);
        }

        // Single game step: move snake, handle collisions and food.
        void Step()
        {
            // Compute new head position
            Point head = snake[0];
            Point newHead = new Point(head.X + direction.X, head.Y + direction.Y);

            // Check wall collision
            if (newHead.X < 0 || newHead.X >= GRID_WIDTH || newHead.Y < 0 || newHead.Y >= GRID_HEIGHT)
            {
                gameOver = true;
                return;
            }

            // Check self-collision
            for (int i = 0; i < snake.Count; i++)
            {
                if (snake[i] == newHead)
                {
                    gameOver = true;
                    return;
                }
            }

            // Insert new head
            snake.Insert(0, newHead);

            // Check food
            if (newHead == food)
            {
                score += 10;
                SpawnFood();
                // keep tail (snake grows)
            }
            else
            {
                // Remove tail (snake advances)
                snake.RemoveAt(snake.Count - 1);
            }
        }

        void SpawnFood()
        {
            // Choose a position not occupied by the snake
            Point candidate;
            int attempts = 0;
            do
            {
                candidate = new Point(rng.Next(0, GRID_WIDTH), rng.Next(0, GRID_HEIGHT));
                attempts++;
                // if grid is nearly full, break to avoid infinite loop
                if (attempts > 1000) break;
            } while (snake.Contains(candidate));

            food = candidate;
        }

        void ResetGame()
        {
            // Reinitialize state
            snake.Clear();
            snake.Add(new Point(GRID_WIDTH / 2, GRID_HEIGHT / 2));
            snake.Add(new Point(GRID_WIDTH / 2 - 1, GRID_HEIGHT / 2));
            snake.Add(new Point(GRID_WIDTH / 2 - 2, GRID_HEIGHT / 2));
            direction = new Point(1, 0);
            score = 0;
            gameOver = false;
            moveTimer = 0f;
            SpawnFood();
        }

        bool IsKeyPressed(KeyboardState kb, Keys key)
        {
            return kb.IsKeyDown(key) && !prevKeyboard.IsKeyDown(key);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Draw snake
            foreach (var segment in snake)
            {
                var rect = new Rectangle(segment.X * TILE_SIZE, segment.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                spriteBatch.Draw(pixel, rect, Color.Green);
            }

            // Draw food
            var foodRect = new Rectangle(food.X * TILE_SIZE, food.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            spriteBatch.Draw(pixel, foodRect, Color.Red);

            // Draw score and messages if font loaded
            if (font != null)
            {
                spriteBatch.DrawString(font, $"Score: {score}", new Vector2(8, 8), Color.White);

                if (gameOver)
                {
                    string txt = "GAME OVER\nPress ENTER to restart";
                    Vector2 size = font.MeasureString(txt);
                    Vector2 pos = new Vector2(
                        (graphics.PreferredBackBufferWidth - size.X) / 2,
                        (graphics.PreferredBackBufferHeight - size.Y) / 2);
                    spriteBatch.DrawString(font, txt, pos, Color.Yellow);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
