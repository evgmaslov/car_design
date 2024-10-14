using Leap71.ShapeKernel;
using PicoGK;
using GenerativeDesign.Cars;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Reflection;
using System.Reflection.Emit;

namespace GenerativeDesign
{
    /// <summary>
    /// This class contains all methods that exploit car geometry engine
    /// </summary>
    public class CarLauncher
    {
        

        #region Dataset creation
        /// <summary>
        /// Generate cars of special type and their textual descriptions
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allCars"></param>
        /// <param name="textsPerSample"></param>
        /// <returns></returns>
        private static (List<string> prompts, List<string> codes) GenerateForType(CarType type, List<Car> allCars, float textsPerSample=-1)
        {
            List<Car> cars = type.GetCars();

            List<string> numParameters = new List<string>()
            {
                "Length", "Width", "Height", "WheelRadius"
            };
            List<string> numParameterNames = new List<string>()
            {
                "length", "width", "height", "wheel radius"
            };
            Dictionary<string, List<float>> classNumParameterBoundaries = new Dictionary<string, List<float>>();
            Dictionary<string, List<float>> totalNumParameterBoundaries = new Dictionary<string, List<float>>();
            foreach (string parameter in numParameters)
            {
                classNumParameterBoundaries[parameter] = new List<float>() { float.MaxValue, float.MinValue };
                totalNumParameterBoundaries[parameter] = new List<float>() { float.MaxValue, float.MinValue };
            }
            foreach (string parameter in numParameters)
            {
                FieldInfo field = typeof(Car).GetField(parameter);
                foreach (Car car in cars)
                {
                    float value = (float)field.GetValue(car);
                    if (value < classNumParameterBoundaries[parameter][0])
                    {
                        classNumParameterBoundaries[parameter][0] = value;
                    }
                    if (value > classNumParameterBoundaries[parameter][1])
                    {
                        classNumParameterBoundaries[parameter][1] = value;
                    }
                }
                foreach (Car car in allCars)
                {
                    float value = (float)field.GetValue(car);
                    if (value < totalNumParameterBoundaries[parameter][0])
                    {
                        totalNumParameterBoundaries[parameter][0] = value;
                    }
                    if (value > totalNumParameterBoundaries[parameter][1])
                    {
                        totalNumParameterBoundaries[parameter][1] = value;
                    }
                }
            }

            string basePrompt = "Generate a car with the following characteristics: ";
            List<string> prompts = new List<string>();
            List<string> codes = new List<string>();
            string carType = "";
            if (type is Sedan)
            {
                carType = "sedan";
            }
            else if (type is Hatchback)
            {
                carType = "hatchback";
            }
            else if (type is StationWagon)
            {
                carType = "station wagon";
            }
            else if (type is Coupe)
            {
                carType = "coupe";
            }
            else if (type is Limousine)
            {
                carType = "limousine";
            }
            else if (type is SUV)
            {
                carType = "SUV";
            }
            else if (type is Pickup)
            {
                carType = "pickup";
            }
            else if (type is Minivan)
            {
                carType = "minivan";
            }
            else if (type is Van)
            {
                carType = "van";
            }
            else if (type is Truck)
            {
                carType = "truck";
            }
            List<string> numWords = new List<string>() { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "tenth"};
            for (int i = 0; i < cars.Count; i++)
            {
                Car car = cars[i];
                string code = car.ToString();
                List<string> localTexts = new List<string>();

                string prompt = basePrompt;
                List<string> textVariants = new List<string>();
                List<string> carTypeTextVariants = new List<string>();
                carTypeTextVariants.Add(prompt);
                carTypeTextVariants.Add(prompt + $"body type: {carType};");
                List<string> typeParameterDescriptions = new List<string>();
                string description = $"{car.WheelRelativeBiasesAlongLength.Count} wheelsets";
                typeParameterDescriptions.Add(description);
                if (type is Sedan || type is Coupe)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    if (car.BodySegmentsSpans[2][0] == 0.8f)
                    {
                        typeParameterDescriptions.Add("normal size boot");
                    }
                    else if (car.BodySegmentsSpans[2][0] == 0.9f)
                    {
                        typeParameterDescriptions.Add("small boot");
                    }
                }
                else if (type is Hatchback || type is SUV)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    typeParameterDescriptions.Add("normal size boot");
                    typeParameterDescriptions.Add("door at the rear");
                }
                else if (type is StationWagon)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    typeParameterDescriptions.Add("large body");
                    typeParameterDescriptions.Add("large boot");
                    typeParameterDescriptions.Add("door at the rear");
                }
                else if (type is Limousine)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    if (car.BodySegmentsSpans[2][0] == 0.8f)
                    {
                        typeParameterDescriptions.Add("normal size boot");
                    }
                    else if (car.BodySegmentsSpans[2][0] == 0.9f)
                    {
                        typeParameterDescriptions.Add("small boot");
                    }
                    typeParameterDescriptions.Add("very long body");
                }
                else if (type is Pickup)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    typeParameterDescriptions.Add("large boot");
                }
                else if (type is Minivan)
                {
                    typeParameterDescriptions.Add("small bonnet");
                    typeParameterDescriptions.Add("large body");
                    typeParameterDescriptions.Add("no boot");
                }
                else if (type is Van)
                {
                    typeParameterDescriptions.Add("normal size bonnet");
                    if (car.BodySegmentsTopSurfaces[2].MaxHeight > car.BodySegmentsTopSurfaces[1].MaxHeight)
                    {
                        typeParameterDescriptions.Add("1 cargo container");
                    }
                    else
                    {
                        typeParameterDescriptions.Add("1 flatbed");
                    }
                }
                else if (type is Truck)
                {
                    if (car.BodySegmentsTopSurfaces[0].MaxHeight > car.Height*0.6f)
                    {
                        typeParameterDescriptions.Add("no bonnet");
                    }
                    else if (car.BodySegmentsTopSurfaces[0].MaxHeight > car.Height * 0.6f)
                    {
                        typeParameterDescriptions.Add("large bonnet");
                    }
                    Line fairing = car.BodySegmentsTopSurfaces.Where(x => x is TotalRounded && x.MaxHeight == car.Height).FirstOrDefault();
                    if (fairing != null)
                    {
                        typeParameterDescriptions.Add("roof fairing with sleeper cab");
                    }
                    int cargoCount = 0;
                    for (int j = 0; j < car.BodySegmentsSpans.Count; j++)
                    {
                        List<float> span = car.BodySegmentsSpans[j];
                        if (span[1] - span[0] > 0.17)
                        {
                            cargoCount = car.BodySegmentsTopSurfaces.Count - i;
                            break;
                        }
                    }
                    if (cargoCount == 1)
                    {
                        typeParameterDescriptions.Add("1 cargo container");
                    }
                    else if (cargoCount > 1)
                    {
                        typeParameterDescriptions.Add($"{cargoCount} cargo containers");
                    }
                }
                List<List<string>> descriptionSamples = SampleWithDrop(typeParameterDescriptions);
                foreach (List<string> sample in descriptionSamples)
                {
                    carTypeTextVariants.Add(prompt + String.Join(";", sample));
                }
                foreach (string carTypePrompt in carTypeTextVariants)
                {
                    textVariants.Add(carTypePrompt);
                    List<int[]> masks = new List<int[]>();
                    for (int j = 0; j < Math.Pow(2, numParameters.Count); j++)
                    {
                        int[] mask = new int[numParameters.Count];
                        int div = j;
                        int ind = numParameters.Count - 1;
                        while (div > 0)
                        {
                            mask[ind] = div % 2;
                            ind--;
                            div = (int)(div / 2);
                        }
                        for (int k = 0; k < ind + 1; k++)
                        {
                            mask[k] = 0;
                        }
                        masks.Add(mask);
                    }
                    foreach (int[] mask in masks)
                    {
                        List<string> numParametersDescriptions = new List<string>();
                        for (int k = 0; k < numParameters.Count; k++)
                        {
                            FieldInfo info = typeof(Car).GetField(numParameters[k]);
                            float value = (float)info.GetValue(car);
                            if (mask[k] == 0)
                            {
                                numParametersDescriptions.Add($"{numParameterNames[k]}: {value} cm");
                            }
                            else
                            {
                                string desc = "";
                                float min = 0;
                                float max = 0;
                                if (carTypePrompt.Contains("body type:"))
                                {
                                    min = classNumParameterBoundaries[info.Name][0];
                                    max = classNumParameterBoundaries[info.Name][1];
                                }
                                else
                                {
                                    min = totalNumParameterBoundaries[info.Name][0];
                                    max = totalNumParameterBoundaries[info.Name][1];
                                }
                                float leftMedium = min + (max - min) / 3;
                                float rightMedium = min + (max - min) * 2 / 3;
                                if (value <= leftMedium)
                                {
                                    desc = "small";
                                }
                                else if (value >= rightMedium)
                                {
                                    desc = "big";
                                }
                                else
                                {
                                    desc = "medium";
                                }
                                numParametersDescriptions.Add($"{numParameterNames[k]} is {desc}");
                            }
                        }


                        List<List<string>> numParametersDescriptionSamples = SampleWithDrop(numParametersDescriptions);
                        foreach (List<string> sample in numParametersDescriptionSamples)
                        {
                            textVariants.Add($"{carTypePrompt}; {String.Join("; ", sample)}");
                        }
                    }
                }
                List<string> finalPrompts = new List<string>();
                foreach (string text in textVariants)
                {
                    if (!finalPrompts.Contains(text))
                    {
                        finalPrompts.Add(text);
                    }
                }
                if (textsPerSample == -1)
                {
                    foreach (string text in finalPrompts)
                    {
                        prompts.Add(text);
                        codes.Add(code);
                    }
                }
                else
                {
                    Random random = new Random();
                    List<int> inds = new List<int>();
                    for (int j = 0; j < textsPerSample; j++)
                    {
                        inds.Add(random.Next(0, finalPrompts.Count - 1));
                    }
                    foreach (int ind in inds)
                    {
                        prompts.Add(finalPrompts[ind]);
                        codes.Add(code);
                    }
                }
                
            }
            return (prompts, codes);
        }

        /// <summary>
        /// Generates all possible combinations from list elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="samples"></param>
        /// <returns></returns>
        private static List<List<T>> SampleWithDrop<T>(List<T> samples)
        {
            List<List<T>> results = new List<List<T>>() { samples};
            if (samples.Count > 1)
            {
                for (int i = 0; i < samples.Count; i++)
                {
                    List<T> toDrop = new List<T>(samples);
                    toDrop.RemoveAt(i);
                    List<List<T>> droppedResults = SampleWithDrop(toDrop);
                    results = results.Concat(droppedResults).ToList();
                }
            }
            return results;
        }

        /// <summary>
        /// Generate dataset of text2code pairs for all car types
        /// </summary>
        public static void GenerateDataset()
        {
            List<CarType> carTypes = new List<CarType>()
            {
                new Sedan(), new Hatchback(), new StationWagon(), new Coupe(), new Limousine(), new SUV(), new Pickup(), new Minivan()
            };
            Van van = new Van();
            van.NumValuesCount["Height"] = 2;
            carTypes.Add(van);
            Truck truck = new Truck();
            truck.NumValuesCount["Height"] = 2;
            carTypes.Add(truck);
            List<Car> allCars = new List<Car>();
            foreach (CarType type in carTypes)
            {
                allCars = allCars.Concat(type.GetCars()).ToList();
            }
            List<string> prompts = new List<string>();
            List<string> codes = new List<string>();
            foreach (CarType type in carTypes)
            {
                (List<string> prompts, List<string> codes) result = GenerateForType(type, allCars, 2);
                prompts = prompts.Concat(result.prompts).ToList();
                codes = codes.Concat(result.codes).ToList();
            }
            for (int i = 0; i < codes.Count; i++)
            {
                string newPath = $"C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset\\OutputCode\\Code_{i}.cs";
                using (StreamWriter writer = new StreamWriter(newPath))
                {
                    writer.WriteLine(codes[i]);
                }
            }
            string textsPath = $"C:\\Все_файлы\\Научная_деятельность\\GenerativeDesign\\CarsDataset\\InputText.txt";
            using (StreamWriter writer = new StreamWriter(textsPath))
            {
                writer.Write(String.Join("\n", prompts));
            }
        }

        /// <summary>
        /// Generate and save codes for every car type
        /// </summary>
        public static void GenerateTypes()
        {
            string folder = "C:\\Все_файлы\\Работа\\AIRI\\computational_intelligence\\leap_71\\code_by_types";

            List<CarType> carTypes = new List<CarType>()
            {
                new Sedan(), new Hatchback(), new StationWagon(), new Coupe(), new Limousine(), new SUV(), new Pickup(), new Minivan()
            };
            Van van = new Van();
            van.NumValuesCount["Height"] = 2;
            carTypes.Add(van);
            Truck truck = new Truck();
            truck.NumValuesCount["Height"] = 2;
            carTypes.Add(truck);
            List<string> carTypesNames = new List<string>()
            {
                "Sedan", "Hatchback", "StationWagon", "Coupe", "Limousine", "SUV", "Pickup", "Minivan", "Van", "Truck"
            };
            for (int i = 0; i < carTypes.Count; i++)
            {
                CarType type = carTypes[i];
                string name = carTypesNames[i];
                (List<string> prompts, List<string> codes) cars = GenerateForType(type, new List<Car>(), 1);
                Directory.CreateDirectory(Path.Combine(folder, name));
                for (int j = 0; j < cars.codes.Count; j++)
                {
                    string newPath = Path.Combine(folder, name, $"Code_{j}.cs");
                    using (StreamWriter writer = new StreamWriter(newPath))
                    {
                        writer.WriteLine(cars.codes[j]);
                    }
                }
            }
        }
        #endregion

        #region Demonstration of capabilities
        /// <summary>
        /// Toy method to show car sample
        /// </summary>
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

        /// <summary>
        /// Compile and display random code samples from dataset downloaded with "load_dataset.py" script
        /// </summary>
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

        /// <summary>
        /// Compile and display code sample from text file
        /// </summary>
        public static void CompileAndDisplay()
        {
            string path = "path to sample.txt";
            string code = "";
            using (StreamReader reader = new StreamReader(path))
            {
                code = reader.ReadToEnd();
            }
            RoslynCompiler compiler = new RoslynCompiler("GenerativeDesign.Cars.Car", code, new[] { typeof(Console), typeof(CarBase), typeof(List<>) });
            Type carType = compiler.Compile();
            object carInstance = Activator.CreateInstance(carType);

            Vector3 position = new Vector3(0, 0, 0);
            carType.GetField("BasePoint").SetValue(carInstance, position);
            object objVoxels = carType.GetMethod("voxConstruct").Invoke(carInstance, new object[] { });
            Voxels voxCar = (Voxels)objVoxels;
            Sh.PreviewVoxels(voxCar, Cp.clrRock);
        }
        #endregion


    }
}
