using System.Collections.Generic;
using System;
using System.Collections;
namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 320f;
            Width = 145f;
            Height = 140f;

            WheelWidth = 15f;
            WheelRadius = 35.15f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 17f) };
            WheelBaseTopSurface = new Constant(height: 80f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new Constant(height: 84f), new CornerRounded(minHeight: 84f, maxHeight: 140f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 224f, leftCornerRounded: true, rightCornerRounded: false) };
        }
    }

}