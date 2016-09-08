using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxRadio_2_Broadcaster_console
{
	static class Ban
	{
		private const string BAN_FILE_DIR = "BAN.cfg";
		public static List<string> BanData = new List<string>( );

		public static bool Load( )
		{
			if ( System.IO.File.Exists( BAN_FILE_DIR ) )
			{
				foreach ( string i in System.IO.File.ReadAllLines( BAN_FILE_DIR ) )
				{
					if ( string.IsNullOrEmpty( i ) ) continue;
					if ( i.StartsWith( "[" ) && i.EndsWith( "]" ) ) continue;

					string[ ] c = i.Split( new char[ ] { '\n' }, StringSplitOptions.RemoveEmptyEntries );

					if ( c.Length == 1 )
					{
						BanData.Add( c[ 0 ] );
						Console.WriteLine( "BAN Register : {0}", c[ 0 ] );
					}
				}

				return true;
			}
			else
				return false;
		}

		public static bool IsBanned( string IP )
		{
			if ( IP.IndexOf( ':' ) > 0 )
			{
				IP = IP.Substring( 0, IP.IndexOf( ':' ) );
			}

			foreach ( string i in BanData )
			{
				if ( i == IP )
					return true;
			}

			return false;
		}
	}
}
