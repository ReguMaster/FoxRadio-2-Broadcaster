using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoxRadio_2_Broadcaster_console
{
	public static class Server
	{
		public static List<Client> Clients = new List<Client>( );
		public static Encoding MessageEncode = Encoding.GetEncoding( "utf-8" );

		public static void Start( )
		{
			TcpListener SERVER = new TcpListener( IPAddress.Parse( ConfigDataTable.GetData<string>( "IP" ) ), 12345 );
			SERVER.Start( );

			Music.LoadSongListFromFile( );
			Music.InitializeCycle( );
			Music.NextCycle( );

			Console.WriteLine( "서버 초기화 됨!" );

			while ( true )
			{
				Thread.Sleep( 100 );

				TcpClient clientsocket = SERVER.AcceptTcpClient( );

				ClientConnect( clientsocket );
			}
		}

		public static void ClientConnect( TcpClient ClientSocket )
		{
			Client CLIENT = new Client( );
			Thread Cycle = new Thread( ( ) => CLIENT.Cycle( ) );

			CLIENT.TCPClient = ClientSocket;
			CLIENT.TCPSocket = ClientSocket.Client;
			CLIENT.CycleThread = Cycle;
			CLIENT.Initialize( "NULL_NICK_NAME", ClientSocket.Client.RemoteEndPoint.ToString( ) );

			Cycle.IsBackground = true;
			Cycle.Start( );

			Clients.Add( CLIENT );

			Console.WriteLine( "클라이언트가 접속 ... [" + CLIENT.ClientData.Value.IP + "] [" + CLIENT.ClientData.Value.Nick + "]" );
			Console.WriteLine( ( Clients.Count - 1 ) + " -> " + Clients.Count );
		}

		public static void ClientDisconnect( Client Client )
		{
			Console.WriteLine( "클라이언트가 접속 종료 ... [" + Client.ClientData.Value.IP + "] [" + Client.ClientData.Value.Nick + "]" );
			Clients.Remove( Client );
			Client.CycleThread.Interrupt( );

			Console.WriteLine( ( Clients.Count + 1 ) + " -> " + Clients.Count );
		}
	}
}
