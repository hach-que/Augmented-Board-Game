using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Protogame;

namespace AugmentedBoardGame.Webcam
{
    public class ColorToDetect
    {
        public Color Color;

        public string Name;

        public int[,] RecognisedArray;

        public float Sensitivity;

        public int TotalDetected;

        public int UnlockedTotalDetected;
    }

    public class DetectorEntity : Entity
    {
        private const int chunkSize = 5;
        private readonly IAssetManager _assetManager;
        private readonly FontAsset _defaultFont;
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly WebcamEntity _webcamEntity;

        private readonly List<ColorToDetect> _colorsToDetect;
        private ColorToDetect _currentColor;
        private int _currentIndex;
        private string _status;

        private object _statusLock;

        private readonly Thread _thread;

        public float GlobalSensitivity;

        public DetectorEntity(WebcamEntity webcamEntity, IAssetManager assetManager, I2DRenderUtilities renderUtilities)
        {
            _webcamEntity = webcamEntity;
            _assetManager = assetManager;
            _renderUtilities = renderUtilities;
            _defaultFont = _assetManager.Get<FontAsset>("font.Default");

            _colorsToDetect = new List<ColorToDetect>
            {
                new ColorToDetect {Color = new Color(1f, 0f, 0f), Name = "Red"},
                new ColorToDetect {Color = new Color(0f, 1f, 0f), Name = "Green"},
                new ColorToDetect {Color = new Color(0f, 0f, 1f), Name = "Blue"},
                new ColorToDetect {Color = new Color(1f, 1f, 0f), Name = "Yellow"}
            };

            _currentIndex = 0;
            _currentColor = _colorsToDetect[_currentIndex];

            GlobalSensitivity = 1/10000000f*25f;

            _thread = new Thread(ProcessorThread);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public List<ColorToDetect> DetectedColors => _colorsToDetect;

        public void NextColor()
        {
            _currentIndex++;
            if (_currentIndex >= _colorsToDetect.Count)
            {
                _currentIndex = 0;
            }
            _currentColor = _colorsToDetect[_currentIndex];
        }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            if (renderContext.IsCurrentRenderPass<I2DBatchedRenderPass>())
            {
                var recogArray = _currentColor.RecognisedArray;

                if (recogArray == null)
                {
                    return;
                }

                WidthScale = (int) Math.Ceiling(renderContext.GraphicsDevice.PresentationParameters.BackBufferWidth/
                                                (float) recogArray.GetLength(0));
                HeightScale = (int) Math.Ceiling(renderContext.GraphicsDevice.PresentationParameters.BackBufferHeight/
                                                 (float) recogArray.GetLength(1));

                for (var x = 0; x < recogArray.GetLength(0); x++)
                {
                    for (var y = 0; y < recogArray.GetLength(1); y++)
                    {
                        Color col;
                        var score = recogArray[x, y]/_currentColor.Sensitivity;

                        //var scoreCapped = MathHelper.Clamp(score, 0f, 255f) / 255f;
                        //col = new Color(scoreCapped, scoreCapped, scoreCapped);

                        if (score < 0)
                        {
                            var scoreCapped = Math.Min(255, -score);
                            col = new Color(scoreCapped/255f, scoreCapped/255f, scoreCapped/255f, 1f);
                        }
                        else
                        {
                            var scoreCapped = Math.Max(0, score);
                            col = new Color(
                                scoreCapped/255f*(_currentColor.Color.R/255f),
                                scoreCapped/255f*(_currentColor.Color.G/255f),
                                scoreCapped/255f*(_currentColor.Color.B/255f),
                                1f);
                        }

                        _renderUtilities.RenderRectangle(
                            renderContext,
                            new Rectangle(
                                (int) (X + x*WidthScale), (int) (Y + y*HeightScale), WidthScale, HeightScale),
                            col, true);
                    }
                }

                _renderUtilities.RenderText(
                    renderContext,
                    new Vector2(
                        renderContext.GraphicsDevice.PresentationParameters.BackBufferWidth/2,
                        40),
                    "Total " + _currentColor.Name + ": " + _currentColor.TotalDetected,
                    _defaultFont);
            }
        }

        public int HeightScale { get; set; }

        public int WidthScale { get; set; }

        private void ProcessorThread()
        {
            while (true)
            {
                try
                {
                    var total = 0;

                    foreach (var color in _colorsToDetect)
                    {
                        color.UnlockedTotalDetected = 0;
                    }

                    var copy = _webcamEntity.UnlockedFrameRGBA;
                    if (copy != null)
                    {
                        for (var x = 0; x < _webcamEntity.ImageWidth/chunkSize; x++)
                        {
                            for (var y = 0; y < _webcamEntity.ImageHeight/chunkSize; y++)
                            {
                                foreach (var color in _colorsToDetect)
                                {
                                    var rT = color.Color.R;
                                    var gT = color.Color.G;
                                    var bT = color.Color.B;

                                    //var totalOnTarget = rT + gT + bT;

                                    var rI = 255 - color.Color.R;
                                    var gI = 255 - color.Color.G;
                                    var bI = 255 - color.Color.B;

                                    //var totalOffTarget = rI + gI + bI;

                                    if (color.RecognisedArray == null ||
                                        color.RecognisedArray.GetLength(0) != _webcamEntity.ImageWidth/chunkSize ||
                                        color.RecognisedArray.GetLength(1) != _webcamEntity.ImageHeight/chunkSize)
                                    {
                                        color.RecognisedArray =
                                            new int[_webcamEntity.ImageWidth/chunkSize,
                                                _webcamEntity.ImageHeight/chunkSize];
                                    }

                                    var chunkScore = 0;
                                    for (var xx = 0; xx < chunkSize; xx++)
                                    {
                                        for (var yy = 0; yy < chunkSize; yy++)
                                        {
                                            var idx = (x*chunkSize + xx + (y*chunkSize + yy)*_webcamEntity.ImageWidth)*4;
                                            var rA = copy[idx];
                                            var gA = copy[idx + 1];
                                            var bA = copy[idx + 2];

                                            var illumination = (rA + gA + bA)/3f/255f;

                                            /*

                                        T:   255, 0, 0                                   200 - (200 + 200) / 2
                                        A:   200, 200, 200
                                        D:   55, 200, 200
                                        OT:  200, 55, 55
                                        OTV: (200 + 55 + 55)
                                        OTV: 310

                                        XT:  0, 255, 255
                                        XA:  200, 200, 200
                                        XD:  200, 55, 55
                                        XOT: 55, 200, 200
                                        XOTV:(55 + 200 + 200)
                                        XOTV:455

                                        -165
                                        
                                        T:   255, 0, 0                                   200 - (200 + 200) / 2
                                        A:   200, 0, 0
                                        D:   55, 0, 0
                                        OT:  200, 255, 255
                                        OTV: (200 + 255 + 255)
                                        OTV: 710

                                        XT:  0, 255, 255
                                        XA:  200, 0, 0
                                        XD:  200, 255, 255
                                        XOT: 55, 0, 0
                                        XOTV:(55 + 200 + 200)
                                        XOTV:455

                                        255

    */

                                            var redOnTarget = 255 - Math.Abs(rT - rA);
                                            var greenOnTarget = 255 - Math.Abs(gT - gA);
                                            var blueOnTarget = 255 - Math.Abs(bT - bA);
                                            var onTarget = (int) ((redOnTarget + greenOnTarget + blueOnTarget)/1f);
                                            //(float)totalOnTarget * 255f

                                            var redOffTarget = 255 - Math.Abs(rI - rA);
                                            var greenOffTarget = 255 - Math.Abs(gI - gA);
                                            var blueOffTarget = 255 - Math.Abs(bI - bA);
                                            var offTarget = (int) ((redOffTarget + greenOffTarget + blueOffTarget)/0.5f);
                                            // (float)totalOffTarget * 255f

                                            chunkScore += (int) ((onTarget - offTarget)*illumination);
                                            color.UnlockedTotalDetected += (int) (illumination*255f);
                                        }
                                    }

                                    color.RecognisedArray[x, y] = chunkScore;
                                    //color.UnlockedTotalDetected += chunkScore;
                                }
                            }
                        }
                    }

                    foreach (var color in _colorsToDetect)
                    {
                        color.Sensitivity = Math.Max(0, color.TotalDetected)*GlobalSensitivity;
                        color.TotalDetected = color.UnlockedTotalDetected;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                Thread.Sleep(20);
            }
        }
    }
}