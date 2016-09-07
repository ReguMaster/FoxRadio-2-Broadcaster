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
		public bool FailedVersion;

		public ClientData( string Nick, string IP, bool FailedVersion )
		{
			this.Nick = Nick;
			this.IP = IP;
			this.FailedVersion = FailedVersion;
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

		public void Initialize( string Nick, string IP, bool FailedVersion )
		{
			ClientData = new ClientData( Nick, IP, FailedVersion );

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
									case ServerProtocolMessage.VersionCheck: // 버전 필터링
										string Version = Protocol.GetProtocolData( Message );

										ClientData = new ClientData( this.ClientData.Value.Nick, this.ClientData.Value.IP, false );
										break;
									case ServerProtocolMessage.Initialize:
										Console.WriteLine( "프로토콜 받음 : " + MessageProtocol + " [ " + ClientData.Value.IP + " ][ " + ClientData.Value.Nick + " ]" );

										string NewNickName = Protocol.GetProtocolData( Message );

										if ( NewNickName != "PROTOCOL_DATA_ERROR" && NewNickName.Length >= 3 && NewNickName.Length <= 18 )
										{
											if ( Ban.IsBanned( ClientData.Value.IP ) )
											{
												SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.CantConnect, "B" ) );
												break;
											}
											if ( ClientData.Value.FailedVersion )
											{
												SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.CantConnect, "최신 버전의 Fox Radio 2 로 업데이트 하세요." ) );
												break;
											}

											ClientData = new ClientData( NewNickName, this.ClientData.Value.IP, false );

											SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.NickNameInitialized, "S" ), ( ) =>
											{
												string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( Music.CurrentMusicMS.ToArray( ) ) );

												SendData( data, ( ) =>
												{
													Console.WriteLine( "프로토콜 전송 : SEND MUSIC [" + ClientData.Value.IP + "] [" + ClientData.Value.Nick + "]" );

													SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, Music.MakeSongInformationForProtocolSend( Music.GetCurrentSong( ) ) ), ( ) =>
													{
														Console.WriteLine( "프로토콜 전송 : SEND MUSIC INFORMATION [ " + ClientData.Value.IP + " ][ " + ClientData.Value.Nick + " ]" );
													} );
												} );
											} );
										}
										else
											SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.CantConnect, "N" ) );
										break;
								}
							}
						}
						catch ( IOException ) { }
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
