using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Protogame;

namespace AugmentedBoardGame.Webcam
{
    public class WebcamEntity : Entity
    {
        private readonly ICameraSensor _cameraSensor;
        private readonly TextBox _deviceName;

        private readonly List<ICamera> _cameras;

        private int _activeCameraIndex;

        public WebcamEntity(ISensorEngine sensorEngine, ICameraSensor cameraSensor, TextBox deviceName)
        {
            _cameraSensor = cameraSensor;
            _deviceName = deviceName;
            _cameras = _cameraSensor.GetAvailableCameras();
            _activeCameraIndex = 0;

            _cameraSensor.ActiveCamera = _cameras[_activeCameraIndex];

            // Because sensors may engage hardware, you need to explicitly register
            // them with the sensor engine.
            sensorEngine.Register(_cameraSensor);
        }

        public Texture2D VideoCaptureFrame
        {
            get { return _cameraSensor.VideoCaptureFrame; }
        }

        public byte[] UnlockedFrameRGBA
        {
            get { return _cameraSensor.VideoCaptureUnlockedRGBA; }
        }

        public int ImageWidth
        {
            get { return _cameraSensor.VideoCaptureWidth.Value; }
        }

        public int ImageHeight
        {
            get { return _cameraSensor.VideoCaptureHeight.Value; }
        }

        public void NextDevice()
        {
            _activeCameraIndex++;
            if (_activeCameraIndex >= _cameras.Count)
            {
                _activeCameraIndex = 0;
            }
            _cameraSensor.ActiveCamera = _cameras[_activeCameraIndex];
        }

        public override void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            base.Update(gameContext, updateContext);

            _deviceName.Text = _cameras[_activeCameraIndex].Name;
        }
    }
}