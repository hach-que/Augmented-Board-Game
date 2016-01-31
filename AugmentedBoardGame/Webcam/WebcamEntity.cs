using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protogame;

namespace AugmentedBoardGame.Webcam
{
    public class WebcamEntity : Entity
    {
        private readonly I2DRenderUtilities _renderUtilities;

        private VideoCapture _videoCapture;

        public WebcamEntity(I2DRenderUtilities renderUtilities)
        {
            _renderUtilities = renderUtilities;
        }

        public Texture2D VideoCaptureFrame { get; set; }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            if (renderContext.IsFirstRenderPass())
            {
                if (_videoCapture == null)
                {
                    _videoCapture = new VideoCapture(renderContext.GraphicsDevice);
                }

                // Access this variable to update the webcam data.
                VideoCaptureFrame = _videoCapture.Frame;
            }
        }

        public byte[] UnlockedFrameRGBA
        {
            get
            {
                if (_videoCapture == null)
                {
                    return null;
                }

                return _videoCapture.UnlockedFrameRGBA;
            }
        }
    }
}
