using System;
using AugmentedBoardGame.Webcam;
using Protoinject;

namespace AugmentedBoardGame
{
    public interface IEntityFactory : IGenerateFactory
    {
        ExampleEntity CreateExampleEntity(string name);

        WebcamEntity CreateWebcamEntity();

        DetectorEntity CreateDetectorEntity(WebcamEntity webcamEntity);
    }
}
