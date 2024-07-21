using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.Cars
{
    public abstract class CarType
    {
        public List<Car> GetCars(int count = -1)
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
                        field.SetValue(newCar, value);
                        newCars.Add(newCar);
                    }
                }
                if (newCars.Count > 0)
                {
                    cars = newCars;
                }
            }
            float shift = 500;
            int ind = 0;
            List<Car> finalCars = new List<Car>();
            if (count == -1)
            {
                for (int i = 0; i < cars.Count; i++)
                {
                    Car car = cars[i];
                    car.BasePoint.X = i * shift;
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
                    randCar.BasePoint.X = i * shift;
                    finalCars.Add(randCar);
                }
            }
            return finalCars;
        }
        public abstract List<object> GetValues(string parameter, Car car); 
    }

    public class AClass : CarType
    {
        public override List<object> GetValues(string parameter, Car car)
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
                            Line line = new CornerRounded(height, car.Height, length, car.Length * relativeLength_2, false, true);
                            lines_2.Add(line);
                        }
                    }
                    foreach (float height in validHeights)
                    {
                        lines_2.Add(new TotalRounded(height, car.Height, false));
                    }
                    List<object> values = new List<object>();
                    foreach (Line line1 in lines_1)
                    {
                        foreach (Line line2 in lines_2)
                        {
                            foreach(Line line3 in lines_3)
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
}
