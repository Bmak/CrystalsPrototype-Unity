using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IniParser;


public delegate void IniLoadedCallback( IDictionary<string,string> data ); 

public class IniLoader : ILoggable
{

	private static readonly string LOCAL_FILE_SCHEME_PREFIX = @"file://";
	private static readonly string JAR_SCHEME_PREFIX = @"jar:";

	private static string[] LOCAL_SCHEME_PREFIXES = new string[] { LOCAL_FILE_SCHEME_PREFIX, JAR_SCHEME_PREFIX };
	private static string[] FILE_PATH_PREFIXES = new string[] { @"\", @"/" };

	[Inject]
	private CoroutineCreator _coroutineCreator;

	public void LoadFromURL( string fileURL, string iniSection, IniLoadedCallback loadedCallback )
    {
		_coroutineCreator.StartCoroutine( AsyncLoadFromURL( fileURL, iniSection, loadedCallback ) );
    }

    private IEnumerator AsyncLoadFromURL( string fileURL, string iniSection, IniLoadedCallback loadedCallback ) {

		fileURL = GetLocalURL( fileURL );

        this.LogTrace( "Loading ini file from URL '" + fileURL + "'", LogCategory.INITIALIZATION);

        string errorMessage;
        byte[] iniBytes = null;

        using ( WWW wwwRequest = new WWW( fileURL ) ) {
            // Wait for download to finish
            yield return wwwRequest;

            errorMessage = wwwRequest.error;
			if ( String.IsNullOrEmpty( errorMessage ) ) {
            	iniBytes = wwwRequest.bytes;
			}
        }

        if ( !String.IsNullOrEmpty( errorMessage ) ) {
			this.LogWarning("Could not load ini file from URL '" + fileURL + "': " + errorMessage );
			loadedCallback( null );
            yield break;
        }

		string iniContents = Encoding.UTF8.GetString(iniBytes, 0, iniBytes.Length);
		loadedCallback( Parse( iniContents, iniSection ) );
    }


	private string GetLocalURL( string fileURL ) {
		// We only want to operate on absolute paths			
		if ( String.IsNullOrEmpty( fileURL ) || !IsFilePath( fileURL ) ) return fileURL;
		return LOCAL_FILE_SCHEME_PREFIX + fileURL;
	}

	private bool IsFilePath( string fileURL ) {
		foreach ( string prefix in FILE_PATH_PREFIXES ) {
			if ( fileURL.StartsWith( prefix ) ) return true;
		}
		return false;
	}

	private bool IsLocalURL( string fileURL ) {
		foreach ( string prefix in LOCAL_SCHEME_PREFIXES ) {
			if ( fileURL.StartsWith( prefix ) ) return true;
		}
		return false;
	}

	public IDictionary<string,string> LoadFromURL( string fileURL, string iniSection )
    {
		string fileContents = WebClientLoader.LoadFromURL( fileURL );
		if(string.IsNullOrEmpty(fileContents))
		{
			return null;
		}
		return Parse( fileContents, iniSection );
    }

	public IDictionary<string,string> LoadFromPath( string filePath, string iniSection )
	{

		string fileContents = FileUtils.ReadFile( filePath );

		return Parse( fileContents, iniSection );
	}

	public IDictionary<string,string> LoadFromAppPath( string filePathFromAppRoot, string iniSection )
	{
		return LoadFromPath( FileUtils.GetProjectRoot() + FileUtils.PATH_SEPARATOR + filePathFromAppRoot, iniSection );
	}

    public IDictionary<string, IDictionary<string, string>> LoadAllFromResources( string directory, string iniSection )
    {
        UnityEngine.Object[] files = Resources.LoadAll(directory);
        if ( files == null ) {
            this.LogWarning("LoadAllFromResources() error: could not find directory '" + directory + "' in resources");
            return null;
        } else if ( files.Length == 0 ) {
            this.LogWarning("LoadAllFromResources() error: directory '" + directory + "' in resources is empty");
            return null;
        }

        IDictionary<string, IDictionary<string, string>> info = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach ( UnityEngine.Object file in files ) {
            TextAsset asset = file as TextAsset;

            if ( asset == null ) {
                this.LogWarning("LoadFromResources() error: '" + file.name + "' is not a txt file");
                continue;
            }

            string fileContents = asset.text;

            if ( String.IsNullOrEmpty(fileContents) ) {
                this.LogWarning("LoadFromResources() error: '" + asset.name + ".txt' is empty");
                continue;
            }

            info.Add(asset.name, Parse(fileContents, iniSection));
        }

        return info;
    }

	public IDictionary<string,string> LoadFromResources( string assetName, string iniSection )
	{
		TextAsset asset = Resources.Load(assetName) as TextAsset;
		if ( asset == null ) {
			this.LogWarning( "LoadFromResources() error: could not find asset '" + assetName + ".txt' in resources" );
			return null;
		} 

		string fileContents = asset.text;

		if ( String.IsNullOrEmpty( fileContents ) ) {
			this.LogWarning( "LoadFromResources() error: '" + assetName + ".txt' is empty" );
			return null;
		}

		return Parse( fileContents, iniSection );
	}

	public void WriteToFile( string filePath, string iniSection, IDictionary<string,string> data )
	{
		try {
			KeyDataCollection keyDataCollection = DictionaryToKeyDataCollection( data );
			IniData iniData = new IniData();
			
			SectionData sectionData = new SectionData (iniSection);
			sectionData.Keys = keyDataCollection;		
			
			iniData.Sections.SetSectionData( iniSection, sectionData );
			
			StringIniParser iniParser = new StringIniParser ();

			FileUtils.RemoveReadOnly( filePath );
			string fileContents = iniParser.WriteString( iniData );
			File.WriteAllText( filePath, fileContents );
		} catch (Exception e) {
			this.LogError("WriteToFile() error: " + e.ToString());
		}
	}

	private IDictionary<string,string> Parse( string data, string iniSection ) {

		if( String.IsNullOrEmpty(data) ) {
			this.LogError("Parse() error: empty or null content");
			return null;
		}

		IniData iniData = null;

		try {
			StringIniParser iniParser = new StringIniParser();
			iniData = iniParser.ParseString( data );
		} catch (Exception e) {
			this.LogError("Parse() error: " + e.ToString());
		}

		if ( iniData == null ) {
			this.LogError ("Parse() error: ini parse failed due to invalid data");
			return null;			
		}

		KeyDataCollection keyDataCollection = null;

		try {
			keyDataCollection = iniData[iniSection];
		} catch (Exception e) {
			this.LogError("Parse() exception: "+ e.ToString());	
			return null;
		}

		if (keyDataCollection == null) {
			this.LogError("Parse() error: could not find section '" + iniSection + "' in ini");	
			return null;
		}

		return KeyDataCollectionToDictionary( keyDataCollection );
	}

	/// <summary>
	/// Method to prevent leaking KeyDataCollection to calling code
	/// </summary>
	/// <returns>The dictionary converted from a KeyDataCollection.</returns>
	/// <param name="keyDataCollection">Key data collection.</param>
	private IDictionary<string, string> KeyDataCollectionToDictionary( KeyDataCollection keyDataCollection ) {

		if( keyDataCollection == null ) return null;

		IDictionary<string, string> result = new Dictionary<string,string>(keyDataCollection.Count);

		foreach (KeyData keyData in keyDataCollection) 
			result[keyData.KeyName] = keyData.Value;

		return result;
	}

	/// <summary>
	/// Method to prevent leaking KeyDataCollection to calling code
	/// </summary>
	/// <returns>The dictionary converted from a KeyDataCollection.</returns>
	/// <param name="keyDataCollection">Key data collection.</param>
	private KeyDataCollection DictionaryToKeyDataCollection( IDictionary<string, string> data ) {

		if( data == null ) return null;

		KeyDataCollection result = new KeyDataCollection();

		foreach( KeyValuePair<string, string> kvp in data )
			result.AddKey( kvp.Key, kvp.Value );

		return result;
	}
}