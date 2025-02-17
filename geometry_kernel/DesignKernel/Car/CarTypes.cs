using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.Cars
{
    public abstract class CarType
    {
        internal Dictionary<string, int> NumValuesCount { get; set; } = new Dictionary<string, int>()
        {
            {"Length", 0},
            {"Width", 0},
            {"Height", 0},
            {"WheelWidth", 0},
            {"WheelRadius", 0},
            {"GapBetweenWheelAndBase", 0},
        };
        public List<Car> GetCars(int count = -1, float baseX=0, float baseY = 0, float baseZ = 0)
        {
            List<string> parameters = new List<string>()
            {
                "Length", "Width", "Height", "WheelWidth", "WheelRadius", "WheelRelativeBiasAlongWidth", "WheelRelativeBiasesAlongLength", "WheelBaseSegmentsSpans", 
                "WheelBaseSegmentsBottomSurfaces", "GapBetweenWheelAndBase", "WheelBaseTopSurface", "BodySegmentsSpans", "BodySegmentsTopSurfaces"
            };
            List<Car> cars = new List<Car>() { new Car()};
            foreach (string parameter in parameters)
            {
                FieldInfo field = typeof(Car).GetField(parameter);
                List<Car> newCars = new List<Car>();
                foreach (Car car in cars)
                {
                    List<object> values = GetValues(parameter, car);
                    foreach (object value in values)
                    {
                        Car newCar = car.Clone() as Car;
                        if (field.FieldType == typeof(float))
                        {
                            field.SetValue(newCar, System.Convert.ToSingle(value));
                        }
                        else
                        {
                            field.SetValue(newCar, value);
                        }
                        newCars.Add(newCar);
                    }
                }
                if (newCars.Count > 0)
                {
                    cars = newCars;
                }
            }
            float shift = 300;
            int ind = 0;
            Vector3 basePoint = new Vector3(baseX, baseY, baseZ);
            List<Car> finalCars = new List<Car>();
            if (count == -1)
            {
                for (int i = 0; i < cars.Count; i++)
                {
                    Car car = cars[i];
                    car.BasePoint.Y = i * shift;
                    car.BasePoint += basePoint;
                    finalCars.Add(car);
                }
            }
            else
            {
                List<int> randomInds = new List<int>();
                Random random = new Random();
                for (int i = 0; i < count; i++)
                {
                    randomInds.Add(random.Next(cars.Count));
                }
                for (int i = 0; i < count; i++)
                {
                    int randInd = randomInds[i];
                    Car randCar = cars[randInd];
                    randCar.BasePoint.Y = i * shift;
                    randCar.BasePoint += basePoint;
                    finalCars.Add(randCar);
                }
            }
            return finalCars;
        }
        internal abstract List<object> GetValues(string parameter, Car car);
        private Dictionary<string, List<List<object>>> ParameterToValues { get; set; } = new Dictionary<string, List<List<object>>>()
        {
            {"Length", new List<List<object>>()
            {
                new List<object>(){320, 370},
                new List<object>(){250, 440},
                new List<object>(){430, 470},
                new List<object>(){470, 510},
                new List<object>(){700, 1000},
                new List<object>(){550, 650},
                new List<object>(){1600, 2000},
            } },
            {"Width", new List<List<object>>()
            {
                new List<object>(){145, 165},
                new List<object>(){167, 177},
                new List<object>(){180, 190},
                new List<object>(){200, 210},
                new List<object>(){240, 260},
            } },
            {"Height", new List<List<object>>()
            {
                new List<object>(){135, 144},
                new List<object>(){140, 150},
                new List<object>(){150, 160},
                new List<object>(){200, 260},
                new List<object>(){260, 360},
            } },
            {"WheelWidth", new List<List<object>>()
            {
                new List<object>(){20},
            } },
            {"WheelRadius", new List<List<object>>()
            {
                new List<object>(){25, 40},
                new List<object>(){40, 45},
            } },
            {"WheelRelativeBiasAlongWidth", new List<List<object>>()
            {
                new List<object>(){0.1},
            } },
            {"WheelRelativeBiasesAlongLength", new List<List<object>>()
            {
                new List<object>(){new List<float>() { 0.2f, 0.8f }},
                new List<object>(){new List<float>() { 0.2f, 0.7f }},
                new List<object>(){new List<float>() { 0.1f, 0.9f }, new List<float>() { 0.07f, 0.93f }, new List<float>() { 0.1f, 0.9f }, new List<float>() { 0.07f, 0.93f }},
                new List<object>()
                {
                    new List<float>() { 0.04f, 0.23f, 0.28f, 0.8f, 0.85f, 0.9f },
                    new List<float>() { 0.04f, 0.23f, 0.28f, 0.5f, 0.55f, 0.67f, 0.95f },
                    new List<float>() { 0.05f, 0.36f, 0.41f, 0.8f, 0.85f, 0.9f },
                    new List<float>() { 0.05f, 0.36f, 0.41f, 0.65f, 0.7f, 0.82f, 0.95f },
                },
            } },
            {"WheelBaseSegmentsSpans", new List<List<object>>()
            {
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 1f } }
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.4f }, new List<float>() { 0.4f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.32f }, new List<float>() { 0.75f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.32f }, new List<float>() { 0.45f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.46f }, new List<float>() { 0.75f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.46f }, new List<float>() { 0.55f, 1f } },
                },
            } },
            {"WheelBaseSegmentsBottomSurfaces", new List<List<object>>()
            {
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 13f) },
                    new List<Line>() { new Constant(height: 18f) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 9f) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 25f) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 30f), new Constant(height: 40f) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 30f), new Constant(height: 70f) },
                },
            } },
            {"WheelBaseTopSurface", new List<List<object>>()
            {
                new List<object>()
                {
                    new Constant(height: 90f),
                },
                new List<object>()
                {
                    new Constant(height: 100f),
                },
            } },
            {"GapBetweenWheelAndBase", new List<List<object>>()
            {
                new List<object>(){3},
                new List<object>(){5},
            } },
            {"BodySegmentsSpans", new List<List<object>>()
            {
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 0.8f }, new List<float>() { 0.8f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 0.9f }, new List<float>() { 0.9f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 1 } },
                    new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 1 } },
                    new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 1 } },
                    new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 1 } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 0.8f }, new List<float>() { 0.8f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 0.9f }, new List<float>() { 0.9f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.4f }, new List<float>() { 0.4f, 0.8f }, new List<float>() { 0.8f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.1f }, new List<float>() { 0.1f, 0.9f }, new List<float>() { 0.9f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.07f }, new List<float>() { 0.07f, 0.93f }, new List<float>() { 0.93f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.2f }, new List<float>() { 0.2f, 0.6f }, new List<float>() { 0.6f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.3f }, new List<float>() { 0.3f, 0.6f }, new List<float>() { 0.6f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.1f }, new List<float>() { 0.1f, 1 } },
                    new List<List<float>>() { new List<float>() { 0f, 0.1f }, new List<float>() { 0.1f, 0.9f }, new List<float>() { 0.9f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.1f }, new List<float>() { 0.1f, 0.4f }, new List<float>() { 0.45f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.05f }, new List<float>() { 0.05f, 0.3f }, new List<float>() { 0.35f, 1f } },
                },
                new List<object>()
                {
                    new List<List<float>>() { new List<float>() { 0f, 0.17f }, new List<float>() { 0.19f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.17f }, new List<float>() { 0.19f, 0.6f }, new List<float>() { 0.62f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.07f }, new List<float>() { 0.07f, 0.17f }, new List<float>() { 0.19f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.07f }, new List<float>() { 0.07f, 0.17f }, new List<float>() { 0.19f, 0.6f }, new List<float>() { 0.62f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.13f }, new List<float>() { 0.13f, 0.3f }, new List<float>() { 0.32f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.13f }, new List<float>() { 0.13f, 0.3f }, new List<float>() { 0.32f, 0.75f }, new List<float>() { 0.77f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.13f }, new List<float>() { 0.13f, 0.2f }, new List<float>() { 0.2f, 0.3f }, new List<float>() { 0.32f, 1f } },
                    new List<List<float>>() { new List<float>() { 0f, 0.13f }, new List<float>() { 0.13f, 0.2f }, new List<float>() { 0.2f, 0.3f }, new List<float>() { 0.32f, 0.75f }, new List<float>() { 0.77f, 1f } },
                },
            } },
            {"BodySegmentsTopSurfaces", new List<List<object>>()
            {
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new TotalRounded(84, 90, false)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, true), new TotalRounded(84, 90, false)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, false, true), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true), new TotalRounded(84, 90, false)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f), new TotalRounded(84, 90, false)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true), new TotalRounded(84, 90, false)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true), new TotalRounded(84, 90, false)},
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, true), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, false, true), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f)},
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true) },
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true) },
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new TotalRounded(84, 90, false)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false)},
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f)},
                },
                new List<object>()
                {
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                },
                new List<object>()
                {
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, false)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, false, true)},
                },
                new List<object>()
                {
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f)},
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(84), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(84), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true)},
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, false)},
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(84), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, false, true), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false) },
                },
                new List<object>()
                {
                    new List<Line>() { new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f)},

                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 140f), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 140f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 140f), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height : 84f), new Constant(height : 84f) },
                    new List<Line>() { new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new Constant(height: 84f), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new CornerRounded(84, 90, 0.2f, 100, true, false), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new Constant(height: 84f), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new CornerRounded(84, 90, 0.2f, 100, true, false), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},

                    new List<Line>() { new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                    new List<Line>() { new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new TotalRounded(84, 90, true), new Constant(height: 84f), new Constant(height: 84f)},
                },
            } },
        };
        private Dictionary<string, List<List<Type>>> ParameterToTypes { get; set; } = new Dictionary<string, List<List<Type>>>()
        {
            {"Length", new List<List<Type>>()
            {
                new List<Type>(){typeof(Coupe), typeof(SUV)},
                new List<Type>(){typeof(Sedan), typeof(Hatchback)},
                new List<Type>(){ typeof(Sedan), typeof(StationWagon), typeof(Pickup), typeof(Minivan) },
                new List<Type>(){ typeof(Sedan), typeof(StationWagon), typeof(Pickup), typeof(Minivan) },
                new List<Type>(){ typeof(Limousine) },
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"Width", new List<List<Type>>()
            {
                new List<Type>(){typeof(Hatchback)},
                new List<Type>(){ typeof(Sedan), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(SUV), typeof(Pickup), typeof(Minivan) },
                new List<Type>(){typeof(Sedan), typeof(Limousine), typeof(Minivan)},
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"Height", new List<List<Type>>()
            {
                new List<Type>(){typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Pickup)},
                new List<Type>(){typeof(Sedan), typeof(Limousine), typeof(SUV), typeof(Pickup)},
                new List<Type>(){typeof(SUV), typeof(Pickup), typeof(Minivan)},
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"WheelWidth", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(SUV), typeof(Pickup), typeof(Minivan), typeof(Van), typeof(Truck) },
            } },
            {"WheelRadius", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(Pickup), typeof(Minivan), typeof(Van) },
                new List<Type>(){ typeof(SUV), typeof(Pickup), typeof(Truck) },
            } },
            {"WheelRelativeBiasAlongWidth", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(SUV), typeof(Minivan), typeof(Van), typeof(Truck) },
            } },
            {"WheelRelativeBiasesAlongLength", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(Coupe), typeof(SUV), typeof(Minivan) },
                new List<Type>(){ typeof(StationWagon), typeof(Pickup), typeof(Van) },
                new List<Type>(){ typeof(Limousine) },
                new List<Type>(){typeof(Truck)},
            } },
            {"WheelBaseSegmentsSpans", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(SUV), typeof(Pickup), typeof(Minivan) },
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"WheelBaseSegmentsBottomSurfaces", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Limousine), typeof(Pickup), typeof(Minivan) },
                new List<Type>(){ typeof(Coupe) },
                new List<Type>(){ typeof(SUV), typeof(Pickup) },
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"WheelBaseTopSurface", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(SUV), typeof(Pickup), typeof(Minivan), typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"GapBetweenWheelAndBase", new List<List<Type>>()
            {
                new List<Type>(){ typeof(Sedan), typeof(Hatchback), typeof(StationWagon), typeof(Coupe), typeof(Limousine), typeof(Pickup), typeof(Minivan), typeof(Van), typeof(Truck) },
                new List<Type>{ typeof(SUV) },
            } },
            {"BodySegmentsSpans", new List<List<Type>>()
            {
                new List<Type>(){typeof(Sedan), typeof(Hatchback)},
                new List<Type>(){typeof(Hatchback), typeof(StationWagon), typeof(SUV)},
                new List<Type>(){typeof(Sedan), typeof(Hatchback), typeof(Coupe)},
                new List<Type>(){typeof(Coupe)},
                new List<Type>(){typeof(Limousine)},
                new List<Type>(){typeof(Pickup)},
                new List<Type>(){typeof(Minivan)},
                new List<Type>(){ typeof(Van) },
                new List<Type>(){typeof(Truck)},
            } },
            {"BodySegmentsTopSurfaces", new List<List<Type>>()
            {
                new List<Type>(){typeof(Sedan), typeof(Limousine)},
                new List<Type>(){typeof(Sedan), typeof(Limousine), typeof(Van)},
                new List<Type>(){typeof(Sedan), typeof(Limousine), typeof(Pickup), typeof(Van), typeof(Truck)},
                new List<Type>(){typeof(Hatchback), typeof(Minivan)},
                new List<Type>(){typeof(Hatchback), typeof(StationWagon), typeof(SUV), typeof(Minivan)},
                new List<Type>(){typeof(Hatchback), typeof(StationWagon), typeof(SUV), typeof(Minivan), typeof(Truck)},
                new List<Type>(){typeof(Hatchback), typeof(StationWagon), typeof(Minivan)},
                new List<Type>(){typeof(Sedan), typeof(Coupe), typeof(Limousine), typeof(Van)},
                new List<Type>(){typeof(Sedan), typeof(Coupe), typeof(Limousine), typeof(Pickup), typeof(Van), typeof(Truck)},
                new List<Type>(){typeof(Van)},
                new List<Type>(){typeof(Truck)},
            } },
        };
        internal List<object> GetDefaultValues(string parameterName, Type type)
        {
            List<object> result = new List<object>();
            List<List<Type>> typesLists = ParameterToTypes[parameterName];
            List<int> valuesInds = new List<int>();
            for (int i = 0; i < typesLists.Count; i++)
            {
                List<Type> list = typesLists[i];
                if (list.Contains(type))
                {
                    valuesInds.Add(i);
                }
            }
            if (valuesInds.Count > 0)
            {
                List<List<object>> values = ParameterToValues[parameterName];
                for (int i = 0; i < values.Count; i++)
                {
                    if (!valuesInds.Contains(i))
                    {
                        continue;
                    }
                    foreach (object value in values[i])
                    {
                        if (!result.Contains(value))
                        {
                            object newValue = value;
                            if (value is ICloneable)
                            {
                                ICloneable clone = (ICloneable)value;
                                newValue = clone.Clone();
                            }
                            else if (value is List<Line>)
                            {
                                List<Line> buffer = new List<Line>();
                                List<Line> valueList = value as List<Line>;
                                foreach (Line item in valueList)
                                {
                                    Line clone = item.Clone() as Line;
                                    buffer.Add(clone);
                                }
                                newValue = buffer;
                            }
                            result.Add(newValue);
                        }
                    }
                }
            }
            List<object> finalResult = new List<object>();
            List<string> numParameters = new List<string>() { "Length", "Width", "Height", "WheelRadius" };
            if (numParameters.Contains(parameterName))
            {
                int min = result.Select(x => System.Convert.ToInt32(x)).Min();
                int max = result.Select(x => System.Convert.ToInt32(x)).Max();
                List<float> interpolated = Interpolate(min, max, NumValuesCount[parameterName], 5);
                foreach (float value in interpolated)
                {
                    finalResult.Add(value);
                }
            }
            else
            {
                finalResult = result;
            }
            return finalResult;
        }
        internal List<float> Interpolate(float first, float last, float numInnerValues, float roundBy = -1)
        {
            List<float> result = new List<float>() { first};
            float step = (last - first) / (numInnerValues + 1);
            for (int i = 1; i < numInnerValues + 1; i++)
            {
                float value = result[i - 1] + step;
                if (roundBy != -1)
                {
                    value = (int)(value / roundBy) * roundBy;
                }
                if (!result.Contains(value))
                {
                    result.Add(value);
                }
            }
            if (!result.Contains(last))
            {
                result.Add(last);
            }
            return result;
        }
    }

    public class AClass : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            if (parameter == "Length")
            {
                List<object> values = new List<object>()
                {
                    250, 260, 270, 280, 290, 300, 310, 320, 330, 340, 350, 360
                };
                return values;
            }
            else if (parameter == "Width")
            {
                List<object> values = new List<object>()
                {
                    145, 150, 160
                };
                return values;
            }
            else if (parameter == "Height")
            {
                List<object> values = new List<object>()
                {
                    135, 140, 148
                };
                return values;
            }
            else if (parameter == "WheelWidth")
            {
                List<object> values = new List<object>()
                {
                    15, 27
                };
                return values;
            }
            else if (parameter == "WheelRadius")
            {
                List<object> values = new List<object>()
                {
                    15*2.54f/2 + car.WheelWidth*0.65f, 20*2.54f/2 + car.WheelWidth*0.65f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasAlongWidth")
            {
                List<object> values = new List<object>()
                {
                    0.1f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasesAlongLength")
            {
                List<object> values = new List<object>()
                {
                    new List<float>(){0.2f, 0.8f},
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 1},
                    },
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsBottomSurfaces")
            {
                List<object> values = new List<object>()
                {
                    new List<Line>()
                    {
                        new Constant(12),
                    },
                    new List<Line>()
                    {
                        new Constant(14),
                    },
                    new List<Line>()
                    {
                        new Constant(17),
                    },
                };
                return values;
            }
            else if (parameter == "GapBetweenWheelAndBase")
            {
                List<object> values = new List<object>()
                {
                    3,
                };
                return values;
            }
            else if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 70, 80, 90};
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                List<object> values = new List<object>()
                {
                    new Constant(goodCandidates.Min()),
                };
                return values;
            }
            else if (parameter == "BodySegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.1f},
                        new List<float>(){0.1f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.9f},
                        new List<float>(){0.9f, 1},
                    },
                };
                return values;
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                if (car.BodySegmentsSpans.Count == 2)
                {
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    List<float> heightCandidates = new List<float>() { car.Height * 0.4f, car.Height * 0.5f, car.Height * 0.6f, car.Height * 0.7f, };
                    List<float> validHeights = new List<float>();
                    float minHeight = car.WheelBaseTopSurface.MinHeight;
                    foreach (float height in heightCandidates)
                    {
                        if (height > minHeight)
                        {
                            validHeights.Add(height);
                        }
                    }
                    List<Line> lines_1 = new List<Line>();
                    foreach (float height in validHeights)
                    {
                        lines_1.Add(new Constant(height));
                    }
                    List<float> cornerLengths = new List<float>() { 0.3f, 0.4f, 0.5f, 0.6f };
                    for (int i = 0; i < validHeights.Count; i++)
                    {
                        for (int j = i + 1; j < validHeights.Count; j++)
                        {
                            lines_1.Add(new TotalRounded(validHeights[i], validHeights[j], true));
                            foreach (float length in cornerLengths)
                            {
                                lines_1.Add(new CornerRounded(validHeights[i], validHeights[j], length, car.Length * relativeLength_1, true, false));
                            }
                        }
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    List<Line> lines_2 = new List<Line>()
                    {
                        new Constant(car.Height),
                    };
                    cornerLengths = new List<float>() { 0.2f, 0.3f, 0.4f};
                    List<bool> rounders = new List<bool>() { true, false };
                    foreach (float height in validHeights)
                    {
                        foreach (float length in cornerLengths)
                        {
                            foreach (bool leftRound in rounders)
                            {
                                foreach (bool rightRound in rounders)
                                {
                                    Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_2, leftRound, rightRound);
                                    lines_2.Add(line);
                                }
                            }
                        }
                    }
                    List<object> values = new List<object>();
                    foreach (Line line1 in lines_1)
                    {
                        foreach (Line line2 in lines_2)
                        {
                            if (line1.MaxHeight <= line2.MinHeight)
                            {
                                List<Line> surfaces = new List<Line>()
                                {
                                    line1, line2
                                };
                                values.Add(surfaces);
                            }
                        }
                    }
                    return values;
                }
                else
                {
                    return new List<object>();
                }
            }
            else
            {
                return new List<object>();
            }
        }
    }

    public class BClass : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            if (parameter == "Length")
            {
                List<object> values = new List<object>()
                {
                    360, 370, 380
                };
                return values;
            }
            else if (parameter == "Width")
            {
                List<object> values = new List<object>()
                {
                    155, 160, 165
                };
                return values;
            }
            else if (parameter == "Height")
            {
                List<object> values = new List<object>()
                {
                    135, 140, 148
                };
                return values;
            }
            else if (parameter == "WheelWidth")
            {
                List<object> values = new List<object>()
                {
                    15, 27
                };
                return values;
            }
            else if (parameter == "WheelRadius")
            {
                List<object> values = new List<object>()
                {
                    15*2.54f/2 + car.WheelWidth*0.65f, 20*2.54f/2 + car.WheelWidth*0.65f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasAlongWidth")
            {
                List<object> values = new List<object>()
                {
                    0.1f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasesAlongLength")
            {
                List<object> values = new List<object>()
                {
                    new List<float>(){0.2f, 0.8f},
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 1},
                    },
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsBottomSurfaces")
            {
                List<object> values = new List<object>()
                {
                    new List<Line>()
                    {
                        new Constant(12),
                    },
                    new List<Line>()
                    {
                        new Constant(14),
                    },
                    new List<Line>()
                    {
                        new Constant(17),
                    },
                };
                return values;
            }
            else if (parameter == "GapBetweenWheelAndBase")
            {
                List<object> values = new List<object>()
                {
                    3,
                };
                return values;
            }
            else if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 70, 80, 90 };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                List<object> values = new List<object>()
                {
                    new Constant(goodCandidates.Min()),
                };
                return values;
            }
            else if (parameter == "BodySegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.1f},
                        new List<float>(){0.1f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.9f},
                        new List<float>(){0.9f, 1},
                    },
                };
                return values;
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                if (car.BodySegmentsSpans.Count == 2)
                {
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    List<float> heightCandidates = new List<float>() { car.Height * 0.4f, car.Height * 0.5f, car.Height * 0.6f, car.Height * 0.7f, };
                    List<float> validHeights = new List<float>();
                    float minHeight = car.WheelBaseTopSurface.MinHeight;
                    foreach (float height in heightCandidates)
                    {
                        if (height > minHeight)
                        {
                            validHeights.Add(height);
                        }
                    }
                    List<Line> lines_1 = new List<Line>();
                    foreach (float height in validHeights)
                    {
                        lines_1.Add(new Constant(height));
                    }
                    List<float> cornerLengths = new List<float>() { 0.3f, 0.4f, 0.5f, 0.6f };
                    for (int i = 0; i < validHeights.Count; i++)
                    {
                        for (int j = i + 1; j < validHeights.Count; j++)
                        {
                            lines_1.Add(new TotalRounded(validHeights[i], validHeights[j], true));
                            foreach (float length in cornerLengths)
                            {
                                lines_1.Add(new CornerRounded(validHeights[i], validHeights[j], length, car.Length * relativeLength_1, true, false));
                            }
                        }
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    List<Line> lines_2 = new List<Line>()
                    {
                        new Constant(car.Height),
                    };
                    cornerLengths = new List<float>() { 0.2f, 0.3f, 0.4f };
                    List<bool> rounders = new List<bool>() { true, false };
                    foreach (float height in validHeights)
                    {
                        foreach (float length in cornerLengths)
                        {
                            foreach (bool leftRound in rounders)
                            {
                                foreach (bool rightRound in rounders)
                                {
                                    Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_2, leftRound, rightRound);
                                    lines_2.Add(line);
                                }
                            }
                        }
                    }
                    List<object> values = new List<object>();
                    foreach (Line line1 in lines_1)
                    {
                        foreach (Line line2 in lines_2)
                        {
                            if (line1.MaxHeight <= line2.MinHeight)
                            {
                                List<Line> surfaces = new List<Line>()
                                {
                                    line1, line2
                                };
                                values.Add(surfaces);
                            }
                        }
                    }
                    return values;
                }
                else
                {
                    return new List<object>();
                }
            }
            else
            {
                return new List<object>();
            }
        }
    }

    public class СClass : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            if (parameter == "Length")
            {
                List<object> values = new List<object>()
                {
                    360, 370, 380
                };
                return values;
            }
            else if (parameter == "Width")
            {
                List<object> values = new List<object>()
                {
                    155, 160, 165
                };
                return values;
            }
            else if (parameter == "Height")
            {
                List<object> values = new List<object>()
                {
                    135, 140, 148
                };
                return values;
            }
            else if (parameter == "WheelWidth")
            {
                List<object> values = new List<object>()
                {
                    15, 27
                };
                return values;
            }
            else if (parameter == "WheelRadius")
            {
                List<object> values = new List<object>()
                {
                    15*2.54f/2 + car.WheelWidth*0.65f, 20*2.54f/2 + car.WheelWidth*0.65f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasAlongWidth")
            {
                List<object> values = new List<object>()
                {
                    0.1f
                };
                return values;
            }
            else if (parameter == "WheelRelativeBiasesAlongLength")
            {
                List<object> values = new List<object>()
                {
                    new List<float>(){0.2f, 0.8f},
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 1},
                    },
                };
                return values;
            }
            else if (parameter == "WheelBaseSegmentsBottomSurfaces")
            {
                List<object> values = new List<object>()
                {
                    new List<Line>()
                    {
                        new Constant(12),
                    },
                    new List<Line>()
                    {
                        new Constant(14),
                    },
                    new List<Line>()
                    {
                        new Constant(17),
                    },
                };
                return values;
            }
            else if (parameter == "GapBetweenWheelAndBase")
            {
                List<object> values = new List<object>()
                {
                    3,
                };
                return values;
            }
            else if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 70, 80, 90 };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                List<object> values = new List<object>()
                {
                    new Constant(goodCandidates.Min()),
                };
                return values;
            }
            else if (parameter == "BodySegmentsSpans")
            {
                List<object> values = new List<object>()
                {
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.8f},
                        new List<float>(){0.8f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.3f},
                        new List<float>(){0.3f, 0.1f},
                        new List<float>(){0.1f, 1},
                    },
                    new List<List<float>>()
                    {
                        new List<float>(){0, 0.2f},
                        new List<float>(){0.2f, 0.9f},
                        new List<float>(){0.9f, 1},
                    },
                };
                return values;
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                if (car.BodySegmentsSpans.Count == 2)
                {
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    List<float> heightCandidates = new List<float>() { car.Height * 0.4f, car.Height * 0.5f, car.Height * 0.6f, car.Height * 0.7f, };
                    List<float> validHeights = new List<float>();
                    float minHeight = car.WheelBaseTopSurface.MinHeight;
                    foreach (float height in heightCandidates)
                    {
                        if (height > minHeight)
                        {
                            validHeights.Add(height);
                        }
                    }
                    List<Line> lines_1 = new List<Line>();
                    foreach (float height in validHeights)
                    {
                        lines_1.Add(new Constant(height));
                    }
                    List<float> cornerLengths = new List<float>() { 0.3f, 0.4f, 0.5f, 0.6f };
                    for (int i = 0; i < validHeights.Count; i++)
                    {
                        for (int j = i + 1; j < validHeights.Count; j++)
                        {
                            lines_1.Add(new TotalRounded(validHeights[i], validHeights[j], true));
                            foreach (float length in cornerLengths)
                            {
                                lines_1.Add(new CornerRounded(validHeights[i], validHeights[j], length, car.Length * relativeLength_1, true, false));
                            }
                        }
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    List<Line> lines_2 = new List<Line>()
                    {
                        new Constant(car.Height),
                    };
                    cornerLengths = new List<float>() { 0.2f, 0.3f, 0.4f };
                    List<bool> rounders = new List<bool>() { true, false };
                    foreach (float height in validHeights)
                    {
                        foreach (float length in cornerLengths)
                        {
                            foreach (bool leftRound in rounders)
                            {
                                foreach (bool rightRound in rounders)
                                {
                                    Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_2, leftRound, rightRound);
                                    lines_2.Add(line);
                                }
                            }
                        }
                    }
                    List<object> values = new List<object>();
                    foreach (Line line1 in lines_1)
                    {
                        foreach (Line line2 in lines_2)
                        {
                            if (line1.MaxHeight <= line2.MinHeight)
                            {
                                List<Line> surfaces = new List<Line>()
                                {
                                    line1, line2
                                };
                                values.Add(surfaces);
                            }
                        }
                    }
                    return values;
                }
                else if (car.BodySegmentsSpans.Count == 3)
                {
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    List<float> heightCandidates = new List<float>() { car.Height * 0.4f, car.Height * 0.5f, car.Height * 0.6f, car.Height * 0.7f, };
                    List<float> validHeights = new List<float>();
                    float minHeight = car.WheelBaseTopSurface.MinHeight;
                    foreach (float height in heightCandidates)
                    {
                        if (height > minHeight)
                        {
                            validHeights.Add(height);
                        }
                    }
                    List<Line> lines_1 = new List<Line>();
                    foreach (float height in validHeights)
                    {
                        lines_1.Add(new Constant(height));
                    }
                    List<float> cornerLengths = new List<float>() { 0.3f, 0.4f, 0.5f, 0.6f };
                    for (int i = 0; i < validHeights.Count; i++)
                    {
                        for (int j = i + 1; j < validHeights.Count; j++)
                        {
                            lines_1.Add(new TotalRounded(validHeights[i], validHeights[j], true));
                            foreach (float length in cornerLengths)
                            {
                                lines_1.Add(new CornerRounded(validHeights[i], validHeights[j], length, car.Length * relativeLength_1, true, false));
                            }
                        }
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    List<Line> lines_2 = new List<Line>()
                    {
                        new Constant(car.Height),
                    };
                    cornerLengths = new List<float>() { 0.2f, 0.3f, 0.4f };
                    List<bool> rounders = new List<bool>() { true, false };
                    foreach (float height in validHeights)
                    {
                        foreach (float length in cornerLengths)
                        {
                            foreach (bool leftRound in rounders)
                            {
                                Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_2, leftRound, false);
                                lines_2.Add(line);
                            }
                        }
                    }
                    foreach (float height in validHeights)
                    {
                        lines_2.Add(new TotalRounded(height, car.Height, true));
                    }

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    List<Line> lines_3 = new List<Line>();
                    cornerLengths = new List<float>() { 0.2f, 0.3f, 0.4f, 0.5f, 0.6f };
                    foreach (float height in validHeights)
                    {
                        foreach (float length in cornerLengths)
                        {
                            Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_3, false, true);
                            lines_3.Add(line);
                        }
                    }
                    foreach (float height in validHeights)
                    {
                        lines_3.Add(new TotalRounded(height, car.Height, false));
                    }
                    List<object> values = new List<object>();
                    foreach (Line line1 in lines_1)
                    {
                        foreach (Line line2 in lines_2)
                        {
                            foreach (Line line3 in lines_3)
                            {
                                if (line2.GetType() != line3.GetType() && line1.MaxHeight <= line2.MinHeight)
                                {
                                    List<Line> surfaces = new List<Line>()
                                    {
                                        line1, line2, line3
                                    };
                                    values.Add(surfaces);
                                }
                            }
                        }
                    }
                    return values;
                }
                else
                {
                    return new List<object>();
                }
            }
            else
            {
                return new List<object>();
            }
        }
    }

    public class Sedan : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1*car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    float maxHeight = value[0].MaxHeight;
                    value[2].MinHeight = maxHeight;
                    value[2].MaxHeight = maxHeight;
                    if (value[2] is TotalRounded)
                    {
                        value[2].MinHeight = Math.Max(maxHeight - 0.1f * car.Height, minHeight);
                    }
                    else if (value[2] is CornerRounded)
                    {
                        CornerRounded line = value[2] as CornerRounded;
                        value[2].MinHeight = minHeight;
                        line.TotalLength = relativeLength_3 * car.Length;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Hatchback : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    if (value.Count != car.BodySegmentsSpans.Count)
                    {
                        continue;
                    }
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    if (value.Count == 3)
                    {
                        float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                        value[2].MinHeight = minHeight;
                        value[2].MaxHeight = car.Height;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class StationWagon : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    if (value.Count != car.BodySegmentsSpans.Count)
                    {
                        continue;
                    }
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Coupe : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f*car.Height, 0.6f*car.Height, 0.7f*car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    CornerRounded line = value[1] as CornerRounded;
                    line.MinHeight = value[1].MaxHeight;
                    line.MaxHeight = car.Height;
                    line.TotalLength = relativeLength_2*car.Length;

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    float maxHeight = value[0].MaxHeight;
                    value[2].MinHeight = maxHeight;
                    value[2].MaxHeight = maxHeight;
                    if (value[2] is TotalRounded)
                    {
                        value[2].MaxHeight = car.Height;
                        value[2].MinHeight = 0.8f*car.Height;
                    }
                    else if (value[2] is CornerRounded)
                    {
                        line = value[2] as CornerRounded;
                        value[2].MinHeight = minHeight;
                        line.TotalLength = relativeLength_3 * car.Length;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Limousine : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    float maxHeight = value[0].MaxHeight;
                    value[2].MinHeight = maxHeight;
                    value[2].MaxHeight = maxHeight;
                    if (value[2] is TotalRounded)
                    {
                        value[2].MinHeight = Math.Max(maxHeight - 0.1f * car.Height, minHeight);
                    }
                    else if (value[2] is CornerRounded)
                    {
                        CornerRounded line = value[2] as CornerRounded;
                        value[2].MinHeight = minHeight;
                        line.TotalLength = relativeLength_3 * car.Length;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class SUV : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    if (value.Count != car.BodySegmentsSpans.Count)
                    {
                        continue;
                    }
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Pickup : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    float maxHeight = value[0].MaxHeight;
                    value[2].MinHeight = maxHeight;
                    value[2].MaxHeight = maxHeight;
                    if (value[2] is TotalRounded)
                    {
                        value[2].MinHeight = Math.Max(maxHeight - 0.1f * car.Height, minHeight);
                    }
                    else if (value[2] is CornerRounded)
                    {
                        CornerRounded line = value[2] as CornerRounded;
                        value[2].MinHeight = minHeight;
                        line.TotalLength = relativeLength_3 * car.Length;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Minivan : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseTopSurface")
            {
                List<float> candidates = new List<float>() { 0.5f * car.Height, 0.6f * car.Height, 0.7f * car.Height };
                List<float> goodCandidates = new List<float>();
                foreach (float item in candidates)
                {
                    if (item > car.WheelRadius * 2 + car.GapBetweenWheelAndBase)
                    {
                        goodCandidates.Add(item);
                    }
                }
                result.Add(new Constant(goodCandidates.Min()));
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    if (value.Count != car.BodySegmentsSpans.Count)
                    {
                        continue;
                    }
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.1f * car.Height;
                    value[0].MaxHeight = minHeight + 0.1f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.2f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = car.Height;
                        line.MaxHeight = car.Height;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = car.Height;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    if (value.Count == 3)
                    {
                        float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                        value[2].MinHeight = minHeight;
                        value[2].MaxHeight = car.Height;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Van : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float minHeight = car.WheelBaseTopSurface.MaxHeight;
                    value[0].MinHeight = minHeight + 0.05f * car.Height;
                    value[0].MaxHeight = minHeight + 0.05f * car.Height;
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = minHeight;
                    }
                    else if (value[0] is CornerRounded)
                    {
                        CornerRounded line = value[0] as CornerRounded;
                        value[0].MinHeight = minHeight;
                        value[0].MaxHeight = minHeight + 0.1f * car.Height;
                        line.TotalLength = relativeLength_1 * car.Length;
                    }

                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    float height_1 = 0;
                    float height_2 = 0;
                    if (car.Height < 230)
                    {
                        height_1 = car.Height;
                        height_2 = 0.5f * car.Height;
                        if (value[2] is CornerRounded)
                        {
                            CornerRounded line = value[2] as CornerRounded;
                            if (line.LeftCorner || line.RightCorner)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        height_1 = 0.8f * car.Height;
                        height_2 = car.Height;
                    }
                    if (value[1] is Constant)
                    {
                        Constant line = value[1] as Constant;
                        line.MinHeight = height_1;
                        line.MaxHeight = height_1;
                    }
                    else if (value[1] is CornerRounded)
                    {
                        CornerRounded line = value[1] as CornerRounded;
                        line.MinHeight = value[0].MaxHeight;
                        line.MaxHeight = height_1;
                        line.TotalLength = relativeLength_2 * car.Length;
                    }

                    float relativeLength_3 = car.BodySegmentsSpans[2][1] - car.BodySegmentsSpans[2][0];
                    value[2].MinHeight = height_2;
                    value[2].MaxHeight = height_2;
                    if (value[2] is CornerRounded)
                    {
                        CornerRounded line = value[2] as CornerRounded;
                        line.TotalLength = relativeLength_3 * car.Length;
                        value[2].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                    }
                    result.Add(value);
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
    public class Truck : CarType
    {
        internal override List<object> GetValues(string parameter, Car car)
        {
            List<object> result = new List<object>();
            List<object> defaults = GetDefaultValues(parameter, this.GetType());
            if (parameter == "WheelBaseSegmentsSpans")
            {
                foreach (object objValue in defaults)
                {
                    List<List<float>> value = objValue as List<List<float>>;
                    if (value[0][1] > car.WheelRelativeBiasesAlongLength[2] && value[1][0] < car.WheelRelativeBiasesAlongLength[3])
                    {
                        result.Add(value);
                    }
                }
            }
            else if (parameter == "BodySegmentsSpans")
            {
                foreach (object objValue in defaults)
                {
                    List<List<float>> value = objValue as List<List<float>>;
                    float cargoStart = 0;
                    int cargoCount = 0;
                    for (int i = 0; i < value.Count; i++)
                    {
                        List<float> span = value[i];
                        if (span[1] - span[0] > 0.17)
                        {
                            cargoStart = span[0];
                            cargoCount = value.Count - i;
                            break;
                        }
                    }
                    foreach (List<float> span in value)
                    {
                        if (span[1] - span[0] > 0.17)
                        {
                            cargoStart = span[0];
                            break;
                        }
                    }
                    if (cargoStart < car.WheelBaseSegmentsSpans[0][1])
                    {
                        if (cargoCount == 2 && car.WheelRelativeBiasesAlongLength.Count == 7 || cargoCount == 1 && car.WheelRelativeBiasesAlongLength.Count == 6)
                        {
                            result.Add(value);
                        }
                    }
                }
            }
            else if (parameter == "BodySegmentsTopSurfaces")
            {
                foreach (object objValue in defaults)
                {
                    List<Line> value = objValue as List<Line>;
                    if (value.Count != car.BodySegmentsSpans.Count)
                    {
                        continue;
                    }
                    float relativeLength_1 = car.BodySegmentsSpans[0][1] - car.BodySegmentsSpans[0][0];
                    float relativeLength_2 = car.BodySegmentsSpans[1][1] - car.BodySegmentsSpans[1][0];
                    float relativeLength_3 = 0;
                    float relativeLength_4 = 0;
                    float relativeLength_5 = 0;
                    int cargoCount = 0;
                    for (int i = 0; i < car.BodySegmentsSpans.Count; i++)
                    {
                        List<float> span = car.BodySegmentsSpans[i];
                        if (span[1] - span[0] > 0.17)
                        {
                            cargoCount = value.Count - i;
                            break;
                        }
                    }
                    float cabinHeight = 0.7f * car.Height;
                    float bonnetHeight = 0.5f * car.Height;
                    float cargoHeight = car.Height;
                    if (car.Height < 300)
                    {
                        cargoHeight = 120;
                    }
                    value[value.Count-1].MinHeight = cargoHeight;
                    value[value.Count - 1].MaxHeight = cargoHeight;
                    if (cargoCount == 2)
                    {
                        value[value.Count - 2].MinHeight = cargoHeight;
                        value[value.Count - 2].MaxHeight = cargoHeight;
                    }
                    if (value.Count == 3)
                    {
                        if (cargoCount == 1)
                        {
                            if (value[1] is TotalRounded)
                            {
                                value[0].MinHeight = cabinHeight;
                                value[0].MaxHeight = cabinHeight;
                                value[1].MinHeight = cabinHeight;
                                value[1].MaxHeight = car.Height;
                                if (value[0] is CornerRounded)
                                {
                                    CornerRounded line = value[0] as CornerRounded;
                                    line.TotalLength = relativeLength_1;
                                    value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                                }
                            }
                            else
                            {
                                value[0].MinHeight = bonnetHeight;
                                value[0].MaxHeight = bonnetHeight;
                                value[1].MinHeight = cabinHeight;
                                value[1].MaxHeight = cabinHeight;
                                if (value[0] is CornerRounded)
                                {
                                    CornerRounded line = value[0] as CornerRounded;
                                    line.TotalLength = relativeLength_1;
                                    value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                                }
                                if (value[1] is CornerRounded)
                                {
                                    CornerRounded line = value[1] as CornerRounded;
                                    line.TotalLength = relativeLength_2;
                                    value[1].MinHeight = value[0].MaxHeight;
                                }
                            }
                        }
                        else if (cargoCount == 2)
                        {
                            value[0].MinHeight = cabinHeight;
                            value[0].MaxHeight = cabinHeight;
                            if (value[0] is CornerRounded)
                            {
                                CornerRounded line = value[0] as CornerRounded;
                                line.TotalLength = relativeLength_1;
                                value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        result.Add(value);
                    }
                    else if (value.Count == 4)
                    {
                        if (cargoCount == 1)
                        {
                            value[0].MinHeight = bonnetHeight;
                            value[0].MaxHeight = bonnetHeight;
                            value[1].MinHeight = cabinHeight;
                            value[1].MaxHeight = cabinHeight;
                            value[2].MinHeight = cabinHeight;
                            value[2].MaxHeight = car.Height;
                            if (value[0] is CornerRounded)
                            {
                                CornerRounded line = value[0] as CornerRounded;
                                line.TotalLength = relativeLength_1;
                                value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                            }
                            if (value[1] is CornerRounded)
                            {
                                CornerRounded line = value[1] as CornerRounded;
                                line.TotalLength = relativeLength_2;
                                value[1].MinHeight = value[0].MaxHeight;
                            }
                        }
                        else if (cargoCount == 2)
                        {
                            if (value[1] is TotalRounded)
                            {
                                value[0].MinHeight = cabinHeight;
                                value[0].MaxHeight = cabinHeight;
                                value[1].MinHeight = cabinHeight;
                                value[1].MaxHeight = car.Height;
                                if (value[0] is CornerRounded)
                                {
                                    CornerRounded line = value[0] as CornerRounded;
                                    line.TotalLength = relativeLength_1;
                                    value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                                }
                            }
                            else
                            {
                                value[0].MinHeight = bonnetHeight;
                                value[0].MaxHeight = bonnetHeight;
                                value[1].MinHeight = cabinHeight;
                                value[1].MaxHeight = cabinHeight;
                                if (value[0] is CornerRounded)
                                {
                                    CornerRounded line = value[0] as CornerRounded;
                                    line.TotalLength = relativeLength_1;
                                    value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                                }
                                if (value[1] is CornerRounded)
                                {
                                    CornerRounded line = value[1] as CornerRounded;
                                    line.TotalLength = relativeLength_2;
                                    value[1].MinHeight = value[0].MaxHeight;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                        result.Add(value);
                    }
                    else if (value.Count == 5)
                    {
                        value[0].MinHeight = bonnetHeight;
                        value[0].MaxHeight = bonnetHeight;
                        value[1].MinHeight = cabinHeight;
                        value[1].MaxHeight = cabinHeight;
                        value[2].MinHeight = cabinHeight;
                        value[2].MaxHeight = car.Height;
                        if (value[0] is CornerRounded)
                        {
                            CornerRounded line = value[0] as CornerRounded;
                            line.TotalLength = relativeLength_1;
                            value[0].MinHeight = car.WheelBaseTopSurface.MaxHeight;
                        }
                        if (value[1] is CornerRounded)
                        {
                            CornerRounded line = value[1] as CornerRounded;
                            line.TotalLength = relativeLength_2;
                            value[1].MinHeight = value[0].MaxHeight;
                        }
                        result.Add(value);
                    }
                    if (value[0] is TotalRounded)
                    {
                        value[0].MinHeight = 0.9f * bonnetHeight;
                    }
                }
            }
            else
            {
                result = defaults;
            }
            return result;
        }
    }
}
