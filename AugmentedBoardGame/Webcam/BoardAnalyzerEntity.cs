using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Protogame;

namespace AugmentedBoardGame.Webcam
{
    public class BoardAnalyzerEntity : Entity
    {
        private readonly DetectorEntity _detectorEntity;

        private readonly I2DRenderUtilities _renderUtilities;

        private readonly Thread _thread;

        private const string _analyzerColor = "Red";

        private bool[,] _checkArray;

        private float _pointThreshold = 10f;

        private int _minNumberOfPoints = 1;

        private int _maxNumberOfPoints = 100;

        private readonly TextBox _pointThresholdTextBox;

        private TextBox _minNumberOfPointsTextBox;

        private TextBox _maxNumberOfPointsTextBox;

        private FontAsset _defaultFont;

        public BoardAnalyzerEntity(
            DetectorEntity detectorEntity, 
            TextBox pointThresholdTextBox, 
            TextBox minNumberOfPointsTextBox, 
            TextBox maxNumberOfPointsTextBox, 
            I2DRenderUtilities renderUtilities,
            IAssetManager assetManager)
        {
            _detectorEntity = detectorEntity;
            _pointThresholdTextBox = pointThresholdTextBox;
            _minNumberOfPointsTextBox = minNumberOfPointsTextBox;
            _maxNumberOfPointsTextBox = maxNumberOfPointsTextBox;
            _renderUtilities = renderUtilities;

            _pointThresholdTextBox.Text = _pointThreshold.ToString();
            _minNumberOfPointsTextBox.Text = _minNumberOfPoints.ToString();
            _maxNumberOfPointsTextBox.Text = _maxNumberOfPoints.ToString();

            _defaultFont = assetManager.Get<FontAsset>("font.Default");

            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public List<Vector2> DetectedPoints { get; set; }

        public Vector2? TopLeft { get; set; }

        public Vector2? TopRight { get; set; }

        public Vector2? BottomLeft { get; set; }

        public Vector2? BottomRight { get; set; }

        public Vector2? TopLeftNormalized { get; set; }

        public Vector2? TopRightNormalized { get; set; }

        public Vector2? BottomLeftNormalized { get; set; }

        public Vector2? BottomRightNormalized { get; set; }

        public override void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            base.Update(gameContext, updateContext);

            try
            {
                if (!string.IsNullOrWhiteSpace(_pointThresholdTextBox.Text))
                {
                    _pointThreshold = float.Parse(_pointThresholdTextBox.Text);
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_minNumberOfPointsTextBox.Text))
                {
                    _minNumberOfPoints = int.Parse(_minNumberOfPointsTextBox.Text);
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_maxNumberOfPointsTextBox.Text))
                {
                    _maxNumberOfPoints = int.Parse(_maxNumberOfPointsTextBox.Text);
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            if (renderContext.IsCurrentRenderPass<I2DBatchedRenderPass>())
            {
                if (DetectedPoints != null)
                {
                    foreach (var point in DetectedPoints)
                    {
                        _renderUtilities.RenderRectangle(
                            renderContext,
                            new Rectangle(
                                (int) point.X*_detectorEntity.WidthScale,
                                (int) point.Y*_detectorEntity.HeightScale,
                                _detectorEntity.WidthScale, _detectorEntity.HeightScale),
                            Color.Lime);
                    }
                }

                if (TopLeft != null)
                {
                    _renderUtilities.RenderText(
                        renderContext,
                        new Vector2(TopLeft.Value.X*_detectorEntity.WidthScale,
                            TopLeft.Value.Y*_detectorEntity.HeightScale),
                        "Top-Left",
                        _defaultFont);
                }

                if (TopRight != null)
                {
                    _renderUtilities.RenderText(
                        renderContext,
                        new Vector2(TopRight.Value.X*_detectorEntity.WidthScale,
                            TopRight.Value.Y*_detectorEntity.HeightScale),
                        "Top-Right",
                        _defaultFont);
                }

                if (BottomLeft != null)
                {
                    _renderUtilities.RenderText(
                        renderContext,
                        new Vector2(BottomLeft.Value.X*_detectorEntity.WidthScale,
                            BottomLeft.Value.Y*_detectorEntity.HeightScale),
                        "Bottom-Left",
                        _defaultFont);
                }

                if (BottomRight != null)
                {
                    _renderUtilities.RenderText(
                        renderContext,
                        new Vector2(BottomRight.Value.X*_detectorEntity.WidthScale,
                            BottomRight.Value.Y*_detectorEntity.HeightScale),
                        "Bottom-Right",
                        _defaultFont);
                }
            }
        }

        private void Run()
        {
            while (true)
            {
                try
                {
                    var targetColor = _detectorEntity.DetectedColors.FirstOrDefault(x => x.Name == _analyzerColor);
                    if (targetColor == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (targetColor.RecognisedArray == null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var detectedPoints = new List<Vector2>();

                    if (_checkArray == null ||
                        _checkArray.GetLength(0) != targetColor.RecognisedArray.GetLength(0) ||
                        _checkArray.GetLength(1) != targetColor.RecognisedArray.GetLength(1))
                    {
                        _checkArray =
                            new bool[targetColor.RecognisedArray.GetLength(0), targetColor.RecognisedArray.GetLength(1)];
                    }
                    else
                    {
                        for (var x = 0; x < _checkArray.GetLength(0); x++)
                        {
                            for (var y = 0; y < _checkArray.GetLength(1); y++)
                            {
                                _checkArray[x, y] = false;
                            }
                        }
                    }

                    for (var x = 0; x < targetColor.RecognisedArray.GetLength(0); x++)
                    {
                        for (var y = 0; y < targetColor.RecognisedArray.GetLength(1); y++)
                        {
                            var score = targetColor.RecognisedArray[x, y]/targetColor.Sensitivity;

                            if (score > _pointThreshold && !_checkArray[x, y])
                            {
                                if (FloodFillCheckAt(targetColor, _checkArray, x, y))
                                {
                                    detectedPoints.Add(new Vector2(x, y));
                                }
                            }
                        }
                    }

                    DetectedPoints = detectedPoints;

                    // If we have four points, try and determine top-left, top-right, bottom-left and bottom-right.
                    if (DetectedPoints.Count == 4)
                    {
                        TopLeft = DetectedPoints.OrderBy(x => x.X + x.Y).FirstOrDefault();
                        TopRight = DetectedPoints.OrderBy(x => (_checkArray.GetLength(0) - x.X) + x.Y).FirstOrDefault();
                        BottomLeft =
                            DetectedPoints.OrderBy(x => x.X + (_checkArray.GetLength(1) - x.Y)).FirstOrDefault();
                        BottomRight =
                            DetectedPoints.OrderBy(
                                x => (_checkArray.GetLength(0) - x.X) + (_checkArray.GetLength(1) - x.Y))
                                .FirstOrDefault();
                        TopLeftNormalized = new Vector2(TopLeft.Value.X/(float) _checkArray.GetLength(0),
                            TopLeft.Value.Y/(float) _checkArray.GetLength(1));
                        TopRightNormalized = new Vector2(TopRight.Value.X/(float) _checkArray.GetLength(0),
                            TopRight.Value.Y/(float) _checkArray.GetLength(1));
                        BottomLeftNormalized = new Vector2(BottomLeft.Value.X/(float) _checkArray.GetLength(0),
                            BottomLeft.Value.Y/(float) _checkArray.GetLength(1));
                        BottomRightNormalized = new Vector2(BottomRight.Value.X/(float) _checkArray.GetLength(0),
                            BottomRight.Value.Y/(float) _checkArray.GetLength(1));
                    }
                    else
                    {
                        TopLeft = null;
                        TopRight = null;
                        BottomLeft = null;
                        BottomRight = null;
                        TopLeftNormalized = null;
                        TopRightNormalized = null;
                        BottomLeftNormalized = null;
                        BottomRightNormalized = null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                Thread.Sleep(20);
            }
        }

        private bool FloodFillCheckAt(ColorToDetect targetColor, bool[,] checkArray, int x, int y)
        {
            var totalCount = 0;
            var toCheck = new Stack<KeyValuePair<int, int>>();
            toCheck.Push(new KeyValuePair<int, int>(x, y));
            while (toCheck.Count > 0)
            {
                var toScan = toCheck.ToList();
                toCheck.Clear();

                foreach (var s in toScan)
                {
                    var xx = s.Key;
                    var yy = s.Value;

                    if (checkArray[xx, yy])
                    {
                        continue;
                    }

                    if (targetColor.RecognisedArray[xx, yy] > _pointThreshold)
                    {
                        totalCount++;

                        if (totalCount > _maxNumberOfPoints)
                        {
                            // We can't be true beyond this point.
                            return false;
                        }

                        checkArray[xx, yy] = true;

                        if (xx >= 1 && xx < checkArray.GetLength(0) - 1)
                        {
                            if (yy >= 1 && yy < checkArray.GetLength(1) - 1)
                            {
                                toCheck.Push(new KeyValuePair<int, int>(xx - 1, yy - 1));
                                toCheck.Push(new KeyValuePair<int, int>(xx - 1, yy));
                                toCheck.Push(new KeyValuePair<int, int>(xx - 1, yy + 1));
                                toCheck.Push(new KeyValuePair<int, int>(xx, yy - 1));
                                toCheck.Push(new KeyValuePair<int, int>(xx, yy + 1));
                                toCheck.Push(new KeyValuePair<int, int>(xx + 1, yy - 1));
                                toCheck.Push(new KeyValuePair<int, int>(xx + 1, yy));
                                toCheck.Push(new KeyValuePair<int, int>(xx + 1, yy + 1));
                            }
                        }
                    }
                }
            }

            if (totalCount >= _minNumberOfPoints && totalCount <= _maxNumberOfPoints)
            {
                return true;
            }

            return false;
        }
    }
}
