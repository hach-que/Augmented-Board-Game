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
    public class BoardRendererEntity : Entity
    {
        private readonly BoardAnalyzerEntity _analyzerEntity;
        private readonly WebcamEntity _webcamEntity;
        private readonly IGraphicsBlit _graphicsBlit;
        private readonly TextBox _alphaTextBox;
        private EffectAsset _warpFromPolygonEffect;

        private Vector2? _cachedTopLeft;
        private Vector2? _cachedTopRight;
        private Vector2? _cachedBottomLeft;
        private Vector2? _cachedBottomRight;

        private float _alpha = 0.5f;

        public BoardRendererEntity(
            BoardAnalyzerEntity analyzerEntity,
            WebcamEntity webcamEntity, 
            IGraphicsBlit graphicsBlit, 
            IAssetManager assetManager,
            TextBox alphaTextBox)
        {
            _analyzerEntity = analyzerEntity;
            _webcamEntity = webcamEntity;
            _graphicsBlit = graphicsBlit;
            _alphaTextBox = alphaTextBox;
            _warpFromPolygonEffect = assetManager.Get<EffectAsset>("effect.WarpFromPolygon");
        }

        public override void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            base.Update(gameContext, updateContext);

            try
            {
                if (!string.IsNullOrWhiteSpace(_alphaTextBox.Text))
                {
                    _alpha = float.Parse(_alphaTextBox.Text);
                }
            }
            catch
            {
            }
        }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            if (renderContext.IsCurrentRenderPass<I2DDirectRenderPass>())
            {
                if (_analyzerEntity.TopLeftNormalized != null && _analyzerEntity.TopRightNormalized != null &&
                    _analyzerEntity.BottomLeftNormalized != null && _analyzerEntity.BottomRightNormalized != null)
                {
                    _cachedTopLeft = _analyzerEntity.TopLeftNormalized;
                    _cachedTopRight = _analyzerEntity.TopRightNormalized;
                    _cachedBottomLeft = _analyzerEntity.BottomLeftNormalized;
                    _cachedBottomRight = _analyzerEntity.BottomRightNormalized;
                }

                if (_cachedTopLeft != null && _cachedTopRight != null &&
                    _cachedBottomLeft != null && _cachedBottomRight != null)
                { 
                    _warpFromPolygonEffect.Effect.Parameters["TopLeft"].SetValue(_cachedTopLeft.Value);
                    _warpFromPolygonEffect.Effect.Parameters["TopRight"].SetValue(_cachedTopRight.Value);
                    _warpFromPolygonEffect.Effect.Parameters["BottomLeft"].SetValue(_cachedBottomLeft.Value);
                    _warpFromPolygonEffect.Effect.Parameters["BottomRight"].SetValue(_cachedBottomRight.Value);
                    _warpFromPolygonEffect.Effect.Parameters["Alpha"].SetValue(_alpha);

                    _graphicsBlit.Blit(
                        renderContext,
                        _webcamEntity.VideoCaptureFrame,
                        null,
                        _warpFromPolygonEffect.Effect,
                        BlendState.AlphaBlend);
                }
            }
        }
    }
}
