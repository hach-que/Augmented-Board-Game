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

            GraphicsDeviceManager.PreferredBackBufferWidth = 1024;
            GraphicsDeviceManager.PreferredBackBufferHeight = 768;
            //GraphicsDeviceManager.IsFullScreen = false;
            GraphicsDeviceManager.ApplyChanges();
        }
    }
}
