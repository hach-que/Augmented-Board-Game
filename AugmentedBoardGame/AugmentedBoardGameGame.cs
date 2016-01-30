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
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
            GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
            GraphicsDeviceManager.IsFullScreen = true;
            GraphicsDeviceManager.ApplyChanges();
        }
    }
}
