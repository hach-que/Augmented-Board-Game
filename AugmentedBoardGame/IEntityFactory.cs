using System;
using AugmentedBoardGame.Webcam;
using Protogame;
using Protoinject;

namespace AugmentedBoardGame
{
    public interface IEntityFactory : IGenerateFactory
    {
        ExampleEntity CreateExampleEntity(string name);

        WebcamEntity CreateWebcamEntity();

        DetectorEntity CreateDetectorEntity(WebcamEntity webcamEntity);

        BoardAnalyzerEntity CreateBoardAnalyzerEntity(DetectorEntity detectorEntity, TextBox pointThresholdTextBox,
            TextBox minNumberOfPointsTextBox,
            TextBox maxNumberOfPointsTextBox);

        CanvasEntity CreateCanvasEntity();

        BoardRendererEntity CreateBoardRendererEntity(BoardAnalyzerEntity analyzerEntity, WebcamEntity webcamEntity,
            TextBox alphaTextBox);
    }
}
