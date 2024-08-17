using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.GenerativeDesign.Car
{
    public class Shape : ICloneable
    {
        public float Radius;
        public virtual float Modulation(float angle, float length)
        {
            return Radius;
        }
        public object Clone()
        {
            Shape clone = new Shape();
            clone.Radius = Radius;
            return clone;
        }
        public override bool Equals(object? obj)
        {
            bool result = false;
            if (obj is Shape)
            {
                Shape shape = (Shape)obj;
                if (shape.Radius == Radius)
                {
                    result = true;
                }
            }
            return result;
        }
    }
    public class Circle : Shape
    {
        public override float Modulation(float angle, float length)
        {
            return base.Modulation(angle, length);
        }
    }
    public class Square : Shape
    {
        public override float Modulation(float angle, float length)
        {
            double result = Radius;
            if ((int)(angle / (Math.PI / 4)) % 2 == 1)
            {
                result = Radius / Math.Cos(angle);
            }
            return base.Modulation(angle, length);
        }
    }
}
