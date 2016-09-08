using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static FoxRadio_2_Broadcaster_console.Protocol;

namespace FoxRadio_2_Broadcaster_console
{
	public static class Server
	{
		public static List<Client> Clients = new List<Client>( );
		public static Encoding MessageEncode = Encoding.GetEncoding( "ks_c_5601-1987" );

		public static void Start( )
		{
			TcpListener SERVER = new TcpListener( IPAddress.Parse( Config.GetData<string>( "IP" ) ), int.Parse( Config.GetData<string>( "PORT" ) ) );
			SERVER.Start( );

			Music.LoadSongListFromFile( );
			Music.InitializeCycle( );

			Console.WriteLine( "서버 초기화 됨!" );

			Thread CommandListen = new Thread( ( ) =>
			{
				while ( true )
				{
					string Command = Console.ReadLine( );

					if ( Command == "exit" )
					{
						System.Diagnostics.Process.GetCurrentProcess( ).Kill( );
					}
					else if ( Command.StartsWith( "setsong" ) )
					{
						string[ ] args = Command.Split( new char[ ] { ' ' }, StringSplitOptions.RemoveEmptyEntries );

						try
						{
							Music.SetCycle( int.Parse( args[ 1 ].Trim( ) ) );
						}
						catch ( Exception ) { }
					}
					else if ( Command.StartsWith( "notify" ) )
					{
						string[ ] args = Command.Split( new char[ ] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
						
						try
						{
							foreach ( Client Client in Server.Clients )
							{
								Client.SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.ChatReceive, Command.Substring( 7 ).Trim( ) ) );
							}
						}
						catch ( Exception ) { }
					}
					else if ( Command.StartsWith( "printsong" ) )
					{
						int i = 0;
						foreach ( SongList Song in Music.Songs )
						{
							Console.WriteLine( "Next Cycle : " + Song.SongAuthor + " - " + Song.SongName + " [" + i + "]" );

							i++;
						}
					}
					else if ( Command.StartsWith( "reloadsongs" ) )
					{
						Music.LoadSongListFromFile( );
					}
					else if ( Command.StartsWith( "printclients" ) )
					{
						int i = 0;
						foreach ( Client Client in Server.Clients )
						{
							Console.WriteLine( "CLIENT[" + i + "] " + Client.ClientData.Value.Nick + " " + Client.ClientData.Value.IP );

							i++;
						}
					}
				}
			} );
			CommandListen.IsBackground = true;
			CommandListen.Start( );

			while ( true )
			{
				Thread.Sleep( 100 );

				TcpClient clientsocket = SERVER.AcceptTcpClient( );

				ClientConnect( clientsocket );
			}
		}

		public static void ClientConnect( TcpClient ClientSocket )
		{
			Client CLIENT = new Client( )
			{
				TCPClient = ClientSocket,
				TCPSocket = ClientSocket.Client
			};
			Thread Cycle = new Thread( ( ) => CLIENT.Cycle( ) );

			CLIENT.CycleThread = Cycle;
			CLIENT.Initialize( "NULL_NICK_NAME", ClientSocket.Client.RemoteEndPoint.ToString( ), false );

			Cycle.IsBackground = true;
			Cycle.Start( );

			Clients.Add( CLIENT );

			Console.WriteLine( "클라이언트가 접속 ... [" + CLIENT.ClientData.Value.IP + "] [" + CLIENT.ClientData.Value.Nick + "]" );
			Console.WriteLine( ( Clients.Count - 1 ) + " -> " + Clients.Count );

			for ( int i = 0; i < Server.Clients.Count; i++ )
			{
				Server.Clients[ i ].SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.ClientCountSend, Clients.Count.ToString( ) ) );
			}
		}

		public static void ClientDisconnect( Client Client )
		{
			Console.WriteLine( "클라이언트가 접속 종료 ... [" + Client.ClientData.Value.IP + "] [" + Client.ClientData.Value.Nick + "]" );
			Clients.Remove( Client );
			Client.CycleThread.Interrupt( );

			Console.WriteLine( ( Clients.Count + 1 ) + " -> " + Clients.Count );

			for ( int i = 0; i < Server.Clients.Count; i++ )
			{
				Server.Clients[ i ].SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.ClientCountSend, Clients.Count.ToString( ) ) );
				Server.Clients[ i ].SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.ChatReceive, Client.ClientData.Value.Nick + " 님이 나가셨습니다." ) );
			}
		}
	}
}
