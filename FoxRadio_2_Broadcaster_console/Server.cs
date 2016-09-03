using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxRadio_2_Broadcaster_console
{
	public static class Server
	{
		private static List<Client> Clients = new List<Client>( );

		public static void Start( )
		{
			TcpListener SERVER = new TcpListener( IPAddress.Parse( "182.212.36.48" ), 12345 );
			SERVER.Start( );

			Console.WriteLine( "Server Started!" );

			while ( true )
			{
				System.Threading.Thread.Sleep( 100 );

				TcpClient clientsocket = SERVER.AcceptTcpClient( );

				ClientConnect( clientsocket );
			}
		}

		public static void ClientConnect( TcpClient ClientSocket )
		{
			Client CLIENT = new Client( );
			Thread Cycle = new Thread( ( ) => CLIENT.Cycle( ) );

			CLIENT.Initialize( "NICK_NAME", ClientSocket.Client.RemoteEndPoint.ToString( ) );
			

			CLIENT.TCPClient = ClientSocket;
			CLIENT.TCPSocket = ClientSocket.Client;
			CLIENT.CycleThread = Cycle;
			CLIENT.Send( );

			Cycle.IsBackground = true;
			Cycle.Start( );

			Clients.Add( CLIENT );

			Console.WriteLine( " <- " + Clients.Count );
		}

		public static void ClientDisconnect( Client Client )
		{
			Console.WriteLine( "CLIENT DISCONNECT [SERVER]" );
			Clients.Remove( Client );
			Client.CycleThread.Interrupt( );

			Console.WriteLine( "-> " + Clients.Count );

			// For test
			foreach ( Client cl in Clients )
			{
				//Console.WriteLine( cl.ClientData.Value.IP + " / " + cl.ClientData.Value.Nick );
			}
		}
	}
}
