using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.IO;
using Noesis.Javascript;

namespace Test_Windows_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SoundEffect soundEffect;
        JavaScriptContext js = new JavaScriptContext();
        Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();

        // This is a texture we can render.
        Texture2D myTexture;

        // Set the coordinates to draw the sprite at.
        Vector2 spritePosition = new Vector2(100, 100);
        Vector2 spriteSpeed = new Vector2(30, 90);

        public Game1()
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
            js.RunVoid(ReadJS("json2"));
            js.RunVoid(ReadJS("gamelib"));
            js.RunVoid(ReadJS("XNA-Shiv"));

            js.SetParameter("log", (Action<object>)((o) => { if (o != null) { log(o.ToString()); } else { log("null"); } }));
            js.SetParameter("clear", (Action<double, double, double>)((r, g, b) => { GraphicsDevice.Clear(new Color(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b))); }));
            js.Run("color = 0");

            js.SetParameter("Sound", new {
                play = (Action<string>)((name) => { sounds[name].Play(); })
            });

            js.SetParameter("__XNA__Sprite", new
            {
                loadByName = (Func<string, Texture2D>)
                ((name) => {
                    log("Loading sprite: " + name);
                    return sprites[name];
                })
            });

            js.SetParameter("__XNA__Canvas", new
            {
                drawImage = (Action<Texture2D, int, int, int, int, int, int, int, int>)
                ((texture, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) =>
                {
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(destX, destY, destWidth, destHeight),
                        new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                        Color.White
                    );
                }),
            });

            js.RunVoid("canvas = Canvas();");

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

            // TODO: use this.Content to load your game content here
            sprites["mcGriff"] = myTexture = Content.Load<Texture2D>("McGriffLogo");

            js.SetParameter("img", myTexture);
            js.RunVoid("mcGriff = GameObject({x: 500, y: 50, sprite: 'mcGriff'});");

            sounds["clink0"] = soundEffect = Content.Load<SoundEffect>("clink0");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (currentState.Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            // Move the sprite by speed, scaled by elapsed time.
            spritePosition +=
                spriteSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int MaxX =
                graphics.GraphicsDevice.Viewport.Width - myTexture.Width;
            int MinX = 0;
            int MaxY =
                graphics.GraphicsDevice.Viewport.Height - myTexture.Height;
            int MinY = 0;

            // Check for bounce.
            if (spritePosition.X > MaxX)
            {
                spriteSpeed.X *= -1;
                spritePosition.X = MaxX;
                soundEffect.Play();
            }

            else if (spritePosition.X < MinX)
            {
                spriteSpeed.X *= -1;
                spritePosition.X = MinX;
                soundEffect.Play();
            }

            if (spritePosition.Y > MaxY)
            {
                spriteSpeed.Y *= -1;
                spritePosition.Y = MaxY;
                soundEffect.Play();
            }

            else if (spritePosition.Y < MinY)
            {
                spriteSpeed.Y *= -1;
                spritePosition.Y = MinY;
                soundEffect.Play();
            }

            js.SetParameter("width", myTexture.Width);
            js.SetParameter("height", myTexture.Height);
            js.SetParameter("posX", (int)spritePosition.X);
            js.SetParameter("posY", (int)spritePosition.Y);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //int c = Convert.ToInt32(JavaScript.Run("return color += 1;"));
            js.Run("clear(0, 0, color);");

            Matrix matrix = new Matrix(
                1, 0, 0, 0, 
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, matrix);

            js.RunVoid("mcGriff.draw(canvas);");

            js.RunVoid("canvas.drawImage(img, 0, 0, width, height, posX, posY, width, height);");
            //spriteBatch.Draw(myTexture, spritePosition, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        static string ReadJS(string filename)
        {
            StreamReader streamReader = new StreamReader("scripts/" + filename + ".js");
            string script = streamReader.ReadToEnd();
            streamReader.Close();

            return script;
        }

        static void log(string msg) { 
            StreamWriter textOut = new StreamWriter(new FileStream("log.txt", FileMode.Append, FileAccess.Write));
            textOut.WriteLine(System.DateTime.Now.ToLongTimeString() + msg);
            textOut.Close();
        }
    }
}
