using PicoGK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.Cars
{
    public abstract class Line
    {
        public float MinHeight;
        public float MaxHeight;
        public abstract float Modulation(float length);
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
    }
}
