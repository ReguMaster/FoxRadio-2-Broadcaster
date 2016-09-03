using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using static FoxRadio_2_Broadcaster_console.Protocol;

namespace FoxRadio_2_Broadcaster_console
{
	public struct ClientData
	{
		public string Nick;
		public string IP;

		public ClientData( string Nick, string IP )
		{
			this.Nick = Nick;
			this.IP = IP;
		}
	}

	public class Client
	{
		public TcpClient TCPClient = null;
		public Socket TCPSocket = null;
		private bool Initialized = false;
		public ClientData? ClientData = null;
		public Thread CycleThread = null;

		public void Initialize( string Nick, string IP )
		{
			ClientData = new ClientData( Nick, IP );

			Console.WriteLine( "Client Initialized!" );

			this.Initialized = true;
		}

		public static bool IsConnected( Socket Socket )
		{
			try
			{
				return !( Socket.Poll( 1, SelectMode.SelectRead ) && Socket.Available == 0 );
			}
			catch ( SocketException ) { return false; }
		}

		public void Cycle( )
		{
			if ( !this.Initialized )
			{
				Console.WriteLine( "Client not Initialized!" );
			}
			else
			{
				NetworkStream stream = TCPClient.GetStream( );
				Encoding encode = Encoding.GetEncoding( "ks_c_5601-1987" );
				

				while ( true )
				{
					Thread.Sleep( 100 );

					if ( IsConnected( TCPSocket ) )
					{
						// DATA
						try
						{
							StreamReader reader = new StreamReader( stream, encode );
							string str = reader.ReadLine( );

							ServerProtocolMessage MessageProtocol = Protocol.GetProtocol< ServerProtocolMessage>( str );

							if ( MessageProtocol == ServerProtocolMessage.Null )
							{
								Console.WriteLine( "NULL" );
							}
							else
							{
								switch ( MessageProtocol )
								{
									case ServerProtocolMessage.ClientNickNameSet:
										string NewNickName = Protocol.GetProtocolData( str );
										Console.WriteLine( NewNickName );
										ClientData = new ClientData( NewNickName, this.ClientData.Value.IP );
										break;
								}
								Console.WriteLine( "PROTOCOL : " + MessageProtocol );
							}
							
						}
						catch ( IOException )
						{
							
						}
					}
					else
					{
						Disconnect( );
						break;
					}
				}
			}
		}

		public void Send( )
		{
			NetworkStream stream = TCPClient.GetStream( );
			Encoding encode = Encoding.GetEncoding( "ks_c_5601-1987" );
			StreamWriter writer = new StreamWriter( stream, encode );

			writer.WriteLine( "1#SUCCESS" );
			writer.Flush( );

			Console.WriteLine( "WRITE!" );

			
			//MakeProtocol( MessageProtocol Protocol, string Data )
		}

		public void Disconnect( bool PrintConsoleLog = true )
		{
			if ( PrintConsoleLog )
			{
				Console.WriteLine( "Disconnected!" );
				Server.ClientDisconnect( this );
			}
		}
	}
}
