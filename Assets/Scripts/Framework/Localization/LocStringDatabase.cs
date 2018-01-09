using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LocEntry = System.Collections.Generic.KeyValuePair<string, string>;

public class LocStringDatabase : ILoggable
{
    public enum Mode { Resources, FileSystem }
    
    private const string MISSING_LOC_KEY_FORMAT = "[{0}]";
    private const string COUNTRY_PREFIX = "COUNTRY_NAME_";
    private const string KEY_POINTER = "__";
    private const char cutChar = '|';
    private const string LOC_KEY_MAPPING_FILE = "Loc_Key_Mapping.txt";
    
    private string _path;
    private string _fileNamePattern;
    private Mode _mode;
    private Dictionary<string, string> _currentLanguage = new Dictionary<string, string>();
    
    /// <summary>
    /// Determines if all the required localization files are available at the given file path.
    /// </summary>
    public static bool HasAllRequiredLocFiles (string path, string fileNamePattern)
    {
        foreach (string language in LanguageTypeExtensions.AllLanguageCodes)
        {
            string languageFilePath = Path.Combine(path, string.Format(fileNamePattern, language));
            if (!File.Exists(languageFilePath))
            {
                return false;
            }
        }
        
        string locKeyMappingFilePath = Path.Combine(path, LOC_KEY_MAPPING_FILE);
        return File.Exists(locKeyMappingFilePath);
    }
    
    public LocStringDatabase (string path, string fileNamePattern, Mode mode)
    {
        _path = path;
        _fileNamePattern = fileNamePattern;
        _mode = mode;
    }
    
	public void LoadLanguage(LanguageType language)
	{
        _currentLanguage.Clear();
        
        string languageFilePath = Path.Combine(_path, string.Format(_fileNamePattern, language.ToString()));
        string locKeyMappingFilePath = Path.Combine(_path, LOC_KEY_MAPPING_FILE);
        
        string languageText = null;
        if (!TryReadAllText(languageFilePath, out languageText))
        {
            this.LogError("Not find the language file for " + language + " at " + languageFilePath);
            return;
        }

        ParseLanguageFile(_currentLanguage, languageText, false);
        
        string locKeyMappings = null;
        if (TryReadAllText(locKeyMappingFilePath, out locKeyMappings))
        {
            ParseLanguageFile(_currentLanguage, locKeyMappings, true);
            ResolveLocKeyMappings(_currentLanguage);
        }
	}
    
    bool TryReadAllText (string path, out string text)
    {
        return _mode == Mode.Resources ? TryReadFromResources(path, out text) : TryReadFromFileSystem(path, out text);
    }
    
    bool TryReadFromResources (string path, out string text)
    {
        TextAsset file = Resources.Load(path.Replace(".txt", string.Empty)) as TextAsset;
        if (file == null)
        {
            text = null;
            return false;
        }
        text = file.text;
        return true;
    }
    
    bool TryReadFromFileSystem (string path, out string text)
    {
        if (!File.Exists(path))
        {
            text = null;
            return false;
        }
        text = File.ReadAllText(path);
        return true;
    }

	public bool TryLocalize(string key, out string result)
	{
		bool success = _currentLanguage.TryGetValue(key, out result);
		if (!success)
		{	
			result = string.Format( MISSING_LOC_KEY_FORMAT, key );
		}
		return success;
	}
	
	private void ParseLanguageFile (Dictionary<string, string> database, string fileText, bool ignoreDuplicates = false)
	{
		string[] lines = fileText.Split('\n');
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }
                string[] splitLine = line.Split(cutChar);
                if (splitLine.Length == 2)
                {
                    if (database.ContainsKey(splitLine[0])) 
                    {
                        if (!ignoreDuplicates)
                            this.LogError("Duplicate entry = " + splitLine[0]);
                    } 
                    else
                    {
                        // swap out the literal \n strings with the real escape sequences
                        splitLine[1] = splitLine[1].Replace("\\n", "\n");
                        // Remove carriage returns because when editor loc db is enabled, it will keep them in
                        // and provide an inaccurate representation of the loc string
                        splitLine[1] = splitLine[1].Replace("\r", "");
                        database.Add(splitLine[0], splitLine[1]);
                    }
                }
            }
        }
	}
    
    private void ResolveLocKeyMappings (Dictionary<string, string> database)
    {
        List<string> redirected = new List<string>();
        foreach (LocEntry entry in database)
        {
            if (entry.Value.StartsWith(KEY_POINTER))
            {
                redirected.Add(entry.Key);
            }
        }
        
        // Assign any redirected loc strings to point to their correct values
        foreach (var entry in redirected)
        {
            // Strip the prepended character sequence from the loc key
            string newValueKey = database[entry].Substring(KEY_POINTER.Length).Replace("\r", System.String.Empty);
            string newValue = null;
            if (database.TryGetValue(newValueKey, out newValue))
            {
                database[entry] = newValue;   
            }
            else
            {
                this.LogWarning("Not able to find the redirected entry for " + newValueKey);
            }
        }
    }

    public IEnumerable<string> GetAllCountries ()
    {
        List<string> allCountries = new List<string>();
        foreach (LocEntry entry in _currentLanguage)
        {
            if (entry.Key.Contains(COUNTRY_PREFIX))
            {
                allCountries.Add(entry.Key);
            }
        }
        
        return allCountries;
    }
}
