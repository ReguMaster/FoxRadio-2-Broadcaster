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
		private const string SONG_FILE_DIR = "SONG.cfg";
		private static Timer SongTickTimer;
		public static List<SongList> Songs = new List<SongList>( );
		public static MemoryStream CurrentMusicMS = new MemoryStream( );
		private static int CurrentSongIndex = 0;
		public static int CurrentSongTickLocation = 0;
		private static int CurrentSongTickMaxLocation = 0;
		public static string MusicBase64Cache = "";

		public static void LoadSongListFromFile( )
		{
			if ( System.IO.File.Exists( SONG_FILE_DIR ) )
			{
				string[ ] DirSongs = Directory.GetFiles( "SONG", "*.*" );

				foreach ( string Song in DirSongs )
				{
					if ( File.Exists( Song ) )
					{
						string[ ] DataSplit = Path.GetFileNameWithoutExtension( Song ).Split( '-' );

						using ( TagLib.File TagFile = TagLib.File.Create( Song ) )
						{
							Console.WriteLine( "SONG REGISTER : < Location : " + Song + " / Name : " + DataSplit[ 0 ] + " / Author : " + DataSplit[ 1 ] + " / Length : " + ( int ) TagFile.Properties.Duration.TotalSeconds );

							Songs.Add( new SongList( Song, DataSplit[ 1 ].Trim( ), DataSplit[ 0 ].Trim( ), ( int ) TagFile.Properties.Duration.TotalSeconds ) );
						}
					}
					else
					{
						Console.WriteLine( "Song File not Found! " + Song );
					}


				}
			}
			else
			{
				Console.WriteLine( "Song Config file not Found!" );
			}
		}

		public static void InitializeCycle( )
		{
			CurrentSongIndex = 0;

			SongList NewSong = Music.FindSongByIndex( CurrentSongIndex );

			Console.WriteLine( "Initialize Cycle : " + NewSong.SongAuthor + " - " + NewSong.SongName + " [" + CurrentSongIndex + "]" );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );

				string Base64 = Convert.ToBase64String( CurrentMusicMS.ToArray( ) );

				Music.MusicBase64Cache = Base64;

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength + 5;

				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Music.MusicBase64Cache );
				string infoData = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, MakeSongInformationForProtocolSend( GetCurrentSong( ) ) );

				for ( int i = 0; i < Server.Clients.Count; i++ )
				{
					Client Client = Server.Clients[ i ];

					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( "음악 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );

						Client.SendData( infoData, ( ) =>
						{
							Console.WriteLine( "음악 정보 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );
						} );
					} );
				}
			}
			else
			{
				// Song file Not found
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

			Console.WriteLine( "Next Cycle : " + NewSong.SongAuthor + " - " + NewSong.SongName + " [" + CurrentSongIndex + "]" );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength + 5;

				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Music.MusicBase64Cache );
				string infoData = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, MakeSongInformationForProtocolSend( GetCurrentSong( ) ) );

				for ( int i = 0; i < Server.Clients.Count; i++ )
				{
					Client Client = Server.Clients[ i ];

					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( "음악 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );

						Client.SendData( infoData, ( ) =>
						{
							Console.WriteLine( "음악 정보 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );
						} );
					} );
				}
			}
			else
			{
				// Song Not found
			}
		}

		public static void SetCycle( int Index )
		{
			if ( Index < Songs.Count )
				CurrentSongIndex = Index;

			SongList NewSong = Music.FindSongByIndex( CurrentSongIndex );

			Console.WriteLine( "Set Cycle : " + NewSong.SongAuthor + " - " + NewSong.SongName + " [" + Index + "]" );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );

				string Base64 = Convert.ToBase64String( CurrentMusicMS.ToArray( ) );

				Music.MusicBase64Cache = Base64;

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength + 5;

				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Music.MusicBase64Cache );
				string infoData = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, MakeSongInformationForProtocolSend( GetCurrentSong( ) ) );

				for ( int i = 0; i < Server.Clients.Count; i++ )
				{
					Client Client = Server.Clients[ i ];

					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( "음악 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );

						Client.SendData( infoData, ( ) =>
						{
							Console.WriteLine( "음악 정보 데이터 전송 ... [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );
						} );
					} );
				}
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

		public static SongList GetCurrentSong( )
		{
			return Songs[ CurrentSongIndex ];
		}

		public static string MakeSongInformationForProtocolSend( SongList Song )
		{
			return string.Format( "{0}#{1}#{2}", Song.SongName, Song.SongAuthor, CurrentSongTickLocation );
		}

		public static void PrecacheMusicData( )
		{
			int NewIndex = CurrentSongIndex + 1;

			if ( NewIndex > Songs.Count )
				NewIndex = 0;

			SongList NewSong = Music.FindSongByIndex( NewIndex );

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				using ( MemoryStream CachedMusicMS = new MemoryStream( ) )
				{
					FileStream.CopyTo( CurrentMusicMS );
					FileStream.Close( );
					CachedMusicMS.Position = 0;

					MusicBase64Cache = Convert.ToBase64String( CachedMusicMS.ToArray( ) );
				}

				Console.WriteLine( "PrecacheMusicData : " + NewSong.SongAuthor + " - " + NewSong.SongName + " [" + CurrentSongIndex + "]" );
			}
		}

		private static void SongTickTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			if ( CurrentSongTickMaxLocation > CurrentSongTickLocation )
			{
				if ( CurrentSongTickMaxLocation - 5 == CurrentSongTickLocation )
				{
					Console.WriteLine( "PreCache Base64 Data!" );

					PrecacheMusicData( );
				}

				CurrentSongTickLocation++;
				//Console.WriteLine( "Current Song Position : " + CurrentSongTickLocation + " / " + CurrentSongTickMaxLocation );
			}
			else
				NextCycle( );
		}
	}
}
