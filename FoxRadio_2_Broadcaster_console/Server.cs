using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FoxRadio_2_Broadcaster_console.Protocol;

namespace FoxRadio_2_Broadcaster_console
{
	public static class Server
	{
		public static List<Client> Clients = new List<Client>( );
		public static Encoding MessageEncode = Encoding.GetEncoding( "utf-8" );

		public static void Start( )
		{
			TcpListener SERVER = new TcpListener( IPAddress.Parse( "182.212.36.48" ), 12345 );
			SERVER.Start( );

			Music.LoadSongListFromFile( );
			Music.InitializeCycle( );
			Music.NextCycle( );

			Console.WriteLine( "서버 초기화 됨!" );

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

			CLIENT.TCPClient = ClientSocket;
			CLIENT.TCPSocket = ClientSocket.Client;
			CLIENT.CycleThread = Cycle;
			CLIENT.Initialize( "NULL_NICK_NAME", ClientSocket.Client.RemoteEndPoint.ToString( ) );
			/*
			CLIENT.SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.NickNameInitialized, "L7D" ), ( ) =>
			{
				Console.WriteLine( CLIENT.ClientData.Value.Nick + " : NICK SEND" );
			} );

			string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( Music.CurrentMusicMS.ToArray( ) ) );

			CLIENT.SendData( data, ( ) =>
			{
				Console.WriteLine( CLIENT.ClientData.Value.Nick + " : SEND MUSIC" );

				CLIENT.SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicSetLocation, Music.CurrentSongTickLocation.ToString( ) ), ( ) =>
				{
					Console.WriteLine( CLIENT.ClientData.Value.Nick + " : SEND MUSIC LOCATION" );
				} );
			} );
			*/
			/*
			using ( MemoryStream ms = Music.LoadFromFile( @"C:\Users\L7D\Music\F-777 - The 7 Seas.mp3" ) )
			{
				ms.Position = 0;
				Console.WriteLine( ms.Length );
				Console.WriteLine( ms.ToArray( ).Length );

				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( ms.ToArray( ) ) );
				CLIENT.SendData( data, ( ) =>
				{
					Console.WriteLine( "PROTOCOL MESSAGE SEND TWO!!!!!" );
				} );
			}*/


			//using ( MemoryStream ms = Music.LoadFromFile( @"C:\Users\L7D\Music\BLACKPINK-01-휘파람.mp3" ) )
			//{
			//	ms.Position = 0;
			//	var sr = new StreamReader( ms );
			//	string myStr = sr.ReadToEnd( );

			//	File.WriteAllBytes( "music.mp3", ms.ToArray( ) );
			//	CLIENT.SendByteData( Protocol.MakeByteProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, ms.ToArray( ) ), ( ) =>
			//	{
			//		Console.WriteLine( "MUSIC PROTOCOL MESSAGE SEND!!!!!" );
			//	} );

			//}

			//Console.WriteLine( Encoding.ASCII.GetString( ms.ToArray( ) ) );


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

			// For test
			foreach ( Client cl in Clients )
			{
				//Console.WriteLine( cl.ClientData.Value.IP + " / " + cl.ClientData.Value.Nick );
			}
		}
	}
}
