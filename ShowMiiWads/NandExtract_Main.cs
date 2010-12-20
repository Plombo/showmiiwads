using System;
using ShowMiiWads;

namespace NandExtractMain
{
	// Extracts a BootMii NAND dump.
	public class BootMiiNandExtractor
	{
		public static int Main(string[] args)
		{
			string appVersion = "v0.1";
			
			Console.WriteLine("NandExtract " + appVersion);
			Console.WriteLine("Copyright (c) 2010 Bryan Cain (Plombo)");
			Console.WriteLine("NAND extraction code from ShowMiiWads is copyright (c) 2009 Ben Wilson");
			Console.WriteLine();
			
			if(args.Length != 2)
			{
				Console.WriteLine("Usage: nandextract nand.bin destdir");
				Console.WriteLine("\tnand.bin: path to a BootMii NAND dump");
				Console.WriteLine("\tdestdir: location to extract the NAND contents");
				return 1;
			}
			
			NandExtract.extractNAND(args[0], args[1]);
			
			Console.WriteLine();
			Console.WriteLine("Done!");
			return 0;
		}
	}
}

