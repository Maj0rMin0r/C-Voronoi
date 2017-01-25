using System;
using Voronoi;


namespace Runner
{
    internal class Program
	{
	    private const int Width = 70;
	    private const int Height = 20;
	    private const int NumSites = 5;

	    private static void Main()
		{
            Console.Out.WriteLine("Started");
			var output = Fortunes.Run(Width, Height, NumSites);
            var lines = output.OutputLines(Width, Height);
            var origins = output.Sites;

            output.OutputConsole();

            output.OutputFile(Width, Height);
            Console.Out.WriteLine("File Created");


		    foreach (var site in origins)
		    {
		        var region = output.OutputRegion(site, lines);
                //TODO Process regions
		    }

            output.PrintRegions(Width, Height);

			Console.Out.WriteLine("Finished");
		}
	}
}
