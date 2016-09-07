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

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength;

				foreach ( Client Client in Server.Clients )
				{
					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( "전송 ...! : SEND MUSIC" );

						Client.SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, Music.MakeSongInformationForProtocolSend( Music.GetCurrentSong( ) ) ), ( ) =>
						{
							Console.WriteLine( "전송 ...! : SEND MUSIC INFORMATION [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );
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

			if ( File.Exists( NewSong.SongLocation ) )
			{
				FileStream FileStream = new FileStream( NewSong.SongLocation, FileMode.Open );
				CurrentMusicMS = new MemoryStream( );
				FileStream.CopyTo( CurrentMusicMS );
				FileStream.Close( );

				CurrentSongTickLocation = 0;
				CurrentSongTickMaxLocation = NewSong.SongLength;

				string data = Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicPlay, Convert.ToBase64String( CurrentMusicMS.ToArray( ) ) );

				foreach ( Client Client in Server.Clients )
				{
					Client.SendData( data, ( ) =>
					{
						Console.WriteLine( "전송 ...! : SEND MUSIC" );

                        Client.SendData( Protocol.MakeProtocol<ClientProtocolMessage>( ClientProtocolMessage.MusicInformation, Music.MakeSongInformationForProtocolSend( Music.GetCurrentSong( ) ) ), ( ) =>
                        {
                            Console.WriteLine( "전송 ...! : SEND MUSIC INFORMATION [ " + Client.ClientData.Value.IP + " ][ " + Client.ClientData.Value.Nick + " ]" );
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

		private static void SongTickTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			if ( CurrentSongTickMaxLocation > CurrentSongTickLocation )
			{
				CurrentSongTickLocation++;
				Console.WriteLine( "Current Song Position : " + CurrentSongTickLocation + " / " + CurrentSongTickMaxLocation );
			}
			else
				NextCycle( );
		}
	}
}
