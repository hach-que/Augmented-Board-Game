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
        private bool _hasSetupFullscreenButton;
        private Button _fullscreen;
        private bool _isFullscreen;

        public AugmentedBoardGameWorld(
            I2DRenderUtilities twoDRenderUtilities,
            IAssetManagerProvider assetManagerProvider,
            IEntityFactory entityFactory)
        {
            this.Entities = new List<IEntity>();

            _renderUtilities = twoDRenderUtilities;
            _assetManager = assetManagerProvider.GetAssetManager();
            _defaultFont = this._assetManager.Get<FontAsset>("font.Default");

            var canvasEntity = entityFactory.CreateCanvasEntity();

            var pointThresholdTextBox = new TextBox();
            var minPoints = new TextBox();
            var maxPoints = new TextBox();
            var alpha = new TextBox();
            var deviceWidth = new TextBox();
            var deviceHeight = new TextBox();
            var nextColor = new Button() {Text = "Show Next Color "};
            var showDiagnostics = new Button() { Text = "Show Diagnostics" };
            var showBoard = new Button() { Text = "Show Board" };
            var nextDevice = new Button() { Text = "Next Device" };
            var deviceName = new TextBox();
            _fullscreen = new Button() { Text = "Fullscreen 1080" };

            var camera640x480 = new Button() { Text = "Camera 640x480" };
            camera640x480.Click += (sender, args) =>
            {
                deviceWidth.Text = "640";
                deviceHeight.Text = "480";
            };
            var camera720p = new Button() { Text = "Camera 720p" };
            camera720p.Click += (sender, args) =>
            {
                deviceWidth.Text = "1080";
                deviceHeight.Text = "720";
            };
            var camera1080p = new Button() { Text = "Camera 1080p" };
            camera1080p.Click += (sender, args) =>
            {
                deviceWidth.Text = "1920";
                deviceHeight.Text = "1080";
            };

            var pointThresholdEntry = new HorizontalContainer();
            pointThresholdEntry.AddChild(new Label() { Text = "Point Threshold: "}, "100");
            pointThresholdEntry.AddChild(pointThresholdTextBox, "*");

            var minPointsEntry = new HorizontalContainer();
            minPointsEntry.AddChild(new Label() { Text = "Min. Points: " }, "100");
            minPointsEntry.AddChild(minPoints, "*");

            var maxPointsEntry = new HorizontalContainer();
            maxPointsEntry.AddChild(new Label() { Text = "Max. Points: " }, "100");
            maxPointsEntry.AddChild(maxPoints, "*");

            var renderAlphaEntry = new HorizontalContainer();
            renderAlphaEntry.AddChild(new Label() { Text = "Render Alpha: " }, "100");
            renderAlphaEntry.AddChild(alpha, "*");

            var deviceWidthEntry = new HorizontalContainer();
            deviceWidthEntry.AddChild(new Label() { Text = "Device Width: " }, "100");
            deviceWidthEntry.AddChild(deviceWidth, "*");

            var deviceHeightEntry = new HorizontalContainer();
            deviceHeightEntry.AddChild(new Label() { Text = "Device Height: " }, "100");
            deviceHeightEntry.AddChild(deviceHeight, "*");

            var vert = new VerticalContainer();
            vert.AddChild(pointThresholdEntry, "24");
            vert.AddChild(minPointsEntry, "24");
            vert.AddChild(maxPointsEntry, "24");
            vert.AddChild(renderAlphaEntry, "24");
            vert.AddChild(nextColor, "24");
            vert.AddChild(showDiagnostics, "48");
            vert.AddChild(showBoard, "48");
            vert.AddChild(nextDevice, "48");
            vert.AddChild(deviceName, "24");
            vert.AddChild(deviceWidthEntry, "24");
            vert.AddChild(deviceHeightEntry, "24");
            vert.AddChild(_fullscreen, "48");
            vert.AddChild(camera640x480, "48");
            vert.AddChild(camera720p, "48");
            vert.AddChild(camera1080p, "48");

            var hor = new HorizontalContainer();
            hor.AddChild(new EmptyContainer(), "*");
            hor.AddChild(vert, "200");

            var canvas = new Canvas();
            canvas.SetChild(hor);

            canvasEntity.Canvas = canvas;

            var webcamEntity = entityFactory.CreateWebcamEntity(deviceName, deviceWidth, deviceHeight);
            var detectorEntity = entityFactory.CreateDetectorEntity(webcamEntity);
            detectorEntity.X = 0;
            detectorEntity.Y = 0;
            var boardAnalyzerEntity = entityFactory.CreateBoardAnalyzerEntity(detectorEntity, pointThresholdTextBox, minPoints, maxPoints);
            var boardRendererEntity = entityFactory.CreateBoardRendererEntity(boardAnalyzerEntity, webcamEntity, alpha);

            nextColor.Click += (sender, args) =>
            {
                detectorEntity.NextColor();
            };
            showDiagnostics.Click += (sender, args) =>
            {
                alpha.Text = "0";
            };
            showBoard.Click += (sender, args) =>
            {
                alpha.Text = "1";
            };
            nextDevice.Click += (sender, args) =>
            {
                webcamEntity.NextDevice();
            };

            _hasSetupFullscreenButton = false;

            this.Entities.Add(webcamEntity);
            this.Entities.Add(detectorEntity);
            this.Entities.Add(boardAnalyzerEntity);
            this.Entities.Add(canvasEntity);
            this.Entities.Add(boardRendererEntity);
        }

        public IList<IEntity> Entities { get; private set; }

        public void Dispose()
        {
        }

        public void RenderAbove(IGameContext gameContext, IRenderContext renderContext)
        {
            if (!_hasSetupFullscreenButton)
            {
                _fullscreen.Click += (sender, args) =>
                {
                    if (_isFullscreen)
                    {
                        gameContext.Graphics.PreferredBackBufferWidth = 1024;
                        gameContext.Graphics.PreferredBackBufferHeight = 768;
                        gameContext.Graphics.IsFullScreen = false;
                        gameContext.Graphics.ApplyChanges();

                        _fullscreen.Text = "Fullscreen 1080";
                    }
                    else
                    {
                        gameContext.Graphics.PreferredBackBufferWidth = 1920;
                        gameContext.Graphics.PreferredBackBufferHeight = 1080;
                        gameContext.Graphics.IsFullScreen = false;
                        gameContext.Graphics.ApplyChanges();

                        _fullscreen.Text = "Windowed 1024";
                    }
                };
            }
        }

        public void RenderBelow(IGameContext gameContext, IRenderContext renderContext)
        {
            if (renderContext.IsFirstRenderPass())
            {
                gameContext.Graphics.GraphicsDevice.Clear(Color.Purple);
            }

            if (renderContext.IsCurrentRenderPass<I2DBatchedRenderPass>())
            {
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
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            gameContext.Game.IsMouseVisible = true;
        }
    }
}
