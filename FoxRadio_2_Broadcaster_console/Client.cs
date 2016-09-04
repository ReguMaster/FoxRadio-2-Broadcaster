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
		private StreamWriter Writer = null;
		private StreamReader Reader = null;

		public void Initialize( string Nick, string IP )
		{
			ClientData = new ClientData( Nick, IP );

			NetworkStream Stream = TCPClient.GetStream( );
			Writer = new StreamWriter( Stream, Server.MessageEncode );
			Reader = new StreamReader( Stream, Server.MessageEncode );

			Console.WriteLine( "Client Initialized!" );

			Initialized = true;
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
				while ( true )
				{
					Thread.Sleep( 100 );

					if ( IsConnected( TCPSocket ) )
					{
						try
						{
							string Message = Reader.ReadLine( );

							ServerProtocolMessage MessageProtocol = Protocol.GetProtocol<ServerProtocolMessage>( Message );

							if ( MessageProtocol == ServerProtocolMessage.Null )
							{
								Console.WriteLine( "NULL" );
							}
							else
							{
								switch ( MessageProtocol )
								{
									case ServerProtocolMessage.ClientNickNameSet:
										string NewNickName = Protocol.GetProtocolData( Message );

										if ( NewNickName != "PROTOCOL_DATA_ERROR" )
										{
											Console.WriteLine( NewNickName );
											ClientData = new ClientData( NewNickName, this.ClientData.Value.IP );

											string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( Music.CurrentMusicMS.ToArray( ) ) );

											SendData( data, ( ) =>
											{
												Console.WriteLine( ClientData.Value.Nick + " : SEND MUSIC" );

												SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicSetLocation, Music.CurrentSongTickLocation.ToString( ) ), ( ) =>
												{
													Console.WriteLine( ClientData.Value.Nick + " : SEND MUSIC LOCATION" );
												} );
											} );
										}
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

		public void SendData( string ProtocolMessage, Action CallBack = null )
		{
			Writer.WriteLine( ProtocolMessage );
			Writer.Flush( );
			CallBack?.Invoke( );
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
