using PicoGK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.Cars
{
    public abstract class Line : ICloneable
    {
        public float MinHeight;
        public float MaxHeight;
        public abstract float Modulation(float length);
        public abstract object Clone();
        public abstract override bool Equals(object? obj);
    }

    public class Constant : Line
    {
        public Constant(float height)
        {
            MinHeight = height;
            MaxHeight = height;
        }
        public override float Modulation(float length)
        {
            return MinHeight*2;
        }
        public override string ToString()
        {
            return $"new Constant(height: {MinHeight}f)";
        }
        public override object Clone()
        {
            Constant line = new Constant(MinHeight);
            return line ;
        }
        public override bool Equals(object? obj)
        {
            return ((Constant)obj).MinHeight == MinHeight && ((Constant)obj).MaxHeight == MaxHeight;
        }
    }

    public class TotalRounded : Line
    {
        public TotalRounded(float minHeight, float maxHeight, bool leftRounded)
        {
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            LeftRounded = leftRounded;
        }
        public bool LeftRounded;
        public override float Modulation(float length)
        {
            float height = 0;
            if (LeftRounded)
            {
                height = MinHeight + (MaxHeight - MinHeight) * (float)Math.Pow(length, 0.5);
            }
            else
            {
                height = MinHeight + (MaxHeight - MinHeight) * (float)Math.Pow(1 - length, 0.5);
            }
            return height*2;
        }

        public override string ToString()
        {
            return $"new TotalRounded(minHeight: {MinHeight}f, maxHeight: {MaxHeight}f, leftRounded: {LeftRounded.ToString().ToLower()})";
        }
        public override object Clone()
        {
            TotalRounded line = new TotalRounded(MinHeight, MaxHeight, LeftRounded);
            return line;
        }
        public override bool Equals(object? obj)
        {
            return ((TotalRounded)obj).MinHeight == MinHeight && ((TotalRounded)obj).MaxHeight == MaxHeight && ((TotalRounded)obj).LeftRounded == LeftRounded;
        }
    }

    public class CornerRounded : Line
    {
        public CornerRounded(float minHeight, float maxHeight, float cornerRelativeLength, float surfaceAbsoluteLength, bool leftCornerRounded, bool rightCornerRounded)
        {
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            CornerLength = cornerRelativeLength;
            TotalLength = surfaceAbsoluteLength;
            LeftCorner = leftCornerRounded;
            RightCorner = rightCornerRounded;
        }
        public float CornerLength;
        public float TotalLength;
        public bool LeftCorner;
        public bool RightCorner;
        public override float Modulation(float length)
        {
            float height = 0;
            float startRadius = CornerLength * TotalLength;
            float heightRadius = Math.Min(CornerLength * TotalLength, MaxHeight - MinHeight);
            if (length <= CornerLength)
            {
                if (LeftCorner)
                {
                    float curRadius = startRadius + (heightRadius - startRadius) * (length / CornerLength);
                    height = MaxHeight * 2 - 2*heightRadius + 2*(float)Math.Sin(Math.Acos((CornerLength - length) * TotalLength / curRadius)) * curRadius;
                }
                else
                {
                    height = MaxHeight * 2;
                }
            }
            else if (length >= CornerLength && length <= 1 - CornerLength)
            {
                height = MaxHeight * 2;
            }
            else if (length >= 1 - CornerLength)
            {
                if (RightCorner)
                {
                    float curRadius = heightRadius - (heightRadius - startRadius) * ((CornerLength - 1 + length) / CornerLength);
                    height = MaxHeight * 2 - 2*heightRadius + 2*(float)Math.Cos(Math.Asin((CornerLength - 1 + length) * TotalLength / curRadius)) * curRadius;
                }
                else
                {
                    height = MaxHeight * 2;
                }
            }
            return height;
        }

        public override string ToString()
        {
            return $"new CornerRounded(minHeight: {MinHeight}f, maxHeight: {MaxHeight}f, cornerRelativeLength: {CornerLength}f, surfaceAbsoluteLength: {TotalLength}f, leftCornerRounded: {LeftCorner.ToString().ToLower()}, rightCornerRounded: {RightCorner.ToString().ToLower()})";
        }
        public override object Clone()
        {
            CornerRounded line = new CornerRounded(MinHeight, MaxHeight, CornerLength, TotalLength, LeftCorner, RightCorner);
            return line;
        }
        public override bool Equals(object? obj)
        {
            return ((CornerRounded)obj).MinHeight == MinHeight && ((CornerRounded)obj).MaxHeight == MaxHeight && ((CornerRounded)obj).CornerLength == CornerLength
                && ((CornerRounded)obj).TotalLength == TotalLength && ((CornerRounded)obj).LeftCorner == LeftCorner && ((CornerRounded)obj).RightCorner == RightCorner;
        }
    }
}
