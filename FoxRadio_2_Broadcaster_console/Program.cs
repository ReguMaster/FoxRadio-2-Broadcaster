using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FoxRadio_2_Broadcaster_console
{
	class Program
	{
		static void Main( string[ ] args )
		{
			Console.Title = "Fox Radio 2 Broadcaster Server CUI - DeveloFOX Studio";
			Server.Start( );
		}
	}
}