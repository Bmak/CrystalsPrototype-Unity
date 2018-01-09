using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

/// <summary>
/// LocalizationDO this is container class for localization files. Too much logic here. Refactoring needed
/// </summary>
public class LocalizationDO : ILoggable
{   
    private static readonly string FALLBACK_LOC_FILE_DIR = "Localization";
    private static readonly string LOC_FILE_NAME = "Loc_{0}.txt";
    private static readonly string LOC_FILE_FALLBACK_NAME = "Loc_{0}.txt";
    private static readonly string LOC_VERSION_KEY = "LOC_VERSION_KEY";
    
    [Inject]
    private LocalPrefs _localPrefs;

    private string _latestLocalizationBundleVersion;
    public string LatestLocalizationBundleVersion
    {
        get { return _latestLocalizationBundleVersion; }
        set
        {
            _latestLocalizationBundleVersion = value;
        }
    }

    private readonly string _localizationBundlePath;   
    
    
    public LocalizationDO() {        
        _localizationBundlePath =  Path.Combine( Application.temporaryCachePath, "Localization" );
    }
    
    public void WriteLocalizationBundlesToDisk (byte[] bytes, string version)
    {
        
        if ( HasCachedVersion(version)) return;

        try {
            if ( Directory.Exists(_localizationBundlePath) )
                Directory.Delete(_localizationBundlePath, true);
        } catch ( IOException e ) {
            this.LogError("Exception deleting directory '" + _localizationBundlePath + "': " + e);
        }

        Directory.CreateDirectory( _localizationBundlePath );

        using (MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length))
        using (ZipInputStream zipFile = new ZipInputStream((stream)))
        {   
            ZipEntry entry = zipFile.GetNextEntry();
            while (entry != null)
            {
                string dir = Path.GetDirectoryName(entry.Name);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(Path.Combine(_localizationBundlePath, dir));
                }
                if (entry.IsFile)
                {
										
				    string filePath = Path.Combine(_localizationBundlePath, entry.Name);
                    using (var file = File.Open( filePath, FileMode.Create))
                    {
                        byte[] buffer = new byte[2048];
                        while (true)
                        {
                            int size = zipFile.Read(buffer, 0, buffer.Length);
                            if (size > 0)
                            {
                                file.Write(buffer, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    FileUtils.SetNoBackupFlag( filePath );
                }
                entry = zipFile.GetNextEntry();
            }
        }
        
        _localPrefs.SetSharedString(LOC_VERSION_KEY, version);
    }
    
    public bool HasCachedVersion (string version)
    {
        string currentVersion = _localPrefs.GetSharedString(LOC_VERSION_KEY, string.Empty);
        return currentVersion == version && HasAllRequiredLocFiles();
    }
    
    private bool HasAllRequiredLocFiles ()
    {
        return LocStringDatabase.HasAllRequiredLocFiles(_localizationBundlePath, LOC_FILE_NAME);
    }
    
    public LocStringDatabase LoadLocDatabase()
    {
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":LoadLocStringDatabase" );
		#endif	

		LocStringDatabase result =  HasAllRequiredLocFiles() ? 
            new LocStringDatabase(_localizationBundlePath, LOC_FILE_NAME, LocStringDatabase.Mode.FileSystem) : 
            new LocStringDatabase(FALLBACK_LOC_FILE_DIR, LOC_FILE_FALLBACK_NAME, LocStringDatabase.Mode.Resources);

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":LoadLocStringDatabase" );
		#endif	

		return result;
    }
}
