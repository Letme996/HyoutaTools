﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HyoutaUtils;
using NumberUtils = HyoutaUtils.NumberUtils;

namespace HyoutaTools.LastRanker {
	public class SCMP {
		public static int ExecuteExtract( List<string> args ) {
			if ( args.Count < 1 ) {
				Console.WriteLine( "Usage: SCMPfile" );
				return -1;
			}

			new SCMP( System.IO.File.ReadAllBytes( args[0] ) ).GenerateFiles( args[0] + ".ex" );

			return 0;
		}
		public static int ExecutePack( List<string> args ) {
			if ( args.Count < 2 ) {
				Console.WriteLine( "Usage: Dir NewSCMPfile" );
				return -1;
			}

			new SCMP().CreateFromDirectory( args[0], args[1] );

			return 0;
		}

		public SCMP( String filename ) {
			if ( !LoadFile( System.IO.File.ReadAllBytes( filename ) ) ) {
				throw new Exception( "SCMP: Load Failed!" );
			}
		}
		public SCMP( byte[] Bytes ) {
			if ( !LoadFile( Bytes ) ) {
				throw new Exception( "SCMP: Load Failed!" );
			}
		}
		public SCMP() { }

		public byte[] File;
		public List<uint> FilePointers;

		private bool LoadFile( byte[] File ) {
			this.File = File;
			FilePointers = new List<uint>();

			for ( int pos = 0x04; ; pos += 4 ) {
				uint ptr = BitConverter.ToUInt32( File, pos );
				if ( pos >= 8 && ptr == 0 ) break; // look at this ridiculous break condition, surely won't come back to bite me in the ass later
				FilePointers.Add( ptr );
			}
			FilePointers.Add( (uint)File.Length );

			return true;
		}

		public void GenerateFiles( String Outdir ) {
			System.IO.Directory.CreateDirectory( Outdir );
			for ( int i = 0; i < FilePointers.Count - 1; ++i ) {
				uint start = FilePointers[i];
				uint end = FilePointers[i + 1];
				uint count = end - start;

				string extension = Encoding.ASCII.GetString( File, (int)start, 4 ).TrimEnd( '\0' );
				string outfilename = i.ToString( "D4" ) + "." + extension;

				string outpath = System.IO.Path.Combine( Outdir, outfilename );
				var fs = new System.IO.FileStream( outpath, System.IO.FileMode.Create );

				fs.Write( File, (int)start, (int)count );
				fs.Close();
			}
		}

		public void CreateFromDirectory( string Dir, string Outfile ) {
			string[] Filepaths = System.IO.Directory.GetFiles( Dir );

			ushort Filecount = (ushort)Filepaths.Length;
			uint RequiredBytesForHeader = NumberUtils.Align( Filecount * 4u + 4u, 0x50u ); // pretty sure this is not right but should work
			var Filestream = new System.IO.FileStream( Outfile, System.IO.FileMode.Create );

			// header
			Filestream.WriteByte( (byte)'S' ); Filestream.WriteByte( (byte)'C' );
			Filestream.WriteByte( (byte)'M' ); Filestream.WriteByte( (byte)'P' );
			uint TotalFilesize = RequiredBytesForHeader;
			foreach ( string Path in Filepaths ) {
				Filestream.Write( BitConverter.GetBytes( TotalFilesize ), 0, 4 );
				TotalFilesize += (uint)( new System.IO.FileInfo( Path ).Length );
				TotalFilesize = TotalFilesize.Align( 0x10u );
			}
			while ( Filestream.Length < RequiredBytesForHeader ) { Filestream.WriteByte( 0x00 ); }

			// files
			foreach ( string Path in Filepaths ) {
				var File = new System.IO.FileStream( Path, System.IO.FileMode.Open );
				StreamUtils.CopyStream( File, Filestream, (int)File.Length );
				File.Close();
				while ( Filestream.Length % 0x10 != 0 ) { Filestream.WriteByte( 0x00 ); }
			}

			Filestream.Close();
		}
	}
}
