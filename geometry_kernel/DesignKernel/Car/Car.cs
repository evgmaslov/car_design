using System.Collections.Generic;
using System;
using System.Collections;

namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 250f;
            Width = 165f;
            Height = 150f;

            WheelWidth = 20f;
            WheelRadius = 30f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 22f) };
            WheelBaseTopSurface = new Constant(height: 85f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 0.9f }, new List<float>() { 0.9f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new TotalRounded(minHeight: 85f, maxHeight: 100f, leftRounded: true), new Constant(height: 150f), new CornerRounded(minHeight: 85f, maxHeight: 100f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 33f, leftCornerRounded: false, rightCornerRounded: true) };
        }
    }
}
