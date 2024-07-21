namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 60;
            Width = 30;
            Height = 30;

            WheelWidth = 5;
            WheelRadius = 5;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>()
            {
                new List<float>(){0, 1},
            };
            WheelBaseSegmentsBottomSurfaces = new List<Line>()
            {
                new Constant(height: 5)
            };
            WheelBaseTopSurface = new Constant(height: 12);
            GapBetweenWheelAndBase = 1;

            BodySegmentsSpans = new List<List<float>>()
            {
                new List<float>(){0, 0.3f},
                new List<float>(){0.3f, 0.8f},
                new List<float>(){0.8f, 1},
            };
            BodySegmentsTopSurfaces = new List<Line>()
            {
                new TotalRounded(minHeight: 12, maxHeight: 20, leftRounded: true),
                new Constant(height: 30),
                new CornerRounded(minHeight: 12, maxHeight: 20, cornerRelativeLength: 0.5f, surfaceAbsoluteLength: 0.2f*60, leftCornerRounded: false, rightCornerRounded: true),
            };
        }
    }
}
