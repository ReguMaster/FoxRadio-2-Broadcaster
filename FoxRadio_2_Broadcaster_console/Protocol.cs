using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxRadio_2_Broadcaster_console
{
	static class Protocol
	{
		public enum ServerProtocolMessage // 서버로 오는 프로토콜 메세지
		{
			Null,
			ClientNickNameSet
		};

		public enum ClientProtocolMessage // 클라이언트로 보내는 프로토콜 메세지
		{
			Null,
			NickNameInitialized,
			MusicPlay,
			MusicSetLocation
		};

		public static bool IsProtocol<T>( string ProtocolString, T Protocol )
		{
			int index = ProtocolString.IndexOf( "#" );

			if ( index > 0 )
				return ProtocolString.Substring( 0, index ) == ( Convert.ToInt32( Protocol ) ).ToString( );
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

				return ( T ) Enum.ToObject( typeof( T ), 0 );
			}
			else
			{
				return ( T ) Enum.ToObject( typeof( T ), 0 );
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

		public static string MakeProtocol<T>( T Protocol, string Data )
		{
			return ( Convert.ToInt32( Protocol ) ) + "#" + Data;
		}
	}
}
