using GenerativeDesign;
try
{
    PicoGK.Library.Go(0.5f, Launcher.CompileAndDisplay);
}

catch (Exception e)
{
	// Apparently something went wrong, output here
	Console.WriteLine(e);
}