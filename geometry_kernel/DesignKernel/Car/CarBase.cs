using Leap71.ShapeKernel;
using PicoGK;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenerativeDesign.Cars
{
    
    public class CarBase : ICloneable
    {
        #region Base car parameters
        public Vector3 BasePoint = new Vector3(0, 0, 0);
        public float Length = 60;
        public float Width = 30;
        public float Height = 30;
        #endregion

        public Voxels voxConstruct()
        {
            Voxels voxWheels = voxWheelsConstruct();
            Voxels voxWheelsBase = voxWheelsBaseConstruct();
            Voxels voxBody = voxBodyConstruct();
            Voxels voxCar = Sh.voxUnion(new List<Voxels>() { voxWheels, voxWheelsBase, voxBody });
            return voxCar;
        }

        #region Wheel construction

        public float WheelWidth = 5;
        public float WheelRadius = 5;
        public float WheelRelativeBiasAlongWidth = 0.1f;
        public List<float> WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };
        public int curBiasInd = 0;
        public Voxels voxWheelsConstruct()
        {
            List<Voxels> voxWheels = new List<Voxels>();
            for (int i = 0; i < WheelRelativeBiasesAlongLength.Count; i++)
            {
                curBiasInd = i;
                BaseCylinder wheel1 = new BaseCylinder(new LocalFrame(new Vector3(0, Width * WheelRelativeBiasAlongWidth - WheelWidth / 2, WheelRadius) + BasePoint, new Vector3(0, 1, 0)), WheelWidth, WheelRadius);
                BaseCylinder wheel2 = new BaseCylinder(new LocalFrame(new Vector3(0, Width * (1 - WheelRelativeBiasAlongWidth) - WheelWidth / 2, WheelRadius) + BasePoint, new Vector3(0, 1, 0)), WheelWidth, WheelRadius);
                wheel1.SetTransformation(WheelTransformation);
                wheel2.SetTransformation(WheelTransformation);
                Voxels voxWheelLine = Sh.voxUnion(wheel1.voxConstruct(), wheel2.voxConstruct());
                voxWheels.Add(voxWheelLine);
            }
            Voxels wheels = Sh.voxUnion(voxWheels);
            return wheels;
        }

        public Vector3 WheelTransformation(Vector3 vec)
        {
            float curBias = WheelRelativeBiasesAlongLength[curBiasInd] * Length;
            Vector3 newVec = new Vector3(vec.X + curBias, vec.Y, vec.Z);
            return newVec;
        }
        #endregion

        #region Wheels base construction
        public List<List<float>> WheelBaseSegmentsSpans = new List<List<float>>()
        {
            new List<float>(){0, 1},
        };
        public List<Line> WheelBaseSegmentsBottomSurfaces = new List<Line>()
        {
            new Constant(5)
        };
        public Line WheelBaseTopSurface = new Constant(12);
        public float GapBetweenWheelAndBase = 1;
        public Voxels voxWheelsBaseConstruct()
        {
            BaseBox baseBox = new BaseBox(new LocalFrame(new Vector3(0, Width / 2, Height / 2) + BasePoint, new Vector3(1, 0, 0)), Length, Height, Width);
            BaseBox topIntersectBox = new BaseBox(new LocalFrame(new Vector3(0, Width / 2, 0) + BasePoint, new Vector3(1, 0, 0)), Length, Height, Width);
            topIntersectBox.SetWidth(new LineModulation(WheelBaseTopSurface.Modulation));

            Voxels voxBaseBox = baseBox.voxConstruct();
            List<Voxels> voxBaseSegments = new List<Voxels>();
            for (int i = 0; i < WheelBaseSegmentsSpans.Count; i++)
            {
                List<float> span = WheelBaseSegmentsSpans[i];
                Line surface = WheelBaseSegmentsBottomSurfaces[i];
                BaseBox segmentBox = new BaseBox(new LocalFrame(new Vector3(span[0]*Length, Width / 2, 0) + BasePoint, new Vector3(1, 0, 0)), (span[1] - span[0])*Length, Height, Width);
                segmentBox.SetWidth(new LineModulation(surface.Modulation));
                BaseBox intersectSegmentBox = new BaseBox(new LocalFrame(new Vector3(span[0] * Length, Width / 2, Height / 2) + BasePoint, new Vector3(1, 0, 0)), (span[1] - span[0]) * Length, Height, Width);
                Voxels voxSegmentBox = Sh.voxSubtract(Sh.voxIntersect(voxBaseBox, intersectSegmentBox.voxConstruct()), segmentBox.voxConstruct());
                voxBaseSegments.Add(voxSegmentBox);
            }
            voxBaseBox = Sh.voxUnion(voxBaseSegments);
            Voxels voxTopIntersectBox = topIntersectBox.voxConstruct();
            voxBaseBox = Sh.voxIntersect(voxBaseBox, voxTopIntersectBox);

            for (int i = 0; i < WheelRelativeBiasesAlongLength.Count; i++)
            {
                curBiasInd = i;
                BaseCylinder wheelSubtractor = new BaseCylinder(new LocalFrame(new Vector3(0, 0, WheelRadius) + BasePoint, new Vector3(0, 1, 0)), Width, WheelRadius + GapBetweenWheelAndBase);
                wheelSubtractor.SetTransformation(WheelTransformation);
                Voxels voxWheelSubtractor = wheelSubtractor.voxConstruct();
                voxBaseBox = Sh.voxSubtract(voxBaseBox, voxWheelSubtractor);
            }
            return voxBaseBox;
        }
        #endregion

        #region Body construction
        public List<List<float>> BodySegmentsSpans = new List<List<float>>()
        {
            new List<float>(){0, 0.3f},
            new List<float>(){0.3f, 0.8f},
            new List<float>(){0.8f, 1},
        };
        public List<Line> BodySegmentsTopSurfaces = new List<Line>()
        {
            new TotalRounded(12, 20, true),
            new Constant(30),
            new CornerRounded(12, 20, 0.5f, 0.2f*60, false, true)
        };
        public Voxels voxBodyConstruct()
        {
            BaseBox baseBox = new BaseBox(new LocalFrame(new Vector3(0, Width / 2, Height / 2) + BasePoint, new Vector3(1, 0, 0)), Length, Height, Width);
            BaseBox bottomSubtractBox = new BaseBox(new LocalFrame(new Vector3(0, Width / 2, 0) + BasePoint, new Vector3(1, 0, 0)), Length, Height, Width);
            bottomSubtractBox.SetWidth(new LineModulation(WheelBaseTopSurface.Modulation));

            Voxels voxBaseBox = baseBox.voxConstruct();
            List<Voxels> voxBodySegments = new List<Voxels>();
            for (int i = 0; i < BodySegmentsSpans.Count; i++)
            {
                List<float> span = BodySegmentsSpans[i];
                Line surface = BodySegmentsTopSurfaces[i];
                BaseBox segmentBox = new BaseBox(new LocalFrame(new Vector3(span[0] * Length, Width / 2, 0) + BasePoint, new Vector3(1, 0, 0)), (span[1] - span[0]) * Length, Height, Width);
                segmentBox.SetWidth(new LineModulation(surface.Modulation));
                Voxels voxSegmentBox = Sh.voxIntersect(voxBaseBox, segmentBox.voxConstruct());
                voxBodySegments.Add(voxSegmentBox);
            }
            voxBaseBox = Sh.voxUnion(voxBodySegments);
            Voxels voxBottomSubtractBox = bottomSubtractBox.voxConstruct();
            voxBaseBox = Sh.voxSubtract(voxBaseBox, voxBottomSubtractBox);
            return voxBaseBox;
        }
        #endregion

        public object Clone()
        {
            Car newCar = new Car();
            newCar.Length = Length;
            newCar.Width = Width;
            newCar.Height = Height;

            newCar.WheelWidth = WheelWidth;
            newCar.WheelRadius = WheelRadius;
            newCar.WheelRelativeBiasAlongWidth = WheelRelativeBiasAlongWidth;
            newCar.WheelRelativeBiasesAlongLength = WheelRelativeBiasesAlongLength;

            newCar.WheelBaseSegmentsSpans = WheelBaseSegmentsSpans;
            newCar.WheelBaseSegmentsBottomSurfaces = WheelBaseSegmentsBottomSurfaces.Select(x => x.Clone() as Line).ToList();
            newCar.WheelBaseTopSurface = WheelBaseTopSurface.Clone() as Line;
            newCar.GapBetweenWheelAndBase = GapBetweenWheelAndBase;

            newCar.BodySegmentsSpans = BodySegmentsSpans;
            newCar.BodySegmentsTopSurfaces = BodySegmentsTopSurfaces.Select(x => x.Clone() as Line).ToList();
            return newCar;
        }
        public override string ToString()
        {
            string path = "C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\generative_design\\GenerativeDesign\\Car\\Car.cs";
            string file = File.ReadAllText(path);

            string pattern = @"(FIELD =)[^;]+(;\s+)";
            Regex re = new Regex(pattern);
            string fieldName = "FIELD";
            string value = "";
            string baseReplacement = @"$1 VALUE$2";
            string replacement = "";

            fieldName = "Length";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.Length.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "Width";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.Width.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "Height";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.Height.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelWidth";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.WheelWidth.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelRadius";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.WheelRadius.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelRelativeBiasAlongWidth";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.WheelRelativeBiasAlongWidth.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelRelativeBiasesAlongLength";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            List<string> formattedBiases = new List<string>();
            foreach (float bias in this.WheelRelativeBiasesAlongLength)
            {
                formattedBiases.Add(bias.ToString() + "f");
            }
            value = $"new List<float>() {{ {String.Join(", ", formattedBiases)} }}";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelBaseSegmentsSpans";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            List<string> spans = new List<string>();
            foreach (List<float> span in this.WheelBaseSegmentsSpans)
            {
                string strSpan = $"new List<float>(){{ {span[0] + "f"}, {span[1] + "f"} }}";
                spans.Add(strSpan);
            }
            value = $"new List<List<float>>() {{ {String.Join(", ", spans)} }}";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelBaseSegmentsBottomSurfaces";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            List<string> bottomSurfaces = new List<string>();
            foreach (Line surf in this.WheelBaseSegmentsBottomSurfaces)
            {
                bottomSurfaces.Add(surf.ToString());
            }
            value = $"new List<Line>() {{ {String.Join(", ", bottomSurfaces)} }}";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "WheelBaseTopSurface";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.WheelBaseTopSurface.ToString();
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "GapBetweenWheelAndBase";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            value = this.GapBetweenWheelAndBase.ToString() + "f";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "BodySegmentsSpans";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            List<string> bodySpans = new List<string>();
            foreach (List<float> span in this.BodySegmentsSpans)
            {
                string strSpan = $"new List<float>(){{ {span[0] + "f"}, {span[1] + "f"} }}";
                bodySpans.Add(strSpan);
            }
            value = $"new List<List<float>>() {{ {String.Join(", ", bodySpans)} }}";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);

            fieldName = "BodySegmentsTopSurfaces";
            re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
            List<string> topSurfaces = new List<string>();
            foreach (Line surf in this.BodySegmentsTopSurfaces)
            {
                topSurfaces.Add(surf.ToString());
            }
            value = $"new List<Line>() {{ {String.Join(", ", topSurfaces)} }}";
            replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
            file = re.Replace(file, replacement);
            return file;
        }
    }
}
