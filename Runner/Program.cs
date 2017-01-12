using System;
using Voronoi;


namespace Runner
{
    internal class Program
	{
	    private const int Width = 1920;
	    private const int Height = 1080;

	    private static void Main()
		{
            Console.Out.WriteLine("Started");
			var output = Fortunes.Run(Width, Height, 500);
            output.OutputConsole();
            output.OutputFile(Width, Height);
			Console.Out.WriteLine("Finished");
		}
	}
}
