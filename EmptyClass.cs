/******************************************************
 * 
 * Author: Gemal Horne-Morgan
 * 
 * A class mostly used to explore the various C# frameworks.
 * A class to fetch definitions from dictionary.com. Definitions
 * are cached into a ditionary to reduce overall network traffic.
 * 
 * The server is meant to be accessed via telnet(terminal)!
 * 
 * 
 * *****************************************************/





using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Application
{
	public class EmptyClass
	{
		//Logs definition requests
		public static void log(string msg) {
			var writer = new StreamWriter ("log.txt", true);
			writer.WriteLine ("Message logged: "+msg);
			writer.Close ();
		}
		//Logs connection requests
		public static void log(TcpClient client) {
			var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
			var writer = new StreamWriter ("log.txt", true);
			writer.WriteLine ("Connection from IP: "+ip);
			writer.Close ();
		}
		//Checks log to check total requests of a definition
		public static int count(String definition) {
			var reader = File.OpenText("log.txt");
			string line;
			var count = 0;
			while ((line = reader.ReadLine ()) != null) {
				if (line.Contains (definition))
					count++;
			}
			reader.Close ();
			return count;
		}
			
		//Retrieves definition from dictionary.com
		public static string getDefinition(string definition) {
			var request = WebRequest.Create ("http://www.dictionary.com/browse/"+definition);
			var response = request.GetResponse (); 
			StreamReader reader = new StreamReader (response.GetResponseStream());
			//string result = reader.ReadToEnd ();
			string line;
			var identifier = "name=\"description\" content=\"";
			while ((line = reader.ReadLine()) != null) 
			{
				//This is the correct portion of the response, although we must
				//extract the definition out of the HTML document.
				if (line.Contains (identifier)) {
					var start = line.IndexOf (identifier);
					line = line.Substring (line.IndexOf(identifier)+identifier.Length, line.Length - start - identifier.Length);
					int end = line.IndexOf ("See more.");
					reader.Close ();
					response.Close ();
					return line.Substring(0,end); 
				}
			}
			reader.Close ();
			response.Close ();
			return "Sorry, no result for definition: "+definition;
		}
		public static void Main()
		{
			//Our collection to cache definitions, hashing via Dictionary is possibly an efficient solution
			var dict = new Dictionary<string, string>();
			int port = 10101;
			var response = "";
			Console.WriteLine ("Now accepting connections on port "+port);

			var listener = new TcpListener (IPAddress.Any, port);
			listener.Start ();
			while (true) {
				
				//First, configure the socket for communication
				var client = listener.AcceptTcpClient ();
				log (client);
				var stream = client.GetStream ();
				var writer = new StreamWriter (stream);
				var reader = new StreamReader (stream);
				writer.AutoFlush = true;
				writer.WriteLine ("Enter any word, and I will fetch the definition for you.");

				//Handling a response from the client
				string input = reader.ReadLine ();//requested definition
				log (input);
				writer.WriteLine ("Requested definition: "+input);
				writer.WriteLine (input+" has been previously requested a total of "+count(input)+" times.");

				//Check our cache for the definition
				if (dict.ContainsKey (input) == true) {
					response = dict[input];
				} else {
					response = getDefinition (input);
					dict.Add (input, response); //add definition to local cache
				}
				writer.WriteLine (response);

				stream.Close();
				client.Close();
			}
		}
		public EmptyClass ()
		{
		}
	}
}

