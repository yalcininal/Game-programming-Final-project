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

        Texture2D pixel;
        SpriteFont font; 

        const int TILE_SIZE = 20;
        const int GRID_WIDTH = 30;
        const int GRID_HEIGHT = 20;
        float moveDelay = 0.12f;
        float moveTimer = 0f;


        List<Point> snake;
        Point direction;    
        Point food;
        bool gameOver = false;
        int score = 0;

        Random rng;

        KeyboardState prevKeyboard;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = GRID_WIDTH * TILE_SIZE;
            graphics.PreferredBackBufferHeight = GRID_HEIGHT * TILE_SIZE;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            rng = new Random();

            snake = new List<Point>
            {
                new Point(GRID_WIDTH / 2, GRID_HEIGHT / 2),
                new Point(GRID_WIDTH / 2 - 1, GRID_HEIGHT / 2),
                new Point(GRID_WIDTH / 2 - 2, GRID_HEIGHT / 2)
            };

            direction = new Point(1, 0); 
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

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            var kb = Keyboard.GetState();

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

            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= moveDelay)
            {
                moveTimer -= moveDelay;
                Step();
            }

            prevKeyboard = kb;
            base.Update(gameTime);
        }

        void Step()
        {

            Point head = snake[0];
            Point newHead = new Point(head.X + direction.X, head.Y + direction.Y);

            if (newHead.X < 0 || newHead.X >= GRID_WIDTH || newHead.Y < 0 || newHead.Y >= GRID_HEIGHT)
            {
                gameOver = true;
                return;
            }

            for (int i = 0; i < snake.Count; i++)
            {
                if (snake[i] == newHead)
                {
                    gameOver = true;
                    return;
                }
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                score += 10;
                SpawnFood();

            }
            else
            {

                snake.RemoveAt(snake.Count - 1);
            }
        }

        void SpawnFood()
        {

            Point candidate;
            int attempts = 0;
            do
            {
                candidate = new Point(rng.Next(0, GRID_WIDTH), rng.Next(0, GRID_HEIGHT));
                attempts++;

                if (attempts > 1000) break;
            } while (snake.Contains(candidate));

            food = candidate;
        }

        void ResetGame()
        {

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


            foreach (var segment in snake)
            {
                var rect = new Rectangle(segment.X * TILE_SIZE, segment.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                spriteBatch.Draw(pixel, rect, Color.Green);
            }


            var foodRect = new Rectangle(food.X * TILE_SIZE, food.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            spriteBatch.Draw(pixel, foodRect, Color.Red);


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
