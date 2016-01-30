using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Protogame;

namespace AugmentedBoardGame.Webcam
{
    public class DetectorEntity : Entity
    {
        private readonly WebcamEntity _webcamEntity;
        private readonly IAssetManager _assetManager;
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly FontAsset _defaultFont;

        private Thread _thread;

        private int[,] _recogArray;

        private object _statusLock;
        private string _status;

        private const int imageWidth = 640;
        private const int imageHeight = 480;
        private const int chunkSize = 5;

        private float _sensitivity = 1f;

        public DetectorEntity(WebcamEntity webcamEntity, IAssetManager assetManager, I2DRenderUtilities renderUtilities)
        {
            _webcamEntity = webcamEntity;
            _assetManager = assetManager;
            _renderUtilities = renderUtilities;
            _defaultFont = _assetManager.Get<FontAsset>("font.Default");

            _recogArray = new int[imageWidth / chunkSize, imageHeight / chunkSize];

            _thread = new Thread(ProcessorThread);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public override void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            base.Update(gameContext, updateContext);

            _sensitivity *= 1.05f;
            if (_sensitivity > 100f)
            {
                _sensitivity = 1f;
            }
        }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            var widthScale = renderContext.GraphicsDevice.PresentationParameters.BackBufferWidth /
                             _recogArray.GetLength(0);
            var heightScale = renderContext.GraphicsDevice.PresentationParameters.BackBufferHeight /
                             _recogArray.GetLength(1);

            for (var x = 0; x < _recogArray.GetLength(0); x++)
            {
                for (var y = 0; y < _recogArray.GetLength(1); y++)
                {
                    Color col;
                    var score = _recogArray[x, y] / _sensitivity;
                    if (score < 0)
                    {
                        var scoreCapped = Math.Min(255, -score);
                        col = new Color(scoreCapped / 255f, scoreCapped / 255f, scoreCapped / 255f, 1f);
                    }
                    else
                    {
                        var scoreCapped = Math.Max(0, score);
                        col = new Color(scoreCapped / 255f, 0, 0, 1f);
                    }

                    _renderUtilities.RenderRectangle(
                        renderContext,
                        new Rectangle(
                            (int)(this.X + x * widthScale), (int)(this.Y + y * heightScale), widthScale, heightScale),
                            col, true);
                }
            }
        }

        private void ProcessorThread()
        {
            while (true)
            {
                var copy = _webcamEntity.UnlockedFrameRGBA;
                if (copy != null)
                {
                    for (var x = 0; x < imageWidth/chunkSize; x++)
                    {
                        for (var y = 0; y < imageHeight / chunkSize; y++)
                        {
                            var chunkScore = 0;
                            for (var xx = 0; xx < chunkSize; xx++)
                            {
                                for (var yy = 0; yy < chunkSize; yy++)
                                {
                                    var idx = ((x * chunkSize + xx) + (y * chunkSize + yy) * imageWidth) * 4;
                                    var r = copy[idx];
                                    var g = copy[idx + 1];
                                    var b = copy[idx + 2];

                                    chunkScore = chunkScore + r - (g + b)/2;

                                    _recogArray[x, y] = chunkScore;
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(20);
            }
        }
    }
}
