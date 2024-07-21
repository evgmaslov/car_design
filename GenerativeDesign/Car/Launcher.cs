using Leap71.ShapeKernel;
using PicoGK;
using GenerativeDesign.Cars;

namespace GenerativeDesign
{
    public class Launcher
    {
        public static void Task()
        {
            try
            {
                #region Car
                Car car = new Car();
                Voxels voxCar = car.voxConstruct();
                #endregion

                Sh.PreviewVoxels(voxCar, Cp.clrRock);
            }
            catch (Exception e)
            {
                Library.Log($"Failed run example: \n{e.Message}"); ;
            }
        }

        public static void SampleCars()
        {
            AClass aClass = new AClass();
            List<Car> cars = aClass.GetCars(10);
            foreach (Car car in cars)
            {
                Sh.PreviewVoxels(car.voxConstruct(), Cp.clrRock);
            }
        }

        public static void GenerateDataset()
        {
            AClass aClass = new AClass();
            List<Car> cars = aClass.GetCars();
            string path = "C:\\Все_файлы\\Научная_деятельность\\PikoGK\\PicoGK Example\\GenerativeDesign\\Car.cs";
            string file = File.ReadAllText(path);
            List<string> texts = new List<string>();
            for (int i = 0; i < cars.Count; i++)
            {
                Car car = cars[i];
                //string pattern = @"(FIELD =)[^;]+(;\s+)";
                //Regex re = new Regex(pattern);
                //string fieldName = "FIELD";
                //string value = "";
                //string baseReplacement = @"$1 VALUE$2";
                //string replacement = "";

                //fieldName = "Length";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.Length.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "Width";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.Width.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "Height";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.Height.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelWidth";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.WheelWidth.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelRadius";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.WheelRadius.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelRelativeBiasAlongWidth";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.WheelRelativeBiasAlongWidth.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelRelativeBiasesAlongLength";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //List<string> formattedBiases = new List<string>();
                //foreach (float bias in car.WheelRelativeBiasesAlongLength)
                //{
                //    formattedBiases.Add(bias.ToString() + "f");
                //}
                //value = $"new List<float>() {{ {String.Join(", ", formattedBiases)} }}";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelBaseSegmentsSpans";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //List<string> spans = new List<string>();
                //foreach (List<float> span in car.WheelBaseSegmentsSpans)
                //{
                //    string strSpan = $"new List<float>(){{ {span[0] + "f"}, {span[1] + "f"} }}";
                //    spans.Add(strSpan);
                //}
                //value = $"new List<List<float>>() {{ {String.Join(", ", spans)} }}";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelBaseSegmentsBottomSurfaces";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //List<string> bottomSurfaces = new List<string>();
                //foreach (Line surf in car.WheelBaseSegmentsBottomSurfaces)
                //{
                //    bottomSurfaces.Add(surf.ToString());
                //}
                //value = $"new List<Line>() {{ {String.Join(", ", bottomSurfaces)} }}";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "WheelBaseTopSurface";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.WheelBaseTopSurface.ToString();
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "GapBetweenWheelAndBase";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //value = car.GapBetweenWheelAndBase.ToString() + "f";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "BodySegmentsSpans";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //List<string> bodySpans = new List<string>();
                //foreach (List<float> span in car.BodySegmentsSpans)
                //{
                //    string strSpan = $"new List<float>(){{ {span[0] + "f"}, {span[1] + "f"} }}";
                //    bodySpans.Add(strSpan);
                //}
                //value = $"new List<List<float>>() {{ {String.Join(", ", bodySpans)} }}";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //fieldName = "BodySegmentsTopSurfaces";
                //re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                //List<string> topSurfaces = new List<string>();
                //foreach (Line surf in car.BodySegmentsTopSurfaces)
                //{
                //    topSurfaces.Add(surf.ToString());
                //}
                //value = $"new List<Line>() {{ {String.Join(", ", topSurfaces)} }}";
                //replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                //file = re.Replace(file, replacement);

                //string newPath = $"C:\\Все_файлы\\Научная_деятельность\\PikoGK\\CarsDataset\\OutputCode\\Code_{i}.cs";
                //using (StreamWriter writer = new StreamWriter(newPath))
                //{
                //    writer.WriteLine(file);
                //}


                string text = "Напиши код для генерации автомобиля со следующими параметрами:";
                text = text + $" длина: {car.Length} см;";
                text = text + $" ширина: {car.Width} см;";
                text = text + $" высота: {car.Height} см;";
                text = text + $" ширина колеса: {car.WheelWidth} см;";
                text = text + $" радиус колеса: {car.WheelRadius} см;";
                float minShift = float.MaxValue;
                foreach (Line surf in car.WheelBaseSegmentsBottomSurfaces)
                {
                    if (surf.MinHeight < minShift)
                    {
                        minShift = surf.MinHeight;
                    }
                }
                text = text + $" высота дорожного просвета: {minShift} см;";

                List<List<float>> segmentSpans = car.BodySegmentsSpans;
                List<Line> segmentSurfaces = car.BodySegmentsTopSurfaces;
                if (segmentSpans[0][1] <= 0.2f)
                {
                    text = text + $" размер капота: малый;";
                }
                else if (segmentSpans[0][1] >= 0.3f)
                {
                    text = text + $" размер капота: большой;";
                }
                if (segmentSurfaces[0] is CornerRounded && ((CornerRounded)segmentSurfaces[0]).LeftCorner || segmentSurfaces[0] is TotalRounded)
                {
                    text = text + $" форма капота: скругленная;";
                }
                else if (segmentSurfaces[0] is Constant)
                {
                    text = text + $" форма капота: прямая;";
                }

                if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner)) || 
                    (segmentSpans.Count == 3 && (segmentSurfaces[1] is TotalRounded || segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner)))
                {
                    text = text + $" форма лобового стекла: скругленная;";
                }
                else if (segmentSurfaces[1] is Constant || segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner == false)
                {
                    text = text + $" форма лобового стекла: прямая;";
                }

                if (segmentSpans.Count == 3)
                {
                    if (segmentSpans[2][0] >= 0.9f)
                    {
                        text = text + $" размер багажника: малый;";
                    }
                    else if (segmentSpans[2][0] <= 0.8f)
                    {
                        text = text + $" размер багажника: средний;";
                    }
                }
                if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner)) ||
                    (segmentSpans.Count == 3 && (segmentSurfaces[2] is TotalRounded || segmentSurfaces[2] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner)))
                {
                    text = text + $" форма багажника: скругленная;";
                }
                else if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner == false || segmentSurfaces[1] is Constant)) ||
                    (segmentSpans.Count == 3 && (segmentSurfaces[2] is Constant || segmentSurfaces[2] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner == false)))
                {
                    text = text + $" форма багажника: прямая;";
                }

                texts.Add(text);
            }
            string textsPath = $"C:\\Все_файлы\\Научная_деятельность\\PikoGK\\CarsDataset\\InputTexts.txt";
            using (StreamWriter writer = new StreamWriter(textsPath))
            {
                writer.Write(String.Join("\n", texts));
            }
        }
    }
}
