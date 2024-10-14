# Generative design

In this repository you will see some cases of AI-based generative design. It might not be possible without contribution of powerful library for computational engineering [PicoGK](https://github.com/leap71/PicoGK) by [Leap71](https://leap71.com/).

# Getting started

## Installation

To install this project and start to use it you should do the following steps:

1. Install Visual Studio 2022 following [this tutorial](https://github.com/leap71/PicoGK/blob/main/Documentation/VisualStudio_FirstTime.md)
2. Clone this repository to some directory on your machine. Now it is how your project folder looks like
    
    ![Untitled](imgs/Untitled.png)
    
3. Install this packages using [Nuget](https://learn.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio):
    - Microsoft.CodeAnalysis (4.10.0)
    - Microsoft.CodeAnalysis.CSharp (4.10.0)
4. Download PicoGK installer from [here](https://github.com/leap71/PicoGK/releases) (.exe file) and run it. Here you must choose some folder:
    
    ![Untitled](imgs/Untitled%201.png)
    
5. Open the folder from step 4. You should see the same picture
    
    ![Untitled](imgs/Untitled%202.png)
    
6. Select all folders from PicoGK Example folder and copy them to your project folder. Then it should looks like this
    
    ![Untitled](imgs/Untitled%203.png)
    
7. Almost done! Now everything you need is installed, so you can try to test some functionalities. Open project with Visual Studio selecting the GenerativeDesign.sln file and run it. If pop-up window with something that seems like a strange car is appeared, than everything is alright.
    
    ![Untitled](imgs/Untitled%204.png)
    
    If picture on your screen is different (perhaps the window is dark), [this tutorial](https://github.com/leap71/PicoGK/blob/main/Documentation/README.md#troubleshooting) can help you with troubleshooting.
    

# Dataset

Dataset of car’s generation code is available on [HuggingFace](https://huggingface.co/datasets/evgmaslov/gen_cars). There are a few steps of how to download and use it:

1. Run script load_dataset.py using command line. Pass path of where to save dataset as the argument. Create dataset beyond the project directory in order to avoid problems with compilations. If you use bash write this from the first directory beyond project directory:
    
    ```bash
    python3 generative_design/ai_experiments/load_dataset.py CarsDataset
    ```
    
2. Open DesignKernel/CarLauncher.cs file and change dataset_path and n_samples local variables with you values of dataset path and number of samples to show
    
    ![Untitled](imgs/Untitled%205.png)
    
3. Replace code in Program.cs file with this code:
    
    ```csharp
    using GenerativeDesign;
    try
    {
        PicoGK.Library.Go(0.5f, CarLauncher.SampleFromDataset);
    }
    
    catch (Exception e)
    {
    	// Apparently something went wrong, output here
    	Console.WriteLine(e);
    }
    ```
    
4. Run solution. If everything is correct, you will see random car samples:
    
    ![Untitled](imgs/Untitled%206.png)

# Generate your car!

You can design your own car with our design kernel and transformer model trained on the dataset of cars. Model is available here: [evgmaslov/Mistral-Nemo-Instruct-2407-cars](https://huggingface.co/evgmaslov/Mistral-Nemo-Instruct-2407-cars).
Here are a few steps of how to generate a car:

1. Run script design_your_car.py using command line. Pass car description as the argument. If you use bash write this from the first directory beyond project directory:
    
    ```bash
    python3 generative_design/ai_experiments/design_your_car.py "Craft a car with the width of 165 cm and wheel radius of 30 cm."
    ```
    The file "sample.txt" will be saved in the directory from which the script was launched.

2. Open DesignKernel/CarLauncher.cs file and change path local variable with the path to "sample.txt" from the step 1.
    
    ![Untitled](imgs/Untitled%207.png)

3. Replace code in Program.cs file with this code:
    
    ```csharp
    using GenerativeDesign;
    try
    {
        PicoGK.Library.Go(0.5f, CarLauncher.CompileAndDisplay);
    }

    catch (Exception e)
    {
	    // Apparently something went wrong, output here
	    Console.WriteLine(e);
    }
    ```

4. Run solution. If everything is correct, you will see the car you asked to generate:
    
    ![Untitled](imgs/Untitled%208.png)