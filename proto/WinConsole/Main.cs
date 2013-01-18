using System;
using System.IO;
using System.Diagnostics;

namespace Testing
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ConsoleApp
	{

		public static void TryItOut()
		{
			Debug.Assert(1==0);
		}

		public static void Main()
		{
			WinConsole.Initialize();

			// To make Debug or Trace output to the console, do the following.
			//Debug.Listeners.Remove("default");
			//Debug.Listeners.Add(new TextWriterTraceListener(new ConsoleWriter(...)));

			Console.SetError(new ConsoleWriter(Console.Error, ConsoleColor.Red | ConsoleColor.Intensified | ConsoleColor.WhiteBG, ConsoleFlashMode.FlashUntilResponse, true));
			WinConsole.Color = ConsoleColor.Blue | ConsoleColor.Intensified | ConsoleColor.BlueBG;
			WinConsole.Flash(true);
			Console.Error.WriteLine("Console.Error.WriteLine()");
			Console.WriteLine("Console.WriteLine()");
			Console.ReadLine();
		}
	}
}
