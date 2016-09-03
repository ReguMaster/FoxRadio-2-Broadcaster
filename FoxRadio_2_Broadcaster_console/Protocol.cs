using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxRadio_2_Broadcaster_console
{
	static class Protocol
	{
		public enum ServerProtocolMessage
		{
			Null,
			ClientNickNameSet
		};

		public enum ClientProtocolMessage
		{
			Null,
			NickNameInitialized
		};

		public static bool IsProtocol<T>( string ProtocolString, T Protocol )
		{
			int index = ProtocolString.IndexOf( "#" );

			if ( index > 0 )
				return ProtocolString.Substring( 0, index ) == ( Convert.ToInt32(Protocol) ).ToString( );
			else
				return false;
		}

		public static T GetProtocol<T>( string ProtocolString )
		{
			int index = ProtocolString.IndexOf( "#" );

			if ( index > 0 )
			{
				string Protocol = ProtocolString.Substring( 0, index );

				foreach ( T i in Enum.GetValues( typeof( T ) ) )
				{
					if ( Protocol == ( Convert.ToInt32( i ) ).ToString( ) )
						return i;
				}

				Enum value = ( Enum ) Enum.ToObject( typeof( T ), 0 );

				return ( T ) ( object ) value;
			}
			else
			{
				Enum value = ( Enum ) Enum.ToObject( typeof( T ), 0 );

				return ( T ) ( object ) value;
			}
		}

		public static string GetProtocolData( string ProtocolString )
		{
			int index = ProtocolString.IndexOf( "#" );

			if ( index > 0 )
			{
				return ProtocolString.Substring( index + 1, ProtocolString.Length - ( index + 1 ) );
			}
			else
				return "PROTOCOL_DATA_ERROR";
		}

		public static byte[ ] MakeProtocol<T>( T Protocol, string Data )
		{
			return Encoding.UTF8.GetBytes( ( Convert.ToInt32( Protocol ) ) + Data );
		}
	}
}
