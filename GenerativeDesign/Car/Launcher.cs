using Leap71.ShapeKernel;
using PicoGK;
using GenerativeDesign.Cars;
using System.Text.RegularExpressions;
using System.Numerics;

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
            //Truck sedan = new Truck();
            //sedan.NumValuesCount["Height"] = 2;
            //List<Car> cars = sedan.GetCars(5);
            //foreach (Car car in cars)
            //{
            //    Sh.PreviewVoxels(car.voxConstruct(), Cp.clrRock);
            //}

            List<Car> cars = new List<Car>();
            CarType carType = new Sedan();
            cars = cars.Concat(carType.GetCars(1)).ToList();
            carType = new Hatchback();
            cars = cars.Concat(carType.GetCars(1, baseY: 300)).ToList();
            carType = new StationWagon();
            cars = cars.Concat(carType.GetCars(1, baseY: 600)).ToList();
            carType = new Coupe();
            cars = cars.Concat(carType.GetCars(1, baseY: 900)).ToList();
            carType = new Limousine();
            cars = cars.Concat(carType.GetCars(1, baseY: 1200)).ToList();
            carType = new SUV();
            cars = cars.Concat(carType.GetCars(1, baseY: 1500)).ToList();
            carType = new Pickup();
            cars = cars.Concat(carType.GetCars(1, baseY: 1800)).ToList();
            carType = new Minivan();
            cars = cars.Concat(carType.GetCars(1, baseY: 2100)).ToList();
            carType = new Van();
            carType.NumValuesCount["Height"] = 2;
            cars = cars.Concat(carType.GetCars(1, baseY: 2400)).ToList();
            carType = new Truck();
            carType.NumValuesCount["Height"] = 2;
            cars = cars.Concat(carType.GetCars(1, baseY: 2700)).ToList();
            foreach (Car car in cars)
            {
                Sh.PreviewVoxels(car.voxConstruct(), Cp.clrRock);
            }
        }

        public static void GenerateDataset()
        {
            AClass aClass = new AClass();
            List<Car> cars = aClass.GetCars();
            string path = "C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\generative_design\\GenerativeDesign\\Car\\Car.cs";
            string file = File.ReadAllText(path);
            List<string> texts_ru = new List<string>();
            List<string> texts_en = new List<string>();
            for (int i = 0; i < cars.Count; i++)
            {
                Car car = cars[i];
                string pattern = @"(FIELD =)[^;]+(;\s+)";
                Regex re = new Regex(pattern);
                string fieldName = "FIELD";
                string value = "";
                string baseReplacement = @"$1 VALUE$2";
                string replacement = "";

                fieldName = "Length";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.Length.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "Width";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.Width.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "Height";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.Height.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelWidth";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.WheelWidth.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelRadius";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.WheelRadius.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelRelativeBiasAlongWidth";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.WheelRelativeBiasAlongWidth.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelRelativeBiasesAlongLength";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                List<string> formattedBiases = new List<string>();
                foreach (float bias in car.WheelRelativeBiasesAlongLength)
                {
                    formattedBiases.Add(bias.ToString() + "f");
                }
                value = $"new List<float>() {{ {String.Join(", ", formattedBiases)} }}";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelBaseSegmentsSpans";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                List<string> spans = new List<string>();
                foreach (List<float> span in car.WheelBaseSegmentsSpans)
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
                foreach (Line surf in car.WheelBaseSegmentsBottomSurfaces)
                {
                    bottomSurfaces.Add(surf.ToString());
                }
                value = $"new List<Line>() {{ {String.Join(", ", bottomSurfaces)} }}";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "WheelBaseTopSurface";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.WheelBaseTopSurface.ToString();
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "GapBetweenWheelAndBase";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                value = car.GapBetweenWheelAndBase.ToString() + "f";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                fieldName = "BodySegmentsSpans";
                re = new Regex(Regex.Replace(pattern, "FIELD", $"{fieldName}"));
                List<string> bodySpans = new List<string>();
                foreach (List<float> span in car.BodySegmentsSpans)
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
                foreach (Line surf in car.BodySegmentsTopSurfaces)
                {
                    topSurfaces.Add(surf.ToString());
                }
                value = $"new List<Line>() {{ {String.Join(", ", topSurfaces)} }}";
                replacement = Regex.Replace(baseReplacement, "VALUE", $"{value}");
                file = re.Replace(file, replacement);

                string newPath = $"C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset\\OutputCode\\Code_{i}.cs";
                using (StreamWriter writer = new StreamWriter(newPath))
                {
                    writer.WriteLine(file);
                }


                string text_ru = "Напиши код для генерации автомобиля со следующими параметрами:";
                text_ru = text_ru + $" длина: {car.Length} см;";
                text_ru = text_ru + $" ширина: {car.Width} см;";
                text_ru = text_ru + $" высота: {car.Height} см;";
                text_ru = text_ru + $" ширина колеса: {car.WheelWidth} см;";
                text_ru = text_ru + $" радиус колеса: {car.WheelRadius} см;";

                string text_en = "Generate a car with the following characteristics:";
                text_en = text_en + $" length: {car.Length} cm;";
                text_en = text_en + $" width: {car.Width} cm;";
                text_en = text_en + $" height: {car.Height} cm;";
                text_en = text_en + $" wheel width: {car.WheelWidth} cm;";
                text_en = text_en + $" wheel radius: {car.WheelRadius} cm;";
                float minShift = float.MaxValue;
                foreach (Line surf in car.WheelBaseSegmentsBottomSurfaces)
                {
                    if (surf.MinHeight < minShift)
                    {
                        minShift = surf.MinHeight;
                    }
                }
                text_ru = text_ru + $" высота дорожного просвета: {minShift} см;";
                text_en = text_en + $" clearance height: {minShift} cm;";

                List<List<float>> segmentSpans = car.BodySegmentsSpans;
                List<Line> segmentSurfaces = car.BodySegmentsTopSurfaces;
                if (segmentSpans[0][1] <= 0.2f)
                {
                    text_ru = text_ru + $" размер капота: малый;";
                    text_en = text_en + $" bonnet size : small;";
                }
                else if (segmentSpans[0][1] >= 0.3f)
                {
                    text_ru = text_ru + $" размер капота: большой;";
                    text_en = text_en + $" bonnet size : large;";
                }
                if (segmentSurfaces[0] is CornerRounded && ((CornerRounded)segmentSurfaces[0]).LeftCorner || segmentSurfaces[0] is TotalRounded)
                {
                    text_ru = text_ru + $" форма капота: скругленная;";
                    text_en = text_en + $" bonnet shape : rounded;";
                }
                else if (segmentSurfaces[0] is Constant)
                {
                    text_ru = text_ru + $" форма капота: прямая;";
                    text_en = text_en + $" bonnet shape : flat;";
                }

                if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner)) || 
                    (segmentSpans.Count == 3 && (segmentSurfaces[1] is TotalRounded || segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner)))
                {
                    text_ru = text_ru + $" форма лобового стекла: скругленная;";
                    text_en = text_en + $" windscreen shape: rounded;";
                }
                else if (segmentSurfaces[1] is Constant || segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).LeftCorner == false)
                {
                    text_ru = text_ru + $" форма лобового стекла: прямая;";
                    text_en = text_en + $" windscreen shape: flat;";
                }

                if (segmentSpans.Count == 3)
                {
                    if (segmentSpans[2][0] >= 0.9f)
                    {
                        text_ru = text_ru + $" размер багажника: малый;";
                        text_en = text_en + $" boot size: small;";
                    }
                    else if (segmentSpans[2][0] <= 0.8f)
                    {
                        text_ru = text_ru + $" размер багажника: средний;";
                        text_en = text_en + $" boot size: medium;";
                    }
                }
                if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner)) ||
                    (segmentSpans.Count == 3 && (segmentSurfaces[2] is TotalRounded || segmentSurfaces[2] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner)))
                {
                    text_ru = text_ru + $" форма багажника: скругленная;";
                    text_en = text_en + $" boot shape: rounded;";
                }
                else if ((segmentSpans.Count == 2 && (segmentSurfaces[1] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner == false || segmentSurfaces[1] is Constant)) ||
                    (segmentSpans.Count == 3 && (segmentSurfaces[2] is Constant || segmentSurfaces[2] is CornerRounded && ((CornerRounded)segmentSurfaces[1]).RightCorner == false)))
                {
                    text_ru = text_ru + $" форма багажника: прямая;";
                    text_en = text_en + $" boot shape: flat;";
                }

                texts_ru.Add(text_ru);
                texts_en.Add(text_en);
            }
            string textsPath = $"C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset";
            List<string> textsFiles = new List<string>() { "InputTextsRu.txt", "InputTextsEn.txt" };
            List<List<string>> texts = new List<List<string>>() { texts_ru, texts_en};
            for (int i = 0; i < texts.Count; i++)
            {
                List<string> localTexts = texts[i];
                string localFile = textsFiles[i];
                string localPath = Path.Combine(textsPath, localFile);
                using (StreamWriter writer = new StreamWriter(localPath))
                {
                    writer.Write(String.Join("\n", localTexts));
                }
            }
        }
        
        public static void SampleFromDataset()
        {
            string datasetPath = "C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset";
            int n_samples = 5;

            List<string> allCodes = Directory.GetFiles(Path.Combine(datasetPath, "OutputCode")).ToList();
            List<int> randomInds = new List<int>();
            Random random = new Random();
            for (int i = 0; i < n_samples; i++)
            {
                randomInds.Add(random.Next(allCodes.Count));
            }
            List<string> carPaths = new List<string>();
            for (int i = 0; i < n_samples; i++)
            {
                carPaths.Add(Path.Combine(datasetPath, $"OutputCode\\Code_{randomInds[i]}.cs"));
            }
            float shift = 500;
            int ind = 0;
            foreach (string sourcePath in carPaths)
            {
                string code = "";
                using (StreamReader reader = new StreamReader($"C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset\\OutputCode\\Code_{randomInds[ind]}.cs"))
                {
                    code = reader.ReadToEnd();
                }
                RoslynCompiler compiler = new RoslynCompiler("GenerativeDesign.Cars.Car", code, new[] { typeof(Console), typeof(CarBase), typeof(List<>) });
                Type carType = compiler.Compile();
                object carInstance = Activator.CreateInstance(carType);

                Vector3 position = new Vector3(shift * ind, 0, 0);
                carType.GetField("BasePoint").SetValue(carInstance, position);
                object objVoxels = carType.GetMethod("voxConstruct").Invoke(carInstance, new object[] { });
                Voxels voxCar = (Voxels)objVoxels;
                Sh.PreviewVoxels(voxCar, Cp.clrRock);
                ind++;
            }
        }

        public static void FixDataset()
        {
            List<string> allCodes = Directory.GetFiles("C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset\\OutputCode").ToList();
            foreach (string path in allCodes)
            {
                string oldcode = "";
                using (StreamReader reader = new StreamReader(path))
                {
                    oldcode = reader.ReadToEnd();
                }
                string insertion = "using System;\r\nusing System.Collections;\r\n";
                string newCode = insertion + oldcode;
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(newCode);
                }
            }
        }
    }
}
