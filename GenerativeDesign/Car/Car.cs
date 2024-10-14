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
            Width = 167f;
            Height = 150f;

            WheelWidth = 20f;
            WheelRadius = 40f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 20f) };
            WheelBaseTopSurface = new Constant(height: 80f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 0.8f }, new List<float>() { 0.8f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() {
                new TotalRounded(minHeight: 80f, maxHeight: 100f, leftRounded: true),
                new CornerRounded(minHeight: 100f, maxHeight: 150f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 150f, leftCornerRounded: true, rightCornerRounded: true),
                new TotalRounded(minHeight: 80f, maxHeight: 100f, leftRounded: false)
            };
        }
    }
}
