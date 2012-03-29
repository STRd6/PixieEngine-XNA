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
        Texture2D solidTexture;
        SpriteBatch spriteBatch;
        JavaScriptContext js = new JavaScriptContext();
        Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        Dictionary<string, Song> music = new Dictionary<string, Song>();
        Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();
        KeyboardState keyboardState;
        Keys k = new Keys();

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
            InitGraphicsMode(480, 320, false);

            this.TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000 / 30f); 

            // TODO: Add your initialization logic here
            js.RunVoid(ReadJS("json2"));
            js.RunVoid(ReadJS("gamelib"));
            js.RunVoid(ReadJS("XNA-Shiv"));

            js.SetParameter("log", (Action<object>)((o) => { if (o != null) { log(o.ToString()); } else { log("null"); } }));
            js.SetParameter("clear", (Action<double, double, double>)((r, g, b) => { GraphicsDevice.Clear(new Color(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b))); }));
            js.Run("color = 0");

            js.SetParameter("Sound", new {
                play = (Action<string>)((name) => {
                    sounds[name].Play();
                })
            });

            js.SetParameter("Music", new {
                play = (Action<string>)((name) => {
                    Song s = music[name];
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(s);
                })
            });

            js.SetParameter("XNA_Sprite", new
            {
                loadByName = (Func<string, Texture2D>)
                ((name) => {
                    return sprites[name];
                })
            });

            SpriteFont spriteFont = Content.Load<SpriteFont>("SpriteFont1");
            js.SetParameter("XNA_Canvas", new
            {
                clear = (Action)(() => {
                    GraphicsDevice.Clear(Color.Black);
                }),
                fill = (Action<int, int, int, int>)((r, g, b, a) => {
                    GraphicsDevice.Clear(new Color(r, g, b, a));
                }),
                fillRect = (Action<double, double, double, double, int, int, int, int>)((x, y, width, height, r, g, b, a) => {
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
                    spriteBatch.Draw(solidTexture, new Rectangle((int)x, (int)y, (int)width, (int)height), new Color(r, g, b, a));
                    spriteBatch.End();
                }),
                fillTiledRect = (Action<Texture2D, double, double, double, double, double, double, double, double>)
                ((texture, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) => {
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
                    
                    Rectangle destRect = new Rectangle((int)destX, (int)destY, (int)destWidth, (int)destHeight);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle((int)destX, (int)destY, (int)destWidth, (int)destHeight),
                        new Rectangle((int)sourceX, (int)sourceY, (int)sourceWidth, (int)sourceHeight),
                        Color.White
                    );

                    spriteBatch.End();
                }),
                fillText = (Action<string, double, double, int, int, int, int>)((text, x, y, r, g, b, a) => {
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                    spriteBatch.DrawString(spriteFont, text, new Vector2((float)x, (float)y), new Color(r, g, b, a));
                    spriteBatch.End();
                }),
                drawImage = (Action<Texture2D, double, double, double, double, double, double, double, double>)
                ((texture, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) =>
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle((int)destX, (int)destY, (int)destWidth, (int)destHeight),
                        new Rectangle((int)sourceX, (int)sourceY, (int)sourceWidth, (int)sourceHeight),
                        Color.White
                    );
                    spriteBatch.End();
                }),
            });

            js.SetParameter("XNA_Keyboard", new {
                keyDown = (Func<string, bool>)((name) => {
                    return keyboardState.IsKeyDown((Keys)Enum.Parse(typeof(Keys), name, false));
                }),
            });

            js.RunVoid(ReadJS("surfn_config"));

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

            solidTexture = new Texture2D(GraphicsDevice, 1, 1);
            solidTexture.SetData(new Color[] { Color.White });

            // TODO: use this.Content to load your game content here

            // Load all images as sprites
            DirectoryInfo directory = new DirectoryInfo(Content.RootDirectory + "\\images");
            FileInfo[] files = directory.GetFiles("*.*");
            foreach (FileInfo file in files) {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                log("Loading sprite: " + name);
                sprites[name] = Content.Load<Texture2D>("images\\" + name);
            }

            // Load all sounds
            directory = new DirectoryInfo(Content.RootDirectory + "\\sounds");
            files = directory.GetFiles("*.*");
            foreach (FileInfo file in files) {
                string name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Extension == ".wav") {
                    log("Loading sound: " + name);
                    try {
                        sounds[name] = SoundEffect.FromStream(file.OpenRead());
                    } catch (Exception) {}
                }

                // TODO: load mp3s for real, currently using content pipeline and wma files
                if (file.Extension == ".mp3" || file.Extension == ".wma") {
                    log("Loading music: " + name);
                    music[name] = Content.Load<Song>("sounds\\" + name);
                }
            }

            // Load the game code after all the resources have been loaded
            js.RunVoid(ReadJS("SurfN_game"));
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
            keyboardState = Keyboard.GetState();

            // Allows the game to exit
            if (currentState.Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            js.RunVoid("engine.update();");

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            

            js.RunVoid("engine.draw();");

            // js.RunVoid("canvas.drawImage(img, 0, 0, width, height, posX, posY, width, height);");
            //spriteBatch.Draw(myTexture, spritePosition, Color.White);

            

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
            textOut.WriteLine(System.DateTime.Now.ToLongTimeString() + " -- " + msg);
            textOut.Close();
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Itterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
