using System;
using System.Collections;
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

			bool BAN_LOAD_SUCCESS = Ban.Load( );

			if ( BAN_LOAD_SUCCESS )
				Console.WriteLine( "Ban data Loaded. [" + Ban.BanData.Count + " 's Ban data]" );
			else
				Console.WriteLine( "Ban data Load failed." );

			bool CONFIG_LOAD_SUCCESS = Config.Load( );

			if ( CONFIG_LOAD_SUCCESS )
				Console.WriteLine( "Config data Loaded." );
			else
				Console.WriteLine( "Config data Load failed." );
			
			if ( BAN_LOAD_SUCCESS && CONFIG_LOAD_SUCCESS )
				Server.Start( );
		}
	}
}