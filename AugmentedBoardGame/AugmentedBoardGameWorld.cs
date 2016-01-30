namespace AugmentedBoardGame
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    using Protogame;

    public class AugmentedBoardGameWorld : IWorld
    {
        private readonly I2DRenderUtilities _renderUtilities;

        private readonly IAssetManager _assetManager;

        private readonly FontAsset _defaultFont;
        
        public AugmentedBoardGameWorld(
            I2DRenderUtilities twoDRenderUtilities,
            IAssetManagerProvider assetManagerProvider,
            IEntityFactory entityFactory)
        {
            this.Entities = new List<IEntity>();

            _renderUtilities = twoDRenderUtilities;
            _assetManager = assetManagerProvider.GetAssetManager();
            _defaultFont = this._assetManager.Get<FontAsset>("font.Default");

            var webcamEntity = entityFactory.CreateWebcamEntity();
            var detectorEntity = entityFactory.CreateDetectorEntity(webcamEntity);
            detectorEntity.X = 0;
            detectorEntity.Y = 0;

            this.Entities.Add(webcamEntity);
            this.Entities.Add(detectorEntity);
        }

        public IList<IEntity> Entities { get; private set; }

        public void Dispose()
        {
        }

        public void RenderAbove(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void RenderBelow(IGameContext gameContext, IRenderContext renderContext)
        {
            gameContext.Graphics.GraphicsDevice.Clear(Color.Purple);

            this._renderUtilities.RenderText(
                renderContext,
                new Vector2(10, 10),
                "Hello AugmentedBoardGame!",
                this._defaultFont);

            this._renderUtilities.RenderText(
                renderContext,
                new Vector2(10, 30),
                "Running at " + gameContext.FPS + " FPS; " + gameContext.FrameCount + " frames counted so far",
                this._defaultFont);
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            gameContext.Game.IsMouseVisible = true;
        }
    }
}
