using UnityEngine;
using System.Net;
using System.Collections;

public class WebClientLoader : ILoggable 
{

	// Only used for logger
	public static readonly WebClientLoader _instance = new WebClientLoader();

	private const string HTTP_SCHEME_PREFIX		= "http";
	private const string LOCAL_SCHEME_PREFIX	= "file://";
	private const string BACKSLASH_SEPARATOR 	= @"\";

	public static string LoadFromURL( string fileURI ) {

		//Fix windows paths. URIs should contain forward slashes on all platforms
		if (fileURI.IndexOf( BACKSLASH_SEPARATOR ) >= 0)
			fileURI = fileURI.Replace( BACKSLASH_SEPARATOR, FileUtils.PATH_SEPARATOR );

		string result = null;

		using ( WebClient webClient = new WebClient() ) {
			try {
				result = webClient.DownloadString( fileURI );
			} catch ( WebException e ) {
				_instance.LogWarning( "Could not load URL '" + fileURI + "':" + e.ToString() );
			}

		}
		return result;
	}

	public static byte[] LoadBytesFromURL( string fileURI ) {

		//Fix windows paths. URIs should contain forward slashes on all platforms
		if (fileURI.IndexOf( BACKSLASH_SEPARATOR ) >= 0)
			fileURI = fileURI.Replace( BACKSLASH_SEPARATOR, FileUtils.PATH_SEPARATOR );

		byte[] result = null;

		using ( WebClient webClient = new WebClient() ) {
			try {
				result = webClient.DownloadData( fileURI );
			} catch ( WebException e ) {
				_instance.LogWarning( "Could not load URL '" + fileURI + "':" + e.ToString() );
			}
		}
		return result;
	}

	public static string LoadFromPath( string filePath )
	{
		return LoadFromURL( LOCAL_SCHEME_PREFIX + filePath );
	}

	public static string LoadFromAppPath( string filePathFromAppRoot )
	{
		return LoadFromURL( LOCAL_SCHEME_PREFIX + FileUtils.GetProjectRoot() + FileUtils.PATH_SEPARATOR + filePathFromAppRoot );
	}

	public static byte[] LoadBytesFromPath( string filePath )
	{
		return LoadBytesFromURL( LOCAL_SCHEME_PREFIX + filePath );
	}

	public static byte[] LoadBytesFromAppPath( string filePathFromAppRoot )
	{
		return LoadBytesFromURL( LOCAL_SCHEME_PREFIX + FileUtils.GetProjectRoot() + FileUtils.PATH_SEPARATOR + filePathFromAppRoot );
	}
	

}
