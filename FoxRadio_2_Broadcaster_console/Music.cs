using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static FoxRadio_2_Broadcaster_console.Protocol;

namespace FoxRadio_2_Broadcaster_console
{
	struct SongList
	{
		public string SongLocation;
		public string SongName;
		public string SongAuthor;
		public int SongLength;

		public SongList( string SongLocation, string SongName, string SongAuthor, int SongLength )
		{
			this.SongLocation = SongLocation;
			this.SongName = SongName;
			this.SongAuthor = SongAuthor;
			this.SongLength = SongLength;
		}
	}

	static class Music
	{
		private static Timer SongTickTimer;
		public static List<SongList> Songs = new List<SongList>( );
		public static MemoryStream CurrentMusicMS = new MemoryStream( );
		private static int CurrentSongIndex = 0;
		public static int CurrentSongTickLocation = 0;
		private static int CurrentSongTickMaxLocation = 0;

		public static void LoadSongListFromFile( )
		{
			string[ ] SongData = File.ReadAllLines( "SONG.cfg" );

			foreach ( string Data in SongData )
			{
				string[ ] DataSplit = Data.Split( '@' );

				Console.WriteLine( "SONG REGISTER : < Location : " + DataSplit[ 0 ] + " / Name : " + DataSplit[ 1 ] + " / Author : " + DataSplit[ 2 ] + " / Length : " + DataSplit[ 3 ] );
				Songs.Add( new SongList( DataSplit[ 0 ], DataSplit[ 1 ], DataSplit[ 2 ], int.Parse( DataSplit[ 3 ] ) ) );
			}
		}

		public static void InitializeCycle( )
		{
			CurrentSongIndex = 0;

			SongList NewSong = Music.FindSongByIndex( CurrentSongIndex );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );
				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( CurrentMusicMS.ToArray( ) ) );

				foreach ( Client Client in Server.Clients )
				{
					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( Client.ClientData.Value.Nick + " : SEND MUSIC" );
					} );
				}

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength;
			}
			else
			{
				// Song Not found
			}

			SongTickTimer = new Timer( );
			SongTickTimer.Interval = 1000;
			SongTickTimer.Elapsed += new ElapsedEventHandler( SongTickTimer_Elapsed );
			SongTickTimer.Start( );
		}

		public static void NextCycle( )
		{
			if ( CurrentSongIndex < Songs.Count )
				CurrentSongIndex++;
			else
				CurrentSongIndex = 0;

			SongList NewSong = Music.FindSongByIndex( CurrentSongIndex );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );
				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( CurrentMusicMS.ToArray( ) ) );

				foreach ( Client Client in Server.Clients )
				{
					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( Client.ClientData.Value.Nick + " : SEND MUSIC" );
					} );
				}

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength;
			}
			else
			{
				// Song Not found
			}
		}

		public static SongList FindSongByIndex( int Index )
		{
			return Songs[ Index ];
		}

		private static void SongTickTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			if ( CurrentSongTickMaxLocation > CurrentSongTickLocation )
			{
				CurrentSongTickLocation++;
				Console.WriteLine( "CUR " + CurrentSongTickLocation + " / " + CurrentSongTickMaxLocation );
			}
			else
			{
				NextCycle( );
			}
		}

		public static MemoryStream LoadFromFile( string FileDir )
		{
			FileStream FileStream = new FileStream( FileDir, FileMode.Open );

			MemoryStream MS = new MemoryStream( );
			FileStream.CopyTo( MS );
			FileStream.Close( );

			return MS;
		}
	}
}
