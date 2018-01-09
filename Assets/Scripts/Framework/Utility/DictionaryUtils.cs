using System.Collections.Generic;

public static class DictionaryExtensions {
    
    /// <summary>
    /// Merge this dictionary with the source dictionary. Duplicate keys will be
    /// overwritten with values from source, unless the overwrite flag = false
    /// </summary>
    /// <param name="source">Source dictionary used for merge</param>
    public static void Merge<TKey, TValue>( this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source, bool overwrite = true ) {
        foreach ( KeyValuePair<TKey, TValue> item in source )
            if ( overwrite || !target.ContainsKey( item.Key ) ) target[item.Key] = item.Value;        
    }

    /// <summary>
    /// Merge this dictionary with the source dictionary. Duplicate keys and their values 
    /// will be extracted and returned.
    /// </summary>
    /// <param name="source">Source dictionary used for merge</param>
    public static Dictionary<TKey, TValue> MergeAndExtractDuplicates<TKey, TValue>( this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source ) {
        Dictionary<TKey, TValue> duplicates = new Dictionary<TKey, TValue>();
        Dictionary<TKey, TValue> itemTarget;
        foreach ( KeyValuePair<TKey, TValue> item in source ) {
            itemTarget = target.ContainsKey( item.Key ) ? duplicates : target;
            itemTarget[item.Key] = item.Value;
        }

        return duplicates;
    }
}
