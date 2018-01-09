using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ProfilerUtil : ILoggable
{	
    private static readonly string MEMORY_TRACE_FILE_PATH_ROOT = Application.persistentDataPath;
    private static readonly string MEMORY_TRACE_FILE_PATH_TEMPLATE = "{0}/memory-trace-{1}-{2}.csv";
    private static readonly string MEMORY_TRACE_CUSTOM_FILE_PATH_TEMPLATE = "{0}/memory-trace-{1}-{2}-{3}.csv";

    #if ENABLE_PROFILER
    private static readonly HashSet<Type> MEMORY_TRACE_CONDENSED_TYPE_WHITELIST = new HashSet<Type> {
        typeof(Texture),
        typeof(Texture2D),
        typeof(Shader),
        typeof(Material),
        typeof(LightProbes),
        typeof(Mesh),
        typeof(Avatar),
        typeof(AnimationClip),
        typeof(Animator),
        typeof(RuntimeAnimatorController),
        typeof(ParticleSystem),
        typeof(Font),
        typeof(AssetBundle),
    };
    #endif

    [Inject]
    private TimeInfo _timeInfo;

    public void MemoryTrace( string traceId = null ) {
        #if ENABLE_PROFILER
        Stopwatch stopWatch = Stopwatch.StartNew();
        MemoryTraceObject[] traceObjects = CollectMemoryTrace();
        stopWatch.Stop();

        this.Log("MemoryTrace() collection took " + stopWatch.ElapsedMilliseconds + "ms");

        long timestamp = _timeInfo.GetCurrentTimestamp();

        WriteTraceFile(
            BuildMemoryTraceReport( traceObjects ),
            GetTraceFilePath( "full", timestamp, traceId )
        );

        WriteTraceFile(
            BuildMemoryTraceReport( traceObjects, MEMORY_TRACE_CONDENSED_TYPE_WHITELIST ),
            GetTraceFilePath( "condensed", timestamp, traceId )
        );

        #else
        this.LogWarning("MemoryTrace() not available on release builds");
        #endif
    }

    private MemoryTraceObject[] CollectMemoryTrace() {
        #if ENABLE_PROFILER
        // Unload all assets that are no longer reachable by walking the unity object tree
        Resources.UnloadUnusedAssets();

        UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
        MemoryTraceObject[] result = new MemoryTraceObject[objects.Length];

        for( int i = 0; i < objects.Length; ++i ) {
            UnityEngine.Object instance = objects[i];
            result[i] = new MemoryTraceObject {
                Instance = instance,
                Size = Profiler.GetRuntimeMemorySize(instance),
                TypeName = instance.GetType().ToString(),
                MetaData = GetMetaDataForObject(instance) 
            };
        }

        // Sort by Type, then by size descending, to group types together
        Array.Sort( result, (x,y) => {
            int cmp = x.TypeName.CompareTo( y.TypeName );
            if ( cmp != 0 ) return cmp;
            return y.Size.CompareTo( x.Size );
        } );
        
        return result;
        #else
        this.LogWarning("CollectMemoryTrace() not available on release builds");
        return null;
        #endif
    }

    private string BuildMemoryTraceReport( MemoryTraceObject[] traceObjects, HashSet<Type> filter = null ) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Size,Type,InstanceID,Name,Metadata");

        foreach( MemoryTraceObject traceObject in traceObjects ) {
            UnityEngine.Object instance = traceObject.Instance;
            // Apply filter, if any
            if ( filter != null && !filter.Contains( instance.GetType() ) ) continue;
            sb.AppendFormat("{0},\"{1}\",{2},\"{3}\",{4}\n", traceObject.Size, instance.GetType().Name, instance.GetInstanceID(), instance.name, traceObject.MetaData);
        }
        return sb.ToString();
    }

    private string GetMetaDataForObject( UnityEngine.Object target ) {
        string result = string.Empty;
        if ( target is Texture ) {            
            if ( target is Texture2D ) {
                Texture2D tex = (Texture2D)target;
                result = String.Format("\"{0}x{1}, fmt={2}, mip={3}\"", tex.width, tex.height, tex.format.ToString(), tex.mipmapCount );
            } else {
                Texture tex = (Texture)target;
                result = String.Format("{0}x{1}", tex.width, tex.height);
            }
        } else if ( target is Mesh ) {
            Mesh mesh = (Mesh)target;
            result = String.Format("\"vert={0}, tri={1}, norm={2}\"", mesh.vertexCount, mesh.triangles != null ? mesh.triangles.Length/3 : 0, mesh.normals != null ? mesh.normals.Length : 0 );
            
        } else if ( target is AnimationClip ) {
            AnimationClip clip = (AnimationClip)target;
            result = String.Format("\"len={0}, frameRate={1}\"", clip.length, clip.frameRate );
        } else if ( target is ParticleSystem ) {
            ParticleSystem particleSystem = (ParticleSystem)target;
			ParticleSystem.MainModule main = particleSystem.main;
			result = String.Format("\"maxParticles={0}, emissionRate={1}, duration={2}\"", main.maxParticles, particleSystem.emission.rateOverTime, main.duration );
        } else if ( target is LightProbes ) {
            LightProbes lightProbes = (LightProbes)target;
            result = String.Format("\"count={0}, cells={1}\"", lightProbes.count, lightProbes.cellCount );
        }
        return result;
    }

    private void WriteTraceFile( string reportText, string reportFilePath ) {
        try {
            File.WriteAllText( reportFilePath, reportText ); 
            this.Log("Wrote memory trace to file '" + reportFilePath + "'");
        } catch ( Exception e ) {
            this.LogError("Exception writing memory trace to '" + reportFilePath + "': " + e.ToString() );
        }
    }

    private string GetTraceFilePath( string traceType, long timestamp, string traceId = null ) {
        return String.IsNullOrEmpty( traceId ) ? 
            String.Format( MEMORY_TRACE_FILE_PATH_TEMPLATE, MEMORY_TRACE_FILE_PATH_ROOT, traceType, timestamp ) :
            String.Format( MEMORY_TRACE_CUSTOM_FILE_PATH_TEMPLATE, MEMORY_TRACE_FILE_PATH_ROOT, traceType, traceId, timestamp );
    }

    private struct MemoryTraceObject {
        public UnityEngine.Object Instance;        
        public int Size;
        public string TypeName;
        public string MetaData;
    }
}
