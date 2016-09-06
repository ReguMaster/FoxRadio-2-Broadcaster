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
								Console.WriteLine( "오류 프로토콜" );
							}
							else
							{
								switch ( MessageProtocol )
								{
									case ServerProtocolMessage.ClientNickNameSet:
										Console.WriteLine( "프로토콜 받음 : " + MessageProtocol + " [ " + ClientData.Value.IP + " ][ " + ClientData.Value.Nick + " ]" );

										string NewNickName = Protocol.GetProtocolData( Message );

										if ( NewNickName != "PROTOCOL_DATA_ERROR" )
										{
											ClientData = new ClientData( NewNickName, this.ClientData.Value.IP );

											string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( Music.CurrentMusicMS.ToArray( ) ) );

											SendData( data, ( ) =>
											{
												Console.WriteLine( "프로토콜 전송 : SEND MUSIC [" + ClientData.Value.IP + "] [" + ClientData.Value.Nick + "]" );

												SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicSetLocation, Music.CurrentSongTickLocation.ToString( ) ), ( ) =>
												{
													Console.WriteLine( "프로토콜 전송 : SEND MUSIC LOCATION [ " + ClientData.Value.IP + " ][ " + ClientData.Value.Nick + " ]" );
												} );
											} );
										}
										break;
								}
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
				Server.ClientDisconnect( this );
			}
		}
	}
}
