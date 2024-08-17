using GenerativeDesign;
try
{
    PicoGK.Library.Go(0.5f, Launcher.SampleCars);
}

catch (Exception e)
{
	// Apparently something went wrong, output here
	Console.WriteLine(e);
}