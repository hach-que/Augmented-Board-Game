using System.IO;
using Microsoft.Xna.Framework;

namespace AugmentedBoardGame
{
    using Protoinject;

    using Protogame;

    public class AugmentedBoardGameGame : CoreGame<AugmentedBoardGameWorld>
    {
        public AugmentedBoardGameGame(IKernel kernel)
            : base(kernel)
        {
        }

        protected override void ConfigureRenderPipeline(IRenderPipeline pipeline, IKernel kernel)
        {
            pipeline.AddFixedRenderPass(kernel.Get<I2DBatchedRenderPass>());
            pipeline.AddFixedRenderPass(kernel.Get<I2DDirectRenderPass>());
            pipeline.AddFixedRenderPass(kernel.Get<ICanvasRenderPass>());
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if (File.Exists("settings.txt"))
            {
                using (var reader = new StreamReader("settings.txt"))
                {
                    GraphicsDeviceManager.PreferredBackBufferWidth = int.Parse(reader.ReadLine().Trim());
                    GraphicsDeviceManager.PreferredBackBufferHeight = int.Parse(reader.ReadLine().Trim());
                    GraphicsDeviceManager.IsFullScreen = reader.ReadLine() == "true";
                }
            }
            else
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = 1024;
                GraphicsDeviceManager.PreferredBackBufferHeight = 768;
                GraphicsDeviceManager.IsFullScreen = false;
            }


            GraphicsDeviceManager.ApplyChanges();
        }
    }
}
