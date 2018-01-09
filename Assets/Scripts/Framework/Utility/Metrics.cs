#if UNITY_EDITOR
#define METRICS_ENABLED
#define INCLUDE_DEV_METRICS
#define INCLUDE_PERFORMANCE_METRICS
#endif

#if METRICS_ENABLED
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonFx.Json;
using MetricsInternal;
#if UNITY_EDITOR
using UnityEditor;
#endif


public static class Metrics
{
    #if UNITY_EDITOR
    // This is to prevent Metrics hooks from running in Editor (non-Play) mode, which can result in the 
    // a GameObject with the MetricsRegistry component being added to the main scene
    private static bool _playMode = false;
    #endif

    private static MetricsRegistry _metricsRegistry;
    private static TimerMetrics _timerMetrics;
    private static FPSMetrics _fpsMetrics;
    private static MemoryMetrics _memoryMetrics;
	private static CounterMetrics _counterMetrics;
	private static ValueMetrics _valueMetrics;

    static Metrics()
    {
        #if UNITY_EDITOR
        _playMode = Application.isPlaying;
        #endif
        Initialize();
    }

    private static void Initialize()
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _metricsRegistry = MetricsRegistry.Build();

        _timerMetrics = _metricsRegistry.Register( new TimerMetrics() );
        _fpsMetrics = _metricsRegistry.Register( new FPSMetrics() );
        _memoryMetrics = _metricsRegistry.Register( new MemoryMetrics() );
		_counterMetrics = _metricsRegistry.Register( new CounterMetrics() );
		_valueMetrics = _metricsRegistry.Register( new ValueMetrics() );
    }

	#if INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS
	private static readonly string REPORT_PATH = UnityEngine.Application.persistentDataPath;

    public static void WriteReports()
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif

        new XMLTimerReport( _timerMetrics, REPORT_PATH, "Timer-Metrics-Report.xml" ).Save();
        new CSVTimerReport( _timerMetrics, REPORT_PATH, "Timer-Metrics-Report.csv" ).Save();
        new XMLTimerTagReport( _timerMetrics, REPORT_PATH, "Timer-Tag-Metrics-Report.xml" ).Save();
        new CSVTimerTagReport( _timerMetrics, REPORT_PATH, "Timer-Tag-Metrics-Report.csv" ).Save();

        new XMLFPSReport( _fpsMetrics, REPORT_PATH, "FPS-Metrics-Report.xml" ).Save();
        new CSVFPSReport( _fpsMetrics, REPORT_PATH, "FPS-Metrics-Report.csv" ).Save();
        new XMLFPSTagReport( _fpsMetrics, REPORT_PATH, "FPS-Tag-Metrics-Report.xml" ).Save();
        new CSVFPSTagReport( _fpsMetrics, REPORT_PATH, "FPS-Tag-Metrics-Report.csv" ).Save();

        new XMLMemoryReport( _memoryMetrics, REPORT_PATH, "Memory-Metrics-Report.xml" ).Save();
        new CSVMemoryReport( _memoryMetrics, REPORT_PATH, "Memory-Metrics-Report.csv" ).Save();
        new XMLMemoryTagReport( _memoryMetrics, REPORT_PATH, "Memory-Tag-Metrics-Report.xml" ).Save();
        new CSVMemoryTagReport( _memoryMetrics, REPORT_PATH, "Memory-Tag-Metrics-Report.csv" ).Save();

		new XMLCounterReport( _counterMetrics, REPORT_PATH, "Counter-Metrics-Report.xml" ).Save();
		new CSVCounterReport( _counterMetrics, REPORT_PATH, "Counter-Metrics-Report.csv" ).Save();

		new XMLValueReport( _valueMetrics, REPORT_PATH, "Value-Metrics-Report.xml" ).Save();
		new CSVValueReport( _valueMetrics, REPORT_PATH, "Value-Metrics-Report.csv" ).Save();
    }

    #if UNITY_EDITOR
    [MenuItem("OctoBox/Write Metrics Reports", false, 500)]
    public static void WriteMetricsReportMenuItem()
    {
        Metrics.WriteReports();
    }
    #endif

	#endif

    public static void Start( string id, params string[] tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _timerMetrics.Start( id, tags );
    }
    
    public static void End( string id, params string[] additional_tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _timerMetrics.End( id, additional_tags );
    }

    public static void AddMeta( string id, params string[] meta )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _timerMetrics.AddMeta( id, meta );
    }

    public static void StartFPS( string id, params string[] tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _fpsMetrics.Start( id, tags );
    }
    
    public static void EndFPS( string id, params string[] additional_tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _fpsMetrics.End( id, additional_tags );
    }

	public static void AddFPSMeta( string id, params string[] meta )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _fpsMetrics.AddMeta( id, meta );
    }

    public static void StartMem( string id, params string[] tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _memoryMetrics.Start( id, tags );
    }
    
    public static void EndMem( string id, params string[] additional_tags )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _memoryMetrics.End( id, additional_tags );
    }

	public static void AddMemMeta( string id, params string[] meta )
    {
        #if UNITY_EDITOR
        if (!_playMode) return;
        #endif
        _memoryMetrics.AddMeta( id, meta );
    }

	public static void Count( string id, uint value = 1, params string[] tags )
	{
		#if UNITY_EDITOR
		if (!_playMode) return;
		#endif
		_counterMetrics.Count( id, value, tags );
	}
	
	public static void AddCountMeta( string id, params string[] meta )
	{
		#if UNITY_EDITOR
		if (!_playMode) return;
		#endif
		_counterMetrics.AddMeta( id, meta );
	}

	public static void Value( string id, int value = 0, params string[] meta )
	{
		#if UNITY_EDITOR
		if (!_playMode) return;
		#endif
		_valueMetrics.Value( id, value, meta );
	}

    public static void Clear()
    {
        Initialize();       
    }

    public static void Reset()
    {
        _metricsRegistry.Reset();   
    }

    public static void IncrementalReset()
    {
        _metricsRegistry.IncrementalReset();
    }

    public static TimerEventResult[] GetIncrementalTimerResults()
    {
        return _timerMetrics.GetIncrementalResults().ToArray();
    } 

    public static FPSEventResult[] GetIncrementalFPSResults()
    {
        return _fpsMetrics.GetIncrementalResults().ToArray();
    } 

    public static MemoryEventResult[] GetIncrementalMemoryResults()
    {
        return _memoryMetrics.GetIncrementalResults().ToArray();
    } 

	public static CounterEventResult[] GetIncrementalCounterResults()
	{
		return _counterMetrics.GetIncrementalResults().ToArray();
	} 

	public static ValueEventResult[] GetIncrementalValueResults()
	{
		return _valueMetrics.GetIncrementalResults().ToArray();
	} 

	public static TimerEventResult[] GetTimerResults()
	{
		return _timerMetrics.GetResults().ToArray();
	} 
	
	public static FPSEventResult[] GetFPSResults()
	{
		return _fpsMetrics.GetResults().ToArray();
	} 
	
	public static MemoryEventResult[] GetMemoryResults()
	{
		return _memoryMetrics.GetResults().ToArray();
	} 
	
	public static CounterEventResult[] GetCounterResults()
	{
		return _counterMetrics.GetResults().ToArray();
	} 
	
	public static ValueEventResult[] GetValueResults()
	{
		return _valueMetrics.GetResults().ToArray();
	} 
}


namespace MetricsInternal {

    public class MetricsRegistry : MonoBehaviour, ILoggableMetrics
    {
		#if INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS
        // Used to trigger metrics write by flipping device face down
        private readonly int CHECK_DEVICE_ORIENTATION_FRAME_INTERVAL = 10;
        private int _checkDeviceOrientationFrameCount;
        private DeviceOrientation _lastDeviceOrientation = DeviceOrientation.Unknown;
		#endif

        public static MetricsRegistry Build() {
            string objectName = "_" + typeof(MetricsRegistry).Name;
            GameObject go = GameObject.Find( objectName );
            // Destroy existing gameobject, if any (from a possible previous invocation)
            if ( null != go ) UnityEngine.Object.Destroy( go );
            // Create new Go+component
            go = new GameObject( objectName );
            MetricsRegistry result =  go.AddComponent<MetricsRegistry>();
            return result.Initialize();
        }   

        private Stopwatch _stopwatch;
        private Dictionary<Type, IMetrics> _registeredMetrics = new Dictionary<Type, IMetrics>();

        public MetricsRegistry Initialize()
        {
            _stopwatch = Stopwatch.StartNew();
            return this;
        }

        public T Register<T>( T metrics ) where T : IMetrics
        {
            metrics.SetStopwatch( _stopwatch );
            _registeredMetrics.Add( typeof(T), metrics );
            return metrics;
        }

        public MetricsType Get<MetricsType>() where MetricsType : IMetrics
        {
            Type metricsType = typeof(MetricsType);
            IMetrics metricsInstance;
            if ( !_registeredMetrics.TryGetValue( metricsType, out metricsInstance ) ) {
                this.LogWarning("No metrics of type '" + metricsType.Name + "' found.");
                return default(MetricsType);
            }
            return (MetricsType)metricsInstance;
        }

        public void Update()
        {
            foreach( IMetrics metrics in _registeredMetrics.Values )
                metrics.Update();

			#if INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS
            if ( ++_checkDeviceOrientationFrameCount > CHECK_DEVICE_ORIENTATION_FRAME_INTERVAL )
                CheckDeviceOrientation();
			#endif
        }

        public void Reset()
        {
            foreach( IMetrics metrics in _registeredMetrics.Values )
                metrics.Reset();
        }

        public void IncrementalReset()
        {
            foreach( IMetrics metrics in _registeredMetrics.Values )
                metrics.IncrementalReset();
        }

        public void Clear()
        {
            foreach( IMetrics metrics in _registeredMetrics.Values )
                metrics.Clear();
        }

		#if INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS
        private void CheckDeviceOrientation() {
            // Reset frame interval
            _checkDeviceOrientationFrameCount = 0;
    
            // Input.deviceOrientation is extern into native code, so we only 
            // access it periodically to avoid a performance hit
            DeviceOrientation currentDeviceOrientation = Input.deviceOrientation;
    
            // Avoid repeatedly writing metrics to disk by requiring an orientation state change
            if ( currentDeviceOrientation == _lastDeviceOrientation ) return;
    
            // On transition to device face down, we write metrics to disk
            if ( currentDeviceOrientation == DeviceOrientation.FaceDown )
                Metrics.WriteReports(); // It's messy to call statically back into Metrics here, but... oh well
    
            // Save last orientation for next update check
            _lastDeviceOrientation = currentDeviceOrientation;
        }
		#endif
    }

    public interface IMetrics
    {
        void Update();
        void Reset();
        void IncrementalReset();
        void Clear();
        void SetStopwatch( Stopwatch stopwatch );
    }

 	public interface IMetricsResults<ResultType>
    {
        List<ResultType> GetResults();
    }

    abstract public class Metrics<EventType, ResultType> : IMetrics, IMetricsResults<ResultType>, ILoggableMetrics where EventType : MetricsEvent<ResultType>
    {
        protected Stopwatch _stopwatch;
		// Maintains all completed events
        protected LinkedList<EventType> _events;
		// Maintains all outstanding events awaiting completion
        protected Dictionary<string, EventType> _activeEvents;
		// Tracks the current offset node in _events to allow incremental reset
        protected LinkedListNode<EventType> _incrementalNode;

        public Metrics() {
            _events = new LinkedList<EventType>();
            _activeEvents = new Dictionary<string, EventType>();
        }

        public void SetStopwatch( Stopwatch stopwatch )
        {
            _stopwatch = stopwatch;
        }

        virtual public void Update()
        {
            // Invoke Update for all active events
            foreach( EventType activeEvent in _activeEvents.Values )
                activeEvent.Update();
        }

        virtual public void Reset()
        {
            // Only reset the event metrics, not currently active events
            _events.Clear();
            _incrementalNode = null;
        }

        virtual public void IncrementalReset()
        {
            _incrementalNode = _events.Last;
        }

        virtual public void Clear()
        {
            // Clear all state, including active events
            _events.Clear();
            _activeEvents.Clear();
            _incrementalNode = null;       
        }

        virtual public void Start( string id, params string[] tags )
        {
			// Ignore events that have already been started
			if ( _activeEvents.ContainsKey( id ) ) return;              

            EventType newEvent = NewEvent( id );

            newEvent.AddTags( tags );

            _activeEvents.Add( newEvent.Id, newEvent );
        }

        virtual public void End( string id, params string[] additional_tags )
        {
            EventType activeEvent;
			// Ignore events that have not been started or have already been closed
			if ( !_activeEvents.TryGetValue( id, out activeEvent ) ) return;          

            activeEvent.Duration = _stopwatch.ElapsedMilliseconds - activeEvent.Timestamp ; 

            activeEvent.AddTags( additional_tags );

            // Deactivate event
            _activeEvents.Remove( activeEvent.Id );
            _events.AddLast( activeEvent );
        }

        virtual public void AddMeta( string id, params string[] meta )
        {
            EventType activeEvent;
			// Ignore events that have not been started or have already been closed
			if ( !_activeEvents.TryGetValue( id, out activeEvent ) ) return;          
            activeEvent.AddMeta( meta );
        }

        // Workaround for inability to use new() constraint with ctor params
        virtual protected EventType NewEvent( string id )
        {
            return null;
        }

        virtual public List<EventType> GetEvents()
        {
			// Return a copy of the list, as the original list may be Clear()ed
			return new List<EventType>( _events );
        }

        virtual public List<ResultType> GetResults()
        {
			return GetResults( _events.First );
        }

        /// <summary>
        /// Returns incremental results since last IncrementalReset()
        /// </summary>
        /// <returns>The incremental results.</returns>
        virtual public List<ResultType> GetIncrementalResults()
        {
            // Return all events if we have no incremental node, 
            // else all events past the last set _incrementalNode
            return _incrementalNode == null ? GetResults() : GetResults( _incrementalNode.Next );
        }

        virtual protected List<ResultType> GetResults( LinkedListNode<EventType> firstNode )
        {
            List<ResultType> results = new List<ResultType>();
            LinkedListNode<EventType> node = firstNode;
            while ( node != null ) {
                results.Add( node.Value.ToResult() ); 
                node = node.Next;
            }

            return results;
        }
    }

    public class TimerMetrics : Metrics<TimerEvent, TimerEventResult>
    {
        protected ushort _depth;    

        override public void Clear()
        {
            base.Clear();
            _depth = 0;
        }

        override public void Start( string id, params string[] tags )
        {
			// Ignore events that have already been started
			if ( _activeEvents.ContainsKey( id ) ) return;
    
            TimerEvent startEvent = new TimerEvent(
                id,
                _stopwatch.ElapsedMilliseconds,
                _depth++
            );

            startEvent.AddTags( tags );
    
            _activeEvents.Add( startEvent.Id, startEvent );
            _events.AddLast( startEvent );
        }
    
        override public void End( string id, params string[] additional_tags )
        {
            TimerEvent startEvent;
			// Ignore events that have not been started or have already been closed
			if ( !_activeEvents.TryGetValue( id, out startEvent ) ) return;
    
            long timestamp = _stopwatch.ElapsedMilliseconds;
    
            TimerEvent endEvent = new TimerEvent(
                startEvent.Id,
                _stopwatch.ElapsedMilliseconds,
                --_depth,
                timestamp - startEvent.Timestamp,
                true
            );

            // End event inherits all tags from the start event
            endEvent.AddTags( startEvent.Tags );
            endEvent.AddTags( additional_tags );
            

            // Remove to allow another event with same id, if desired
            _activeEvents.Remove( endEvent.Id );
            _events.AddLast( endEvent );
        }

		/// <summary>
		/// Returns incremental results since last IncrementalReset()
		/// For timer results, we only return end results since they contain total duration data
		/// </summary>
		/// <returns>The incremental results.</returns>
		override public List<TimerEventResult> GetIncrementalResults()
		{
			List<TimerEventResult> results = new List<TimerEventResult>();

			// Return all events if we have no incremental node, 
			// else all events past the last set _incrementalNode
			LinkedListNode<TimerEvent> node = _incrementalNode != null ? _incrementalNode.Next : _events.First;
			while ( node != null ) {
				TimerEvent timerEvent = node.Value;
				// Only return End events
				if ( timerEvent.EndEvent )
					results.Add( timerEvent.ToResult() ); 
				
				node = node.Next;
			}
			
			return results;
		}
    }

    public class FPSMetrics : Metrics<FPSEvent, FPSEventResult>
    {
        override protected FPSEvent NewEvent( string id )
        {
            return new FPSEvent( id, _stopwatch.ElapsedMilliseconds );
        }
    }

    public class MemoryMetrics : Metrics<MemoryEvent, MemoryEventResult>
    {
        override protected MemoryEvent NewEvent( string id )
        {
            return new MemoryEvent( id, _stopwatch.ElapsedMilliseconds );
        }
    }

	public class CounterMetrics : Metrics<CounterEvent, CounterEventResult>
	{
		// Maintains all events, even after incremental reset, to enable full report generation
		protected Dictionary<string, CounterEvent> _allEvents;
		
		public CounterMetrics() : base()
		{
			_allEvents = new Dictionary<string, CounterEvent>();			
		}

		override public void Reset()
		{
			// Only reset the event metrics, not currently active events
			_allEvents.Clear();
		}

		override public void IncrementalReset()
		{
			// CounterMetrics uses _activeEvents to track ongoing count totals
			_activeEvents.Clear();
		}

		override public void Update() {}
		override public void Start( string id, params string[] tags ) {}
		override public void End( string id, params string[] tags ) {}

		virtual public void Count( string id, uint value, params string[] tags )
		{
			// Increment counter for events recorded since last incremental reset
			Increment( _activeEvents, id, value, tags );
			// Increment global counter for overall report generation
			Increment( _allEvents, id, value, tags );
		}

		virtual protected void Increment( Dictionary<string, CounterEvent> target, string id, uint value, params string[] tags )
		{
			CounterEvent counterEvent; 
			if ( target.TryGetValue( id, out counterEvent ) ) {
				counterEvent.Increment( value );
			} else {
				counterEvent = new CounterEvent( id, value );
				target.Add( id, counterEvent );
			}			
		}

		override public List<CounterEventResult> GetIncrementalResults()
		{
			return GetResults( _activeEvents );
		}

		override public List<CounterEventResult> GetResults()
		{
			return GetResults( _allEvents );
		}

		virtual protected List<CounterEventResult> GetResults( Dictionary<string, CounterEvent> target )
		{
			List<CounterEventResult> result = new List<CounterEventResult>( _activeEvents.Count );
			foreach( CounterEvent counterEvent in target.Values )
				result.Add( counterEvent.ToResult() );
			return result;
		}		
	}

	public class ValueMetrics : Metrics<ValueEvent, ValueEventResult>
	{
		public ValueMetrics() : base() {}
		
		override public void Update() {}
		override public void Start( string id, params string[] tags ) {}
		override public void End( string id, params string[] tags ) {}
		
		virtual public void Value( string id, int value, params string[] meta )
		{
			ValueEvent valueEvent = new ValueEvent( id, value );
			valueEvent.AddMeta( meta );
			_events.AddLast( valueEvent );
			this.Log("Added value metric! Id = " + valueEvent.Id + ", Value = " + value);
		}
	}

    abstract public class MetricsEvent<T> : ILoggableMetrics
    {
        protected static readonly char INDENT_SPACER = '\t';

        protected string _id;
        public string Id {
            get { return _id; }
        }

        protected long _timestamp;
        public long Timestamp {
            get { return _timestamp; }
        }

        protected long _duration;
        public long Duration {
            get { return _duration; }
            set { _duration = value; }
        }

        protected List<string> _meta = new List<string>();
        public List<string> Meta {
            get { return _meta; }            
        }

        protected List<string> _tags = new List<string>();
		public List<string> Tags {
            get { return _tags; }
        }

        public MetricsEvent( string id, long timestamp = 0 )
        {       
            _id = id;
            _timestamp = timestamp;
        } 

        public void AddMeta( params string[] meta )
        {
            if ( meta == null ) return;
            _meta.AddRange( meta );
        }

        public void AddTags( IEnumerable<string> tags )
        {
            if ( tags == null ) return;
            _tags.AddRange( tags );
        }

        virtual public void Update() {}

        abstract public T ToResult();

        protected int DoubleToInt( double value ) {
            return Convert.ToInt32( Math.Round(  value ) );
        }
    }

	abstract public class EventResult {
		public string[] Meta;
		[JsonIgnoreAttribute] public List<string> Tags;
	}

	public class TimerEventResult : EventResult
	{
		public string Id;
		public long Duration;
		[JsonIgnoreAttribute] public long Timestamp;		
		[JsonIgnoreAttribute] public ushort Depth;		
		[JsonIgnoreAttribute] public bool EndEvent;
	}

    public class TimerEvent : MetricsEvent<TimerEventResult>
    {
        protected readonly ushort _depth;
        public ushort Depth {
            get { return _depth; }
        }

        protected readonly bool _endEvent;
        public bool EndEvent {
            get { return _endEvent; }
        }

        public TimerEvent( string id, long timestamp, ushort depth, long duration = 0, bool endEvent = false ) : base( id, timestamp )
        {               
            _depth = depth;
            _duration = duration;
            _endEvent = endEvent;
        }

        override public TimerEventResult ToResult() {
            return new TimerEventResult {
                Id = Id,
                Meta = _meta.ToArray(),
                Tags = _tags,
                Timestamp = _timestamp,
                Duration = _duration,
                Depth = _depth,
                EndEvent = _endEvent
            };
        }
    }

	public class FPSEventResult : EventResult
	{
		public string Id;
		public int AvgFPS;
		public int MaxFPS;
		public int MinFPS;
		public int FrameCount;
		public long Timestamp;
		public long Duration;
	}

    public class FPSEvent : MetricsEvent<FPSEventResult>
    {
        // Sets the number of samples used for smoothing
        // FPS values via moving average
        protected static readonly int MAX_SAMPLE_COUNT = 30;
        protected int sampleCount;
        protected int bufferIndex;
        protected double bufferSum;
        protected double[] buffer = new double[MAX_SAMPLE_COUNT];

        protected int _frameCount;
        protected double _accumulatedFPS;
        protected double _currentRawFPS;

        protected double _currentFPS;
        public double CurrentFPS {
            // Returns current moving average FPS
            get { return _currentFPS; }
        }

        protected double _minFPS;
        public double MinFPS {
            get { return _minFPS; }
        }

        protected double _maxFPS;
        public double MaxFPS {
            get { return _maxFPS; }
        }

        public double AvgFPS {
            // Returns average FPS, an average of all moving average data points
            get { return _frameCount > 0 ? _accumulatedFPS / _frameCount : 0; }
        }

        public FPSEvent( string id, long timestamp ) : base( id, timestamp ) {}

        override public void Update() {
            ++_frameCount;

            // This value can be very different from Application.targetFrameRate. 
            // Some frames are much shorter and some are much longer, so we feed
            // this value to a moving average algorithm to smooth out output.
            _currentRawFPS = Time.timeScale / Time.deltaTime;
            _currentFPS = MovingAverage( _currentRawFPS );

            // This is the standard path, once we have collected enough
            // samples to track min/max FPS based upon our moving average
            if ( _frameCount > MAX_SAMPLE_COUNT ) {
                if ( _currentFPS < _minFPS  ) _minFPS = _currentFPS;
                if ( _currentFPS > _maxFPS ) _maxFPS = _currentFPS;
            } else if ( _frameCount == MAX_SAMPLE_COUNT ) {
                // Once we have fully populated our buffer, we 
                // initialize min/max FPS to the current frame rate.
                // Until this point, MinFPS and MaxFPS will return zero.
                // We may wish to return the _currentFPS instead of zero,
                // or some other value. zero indicates "no data".
                _minFPS = _maxFPS = _currentFPS;
            }  

            // We accumulate FPS values each frame for an overall 
            // average, returned by the AvgFPS property
            _accumulatedFPS += _currentFPS;
        }

        protected double MovingAverage( double newValue ) {
            if ( sampleCount < MAX_SAMPLE_COUNT ) ++sampleCount;
            // Subtract from sum the value that will soon be overwritten
            bufferSum -= buffer[bufferIndex];
            // Add the current value to the sum
            bufferSum += newValue;
            // Save currentValue into the current buffer index
            buffer[bufferIndex] = newValue;
            // Loop around ring buffer
            if ( ++bufferIndex >= sampleCount ) bufferIndex =  0;

            return bufferSum / sampleCount;
        }

        override public FPSEventResult ToResult() {
            return new FPSEventResult {
                Id = Id,
                Tags = _tags,
                Meta = _meta.ToArray(),
                AvgFPS = DoubleToInt ( AvgFPS ),
                MaxFPS = DoubleToInt ( MaxFPS ),
                MinFPS = DoubleToInt ( MinFPS ),
                FrameCount = _frameCount,
                Timestamp = _timestamp,
                Duration = _duration
            };
        }
    }

	/// <summary>
	/// FPS metric event result. Simplified version of above class
	/// without inheritance, as JsonFx does not properly serialize
	/// fields in subclasses
	/// </summary>
	public class MemoryEventResult : EventResult
	{
		public string Id;
		public int AvgUsage;
		public int MaxUsage;
		public int MinUsage;
		public int SampleCount;
		public long Timestamp;
		public long Duration;
	}

    public class MemoryEvent : MetricsEvent<MemoryEventResult>
    {
        protected static readonly int SAMPLE_FRAME_INTERVAL = 90;
        protected static readonly int ONE_MB = 1024*1024;

        protected int _frameCount;
        protected int _sampleCount;        

        protected long _accumulatedUsage;

        public double AvgUsage {
            // Returns average memory usage during this event
            get { return _sampleCount > 0 ? (double)_accumulatedUsage / ( _sampleCount * ONE_MB ): 0; }
        }

        protected long _maxUsage;
        public double MaxUsage {
            get { return (double)_maxUsage/ONE_MB; }
        }

        protected long _minUsage;
        public double MinUsage {
            get { return (double)_minUsage/ONE_MB; }
        }

        public MemoryEvent( string id, long timestamp ) : base( id, timestamp ) {
            // Set initial min/max values
            _minUsage = _maxUsage = GetCurrentMemoryUsage();
        }

        override public void Update() {
            if ( ++_frameCount < SAMPLE_FRAME_INTERVAL ) return;
            // Reset interval counter
            _frameCount = 0;

            int usage = GetCurrentMemoryUsage();

            _accumulatedUsage += usage;
            ++_sampleCount;

            if ( usage < _minUsage ) {
                _minUsage = usage;
            } else if ( usage > _maxUsage ) {
                _maxUsage = usage;
            }
        }

        override public MemoryEventResult ToResult() {
            return new MemoryEventResult {
                Id = Id,
                Meta = _meta.ToArray(),
                Tags = _tags,
                AvgUsage = DoubleToInt ( AvgUsage ),
                MaxUsage = DoubleToInt ( MaxUsage ),
                MinUsage = DoubleToInt ( MinUsage ),
                SampleCount = _sampleCount,
                Timestamp = _timestamp,
                Duration = _duration,
            };
        }

        protected int GetCurrentMemoryUsage()
        {
            return MemoryUtils.GetMemoryInfo().resident_size;
        }
    }

	public class CounterEventResult : EventResult
	{
		public string Id;
		public uint Count;		
	}
	
	public class CounterEvent : MetricsEvent<CounterEventResult>
	{		
		private uint _count;

		public CounterEvent( string id, uint value = 1 ) : base( id ) {
			_count = value;
		}
		
		public void Increment( uint value = 1)
		{
			_count += value;
		}

		override public CounterEventResult ToResult() {
			return new CounterEventResult {
				Id = Id,
				Meta = _meta.ToArray(),
				Tags = _tags,
				Count = _count
			};
		}
	}

	public class ValueEventResult : EventResult
	{
		public string Id;
		public int Value;		
	}
	
	public class ValueEvent : MetricsEvent<ValueEventResult>
	{		
		private int _value;
		
		public ValueEvent( string id, int value = 0 ) : base( id ) {
			_value = value;
		}
		
		override public ValueEventResult ToResult() {
			return new ValueEventResult {
				Id = Id,
				Meta = _meta.ToArray(),
				Tags = _tags,
				Value = _value
			};
		}
	}

	#if INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS
    abstract public class MetricsReport<MetricsType, EventResultType> : ILoggableMetrics where MetricsType : IMetricsResults<EventResultType>
    {        
        protected static readonly string DEFAULT_REPORT_FILE_SUFFIX = ".txt";
        protected static readonly char INDENT_SPACER = '\t';
        protected static readonly string DATA_SEPARATOR = ",";
        protected static readonly string CSV_DATA_SEPARATOR = ";";

        protected readonly MetricsType _metrics;
        protected readonly string _reportPath;
        protected readonly string _reportFilename;

        public MetricsReport( MetricsType metrics, string reportPath = null, string reportFilename = null ) {
            _metrics = metrics;
            _reportPath = reportPath ?? UnityEngine.Application.persistentDataPath;
            _reportFilename = reportFilename ?? GetType().FullName + DEFAULT_REPORT_FILE_SUFFIX;
        }

        virtual protected string BuildReport() {
            StringBuilder sb = new StringBuilder();

            string header = GetHeader();
            string footer = GetFooter();

            if ( !String.IsNullOrEmpty( header ) ) sb.AppendLine( header );
			
			foreach( EventResultType result in _metrics.GetResults() ) {
				string resultLine = FormatResult( result );
				if ( !String.IsNullOrEmpty( resultLine ) ) sb.AppendLine( resultLine );
            }

            if ( !String.IsNullOrEmpty( footer ) ) sb.AppendLine( footer );

            return sb.ToString();
        }

        virtual public void Save() {
            string targetPath = Path.Combine( _reportPath, _reportFilename );
            string reportText = BuildReport();
            // We write the report to disk even if it is empty, else we may leave confusing reports from
            // a previous invocation if there is not yet any data during this session
            WriteReport( targetPath, reportText );
        }

        virtual protected void WriteReport( string targetPath, string reportText ) {
            try {
                File.WriteAllText( targetPath, reportText );
                this.Log("Metrics report written to file '" + targetPath + "' ( " + reportText .Length + " bytes )");
            } catch ( Exception e) {
                this.LogError( "Exception writing metrics report to file '" + targetPath + "': " + e.ToString() );
            }
        }

        virtual protected string GetHeader() { return string.Empty; }
        virtual protected string FormatResult( EventResultType result ) { return string.Empty; }
        virtual protected string GetFooter() { return string.Empty; }

    }

    abstract public class TagMetricsReport<MetricsType, EventResultType, TagResultType> : MetricsReport<MetricsType, EventResultType> where MetricsType : IMetricsResults<EventResultType> where EventResultType : EventResult
    {
        public TagMetricsReport( MetricsType metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        abstract protected IEnumerable<TagResultType> GetTagResults();
        abstract protected string FormatTagResult( TagResultType result );

        override protected string BuildReport() {                        
            StringBuilder sb = new StringBuilder();

            string header = GetHeader();
            string footer = GetFooter();

            if ( !String.IsNullOrEmpty( header ) ) sb.AppendLine( header );
    
            foreach( TagResultType result in GetTagResults() ) {
                string resultLine = FormatTagResult( result );
                if ( !String.IsNullOrEmpty( resultLine ) ) sb.AppendLine( resultLine );
            }
    
            if ( !String.IsNullOrEmpty( footer ) ) sb.AppendLine( footer );

            return sb.ToString();
        }

        virtual protected IDictionary<string, List<EventResultType>> BuildTagEventResultsMap( List<EventResultType> eventResults, Predicate<EventResultType> filter = null ) 
        {
            Dictionary<string, List<EventResultType>> tagEventResultsMap = new Dictionary<string, List<EventResultType>>();

            // Apply filter, if any            
            if ( filter != null )
                eventResults = eventResults.FindAll( filter );

            foreach( EventResultType eventResult in eventResults ) {
				List<string> tags = eventResult.Tags;
                if ( tags.Count <= 0 ) continue;

                foreach( string tag in tags ) {
                    List<EventResultType> tagResults;
                    if ( tagEventResultsMap.TryGetValue( tag, out tagResults ) ) {
                        tagResults.Add( eventResult );
                    } else {
                        tagEventResultsMap.Add( tag, new List<EventResultType> { eventResult } );
                    }
                }
            }

            return tagEventResultsMap;
        }
    }  

	public class XMLTimerReport : MetricsReport<TimerMetrics, TimerEventResult>
	{
		protected static readonly string FORMAT_TEMPLATE_START =        "{0}<{1} timestamp=\"{2}\"{3}{4}>";        
		protected static readonly string FORMAT_TEMPLATE_END =          "{0}</{1} timestamp=\"{2}\" duration=\"{3}\"{4}{5}>";
		
		public XMLTimerReport( TimerMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string FormatResult( TimerEventResult result ) {
			
			string meta = ( ( result.Meta != null ) && ( result.Meta.Length > 0 ) ) ? String.Format(" meta=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Meta ) ) : string.Empty;
			
			string tags = ( ( result.Tags != null ) && ( result.Tags.Count > 0 ) ) ? String.Format(" tags=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Tags.ToArray() ) ) : string.Empty;
			
			
			return result.EndEvent ?
				String.Format( FORMAT_TEMPLATE_END,
				              new String(INDENT_SPACER, result.Depth ), result.Id, result.Timestamp, result.Duration, meta, tags ) :
					String.Format( FORMAT_TEMPLATE_START,
					              new String(INDENT_SPACER, result.Depth ), 
					              result.Id,
					              result.Timestamp,
					              meta,
					              tags );                
		}
	}
	
	public class CSVTimerReport : MetricsReport<TimerMetrics, TimerEventResult>
	{
		public CSVTimerReport( TimerMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string GetHeader() {
			return "Id,Timestamp,Duration,Depth,Meta";
		}
		
		override protected string FormatResult( TimerEventResult result ) {
			// We drop start lines in the timer CSV output, to allow meaningful sorting
			if ( !result.EndEvent ) return null;
			
			return String.Format( "{0},{1},{2},{3},{4}",
			                     result.Id,
			                     result.Timestamp,                
			                     result.Duration,
			                     result.Depth,
			                     ( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) ? string.Empty : String.Join( CSV_DATA_SEPARATOR, result.Meta )
			                     );
		}
	}	

    abstract public class TimerTagReport : TagMetricsReport<TimerMetrics, TimerEventResult, TimerTagResult> 
    {
        public TimerTagReport( TimerMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
    
        override protected IEnumerable<TimerTagResult> GetTagResults()
        {
            List<TimerTagResult> tagResults = new List<TimerTagResult>();

            // Scan only end events
            foreach( KeyValuePair<string, List<TimerEventResult>> kvp in BuildTagEventResultsMap( _metrics.GetResults(), (eventResult) => eventResult.EndEvent ) ) {
                string tag = kvp.Key;
                List<TimerEventResult> eventResults = kvp.Value;
                if ( eventResults.Count <= 0 ) continue;
                
                List<TimeRange> timeRanges = EventResultsToTimeRanges( eventResults );
                List<TimeRange> unionTimeRanges = RangeUnion( timeRanges );

				// Calculate parallel duration of even by simply adding all individual event time ranges disregarding possible parallel execution
				long parallelDuration = 0;
                foreach( TimeRange range in timeRanges ) 
					parallelDuration += range.Duration;

				// Calculate duration of event in linear time by combining overlapping/parallel events
                long linearDuration = 0;
                foreach( TimeRange range in unionTimeRanges ) 
                    linearDuration += range.Duration;

                tagResults.Add( new TimerTagResult { Tag = tag, EventCount = eventResults.Count, ParallelDuration = parallelDuration, LinearDuration = linearDuration } );
            }

            // Sort descending, so results with longest duration are listed first
            tagResults.Sort( (x, y) => { return -1*x.LinearDuration.CompareTo( y.LinearDuration ); } );

            return tagResults;
        }

        protected class TimeRange {        
            public readonly long Start;
            public readonly long End;
            public readonly long Duration;

            public TimeRange( long start, long end ) {
                Start = start;
                End = end;
                Duration = End - Start;
            }           
        }

        protected List<TimeRange> EventResultsToTimeRanges( IEnumerable<TimerEventResult> eventResults )
        {
            List<TimeRange> result = new List<TimeRange>();
            foreach( TimerEventResult eventResult in eventResults )
                result.Add( new TimeRange( eventResult.Timestamp, eventResult.Timestamp+eventResult.Duration ) );
            return result;
        }

        /// <summary>
        /// Takes the union of TimeRanges, returning a collection of TimeRanges
        /// whose endpoints are all mutually exclusive.
        /// </summary>
        /// <returns>The ranges.</returns>
        /// <param name="target">Target.</param>
        protected List<TimeRange> RangeUnion( List<TimeRange> target )
        {
            // Collapse only meaningful against at least two elements
            if ( ( target == null ) || target.Count <= 1 ) return target;

            List<TimeRange> result = new List<TimeRange>();

            // Sort ranges by start time
            target.Sort( (x, y) => { return x.Start.CompareTo( y.Start ); } );
    
            // Bounds for the current collapsed range
            long start_collapsed = 0;
            long end_collapsed = 0;            
    
            foreach ( TimeRange range in target ) {
                // Initialize bounds to start/end of first range
                if ( ( start_collapsed <= 0 ) && ( end_collapsed <= 0 ) ) {
                    start_collapsed = range.Start;
                    end_collapsed = range.End;
                    continue;
                }

                // We have found a gap, so close the previous range 
                // and commit it to the results list
                if ( range.Start > end_collapsed ) {
                    result.Add( new TimeRange ( start_collapsed, end_collapsed ) );
                    // Initialize new range
                    start_collapsed = range.Start;
                }

                // Extend current range, or initialize endpoint of new range
                if ( range.End > end_collapsed ) end_collapsed = range.End;
            }

            // Add terminating range for last start_collapsed/end_collapsed pair
            result.Add( new TimeRange( start_collapsed, end_collapsed ) );
    
            return result;
        }  
    }

    public class TimerTagResult {
        public string Tag;
        public int EventCount;
        public long LinearDuration;
		public long ParallelDuration;
    }

    public class XMLTimerTagReport : TimerTagReport 
    {
        public XMLTimerTagReport( TimerMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string FormatTagResult( TimerTagResult result ) {
			return string.Format( "<{0} event-count=\"{1}\" linear-duration=\"{2}\" parallel-duration=\"{3}\" />",
                result.Tag,
                result.EventCount,
                result.LinearDuration,
                result.ParallelDuration
            );
        }
    }

    public class CSVTimerTagReport : TimerTagReport 
    {
        public CSVTimerTagReport( TimerMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string GetHeader() {
            return "Tag,EventCount,TotalDuration";
        }

        override protected string FormatTagResult( TimerTagResult result ) {
			return string.Format(  "{0},{1},{2},{3}",
                result.Tag,
                result.EventCount,
                result.LinearDuration,
                result.ParallelDuration
            );
        }
    }

	
	public class XMLFPSReport : MetricsReport<FPSMetrics, FPSEventResult>
	{
		protected static readonly string FORMAT_TEMPLATE =          "<{0} timestamp=\"{1}\" duration=\"{2}\" avg-fps=\"{3}\" min-fps=\"{4}\" max-fps=\"{5}\" />";
		protected static readonly string FORMAT_TEMPLATE_META =     "<{0} timestamp=\"{1}\" duration=\"{2}\" avg-fps=\"{3}\" min-fps=\"{4}\" max-fps=\"{5}\" meta=\"{6}\"/>";
		
		public XMLFPSReport( FPSMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string FormatResult( FPSEventResult result ) {
			
			if ( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) return String.Format( FORMAT_TEMPLATE,
			                                                                                   result.Id,
			                                                                                   result.Timestamp,
			                                                                                   result.Duration,
			                                                                                   result.AvgFPS,
			                                                                                   result.MinFPS,
			                                                                                   result.MaxFPS
			                                                                                   );
			
			return String.Format( FORMAT_TEMPLATE_META,
			                     result.Id,
			                     result.Timestamp,
			                     result.Duration,
			                     result.AvgFPS,
			                     result.MinFPS,
			                     result.MaxFPS,
			                     String.Join( DATA_SEPARATOR, result.Meta )
			                     );
			
		}
	}
	
	public class CSVFPSReport : MetricsReport<FPSMetrics, FPSEventResult>
	{
		public CSVFPSReport( FPSMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string GetHeader() {
			return "Id,Timestamp,Duration,AvgFPS,MinFPS,MaxFPS,Meta";
		}
		
		override protected string FormatResult( FPSEventResult result ) {
			return String.Format( "{0},{1},{2},{3},{4},{5},{6}",
			                     result.Id,
			                     result.Timestamp,
			                     result.Duration,
			                     result.AvgFPS,
			                     result.MinFPS,
			                     result.MaxFPS,
			                     ( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) ? string.Empty : String.Join( CSV_DATA_SEPARATOR, result.Meta )
			                     );
		}
	}

    abstract public class FPSTagReport : TagMetricsReport<FPSMetrics, FPSEventResult, FPSTagResult> 
    {
        public FPSTagReport( FPSMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
    
        override protected IEnumerable<FPSTagResult> GetTagResults()
        {
            List<FPSTagResult> tagResults = new List<FPSTagResult>();

            foreach( KeyValuePair<string, List<FPSEventResult>> kvp in BuildTagEventResultsMap( _metrics.GetResults() ) )
            {
                string tag = kvp.Key;
                List<FPSEventResult> eventResults = kvp.Value;
                if ( eventResults.Count <= 0 ) continue;

                int minFPS = 0;
                int maxFPS = 0;
                int totalFrameCount = 0;
                long accumulatedAvgFPS = 0; 

                foreach( FPSEventResult eventResult in eventResults ) {
                    // Initialize min/max FPS
                    if (( minFPS <= 0 ) && ( maxFPS <= 0 )) {
                        minFPS = eventResult.MinFPS;
                        maxFPS = eventResult.MaxFPS;
                    }

                    totalFrameCount += eventResult.FrameCount;

                    // Dirty weighting of AvgFPS by framecount, for dirty overall "average" FPS
                    accumulatedAvgFPS += ( eventResult.AvgFPS * eventResult.FrameCount );

                    if ( eventResult.MinFPS < minFPS ) minFPS = eventResult.MinFPS;
                    if ( eventResult.MaxFPS > maxFPS ) maxFPS = eventResult.MaxFPS;
                }

                int avgFPS = totalFrameCount > 0 ? Convert.ToInt32( Math.Round( (double)accumulatedAvgFPS / totalFrameCount ) ) : 0;

                tagResults.Add( new FPSTagResult {
                    Tag = tag,
                    EventCount = eventResults.Count,
                    AvgFPS = avgFPS,
                    MaxFPS = maxFPS,
                    MinFPS = minFPS,
                    TotalFrameCount = totalFrameCount
                } );
            }

            // Sort ascending, so results with lowest FPS are listed first
            tagResults.Sort( (x, y) => { return x.MinFPS.CompareTo( y.MinFPS ); } );

            return tagResults;
        }
    }

    public class FPSTagResult {
        public string Tag;
        public int EventCount;
        public int AvgFPS;
        public int MaxFPS;
        public int MinFPS;
        public int TotalFrameCount;          
    }

    public class XMLFPSTagReport : FPSTagReport 
    {
        public XMLFPSTagReport( FPSMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string FormatTagResult( FPSTagResult result ) {
            return string.Format( "<{0} event-count=\"{1}\" avg-fps=\"{2}\" max-fps=\"{3}\" min-fps=\"{4}\" total-frame-count=\"{5}\" />",
                result.Tag,
                result.EventCount,
                result.AvgFPS,
                result.MaxFPS,
                result.MinFPS,
                result.TotalFrameCount
            );
        }
    }

    public class CSVFPSTagReport : FPSTagReport 
    {
        public CSVFPSTagReport( FPSMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string GetHeader() {
            return "Tag,EventCount,AvgFPS,MaxFPS,MinFPS,TotalFrameCount";
        }

        override protected string FormatTagResult( FPSTagResult result ) {
            return string.Format( "{0},{1},{2},{3},{4},{5}",
                result.Tag,
                result.EventCount,
                result.AvgFPS,
                result.MaxFPS,
                result.MinFPS,
                result.TotalFrameCount
            );
        }
    }

	public class XMLMemoryReport : MetricsReport<MemoryMetrics, MemoryEventResult>
	{
		protected static readonly string FORMAT_TEMPLATE =          "<{0} timestamp=\"{1}\" duration=\"{2}\" avg-usage=\"{3}\" max-usage=\"{4}\" min-usage=\"{5}\" />";
		protected static readonly string FORMAT_TEMPLATE_META =     "<{0} timestamp=\"{1}\" duration=\"{2}\" avg-usage=\"{3}\" max-usage=\"{4}\" min-usage=\"{5}\" meta=\"{6}\"/>";
		
		public XMLMemoryReport( MemoryMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string FormatResult( MemoryEventResult result ) {
			if ( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) return String.Format( FORMAT_TEMPLATE,
			                                                                                   result.Id,
			                                                                                   result.Timestamp,
			                                                                                   result.Duration, 
			                                                                                   result.AvgUsage,
			                                                                                   result.MaxUsage,
			                                                                                   result.MinUsage
			                                                                                   );
			
			return String.Format( FORMAT_TEMPLATE_META,
			                     result.Id,
			                     result.Timestamp,
			                     result.Duration, 
			                     result.AvgUsage,
			                     result.MaxUsage,
			                     result.MinUsage,
			                     String.Join( DATA_SEPARATOR, result.Meta )
			                     );
		}
	}
	
	public class CSVMemoryReport : MetricsReport<MemoryMetrics, MemoryEventResult>
	{
		public CSVMemoryReport( MemoryMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string GetHeader() {
			return "Id,Timestamp,Duration,AvgUsage,MaxUsage,MinUsage,Meta";
		}
		
		override protected string FormatResult( MemoryEventResult result ) {
			return String.Format( "{0},{1},{2},{3},{4},{5},{6}",
			                     result.Id,
			                     result.Timestamp,
			                     result.Duration, 
			                     result.AvgUsage,
			                     result.MaxUsage,
			                     result.MinUsage,
			                     ( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) ? string.Empty : String.Join( CSV_DATA_SEPARATOR, result.Meta )
			                     );
		}
	}

    abstract public class MemoryTagReport : TagMetricsReport<MemoryMetrics, MemoryEventResult, MemoryTagResult> 
    {
        public MemoryTagReport( MemoryMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
    
        override protected IEnumerable<MemoryTagResult> GetTagResults()
        {
            List<MemoryTagResult> tagResults = new List<MemoryTagResult>();

            foreach( KeyValuePair<string, List<MemoryEventResult>> kvp in BuildTagEventResultsMap( _metrics.GetResults() ) )
            {
                string tag = kvp.Key;
                List<MemoryEventResult> eventResults = kvp.Value;
                if ( eventResults.Count <= 0 ) continue;

                int minUsage = 0;
                int maxUsage = 0;
                int totalSampleCount = 0;
                long accumulatedAvgUsage = 0; 

                foreach( MemoryEventResult eventResult in eventResults ) {
                    // Initialize min/max usage
                    if (( minUsage <= 0 ) && ( maxUsage <= 0 )) {
                        minUsage = eventResult.MinUsage;
                        maxUsage = eventResult.MaxUsage;
                    }

                    totalSampleCount += eventResult.SampleCount;

                    // Dirty weighting of AvgUsage by sample count, for dirty overall "average" usage
                    accumulatedAvgUsage += ( eventResult.AvgUsage * eventResult.SampleCount );

                    if ( eventResult.MinUsage < minUsage ) minUsage = eventResult.MinUsage;
                    if ( eventResult.MaxUsage > maxUsage ) maxUsage = eventResult.MaxUsage;
                }

                int avgUsage = totalSampleCount > 0 ? Convert.ToInt32( Math.Round( (double)accumulatedAvgUsage / totalSampleCount ) ) : 0;

                tagResults.Add( new MemoryTagResult {
                    Tag = tag,
                    EventCount = eventResults.Count,
                    AvgUsage = avgUsage,
                    MaxUsage = maxUsage,
                    MinUsage = minUsage,
                    TotalSampleCount = totalSampleCount
                } );
            }

            // Sort descending, so results with higest max memory usage are listed first
            tagResults.Sort( (x, y) => { return x.MaxUsage.CompareTo( y.MaxUsage ); } );

            return tagResults;
        }
    }

    public class MemoryTagResult {
        public string Tag;
        public int EventCount;
        public int AvgUsage;
        public int MaxUsage;
        public int MinUsage;
        public int TotalSampleCount;
    }

    public class XMLMemoryTagReport : MemoryTagReport 
    {
        public XMLMemoryTagReport( MemoryMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string FormatTagResult( MemoryTagResult result ) {
            return string.Format( "<{0} event-count=\"{1}\" avg-usage=\"{2}\" max-usage=\"{3}\" min-usage=\"{4}\" total-sample-count=\"{5}\" />",
                result.Tag,
                result.EventCount,
                result.AvgUsage,
                result.MaxUsage,
                result.MinUsage,
                result.TotalSampleCount
            );
        }
    }

    public class CSVMemoryTagReport : MemoryTagReport 
    {
        public CSVMemoryTagReport( MemoryMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}

        override protected string GetHeader() {
            return "Tag,EventCount,AvgUsage,MaxUsage,MinUsage,TotalSampleCount";
        }

        override protected string FormatTagResult( MemoryTagResult result ) {
            return string.Format( "{0},{1},{2},{3},{4},{5}",
                result.Tag,
                result.EventCount,
                result.AvgUsage,
                result.MaxUsage,
                result.MinUsage,
                result.TotalSampleCount
            );
        }
    }

	public class XMLCounterReport : MetricsReport<CounterMetrics, CounterEventResult>
	{
		protected static readonly string FORMAT_TEMPLATE = "<{0} count=\"{1}\"{2}{3}>";        
		
		public XMLCounterReport( CounterMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string FormatResult( CounterEventResult result )
		{			
			string meta = ( ( result.Meta != null ) && ( result.Meta.Length > 0 ) ) ? String.Format(" meta=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Meta ) ) : string.Empty;
			string tags = ( ( result.Tags != null ) && ( result.Tags.Count > 0 ) ) ? String.Format(" tags=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Tags.ToArray() ) ) : string.Empty;						
			return String.Format( FORMAT_TEMPLATE, result.Id, result.Count, meta, tags );           
		}
	}
	
	public class CSVCounterReport : MetricsReport<CounterMetrics, CounterEventResult>
	{
		public CSVCounterReport( CounterMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string GetHeader() {
			return "Id,Count,Meta";
		}
		
		override protected string FormatResult( CounterEventResult result ) {
			return String.Format( "{0},{1},{2}",
				result.Id,
				result.Count,
				( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) ? string.Empty : String.Join( CSV_DATA_SEPARATOR, result.Meta )
			);
		}
	}

	public class XMLValueReport : MetricsReport<ValueMetrics, ValueEventResult>
	{
		protected static readonly string FORMAT_TEMPLATE = "<{0} value=\"{1}\"{2}{3}>";        
		
		public XMLValueReport( ValueMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string FormatResult( ValueEventResult result )
		{			
			string meta = ( ( result.Meta != null ) && ( result.Meta.Length > 0 ) ) ? String.Format(" meta=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Meta ) ) : string.Empty;
			string tags = ( ( result.Tags != null ) && ( result.Tags.Count > 0 ) ) ? String.Format(" tags=\"{0}\"",  String.Join( DATA_SEPARATOR, result.Tags.ToArray() ) ) : string.Empty;						
			return String.Format( FORMAT_TEMPLATE, result.Id, result.Value, meta, tags );           
		}
	}
	
	public class CSVValueReport : MetricsReport<ValueMetrics, ValueEventResult>
	{
		public CSVValueReport( ValueMetrics metrics, string reportPath = null, string reportFilename = null ) : base( metrics, reportPath, reportFilename ) {}
		
		override protected string GetHeader() {
			return "Id,Value,Meta";
		}
		
		override protected string FormatResult( ValueEventResult result ) {
			return String.Format( "{0},{1},{2}",
				result.Id,
				result.Value,
				( ( result.Meta == null ) || ( result.Meta.Length <= 0 ) ) ? string.Empty : String.Join( CSV_DATA_SEPARATOR, result.Meta )
			);
		}
	}
	#endif // End INCLUDE_DEV_METRICS || INCLUDE_PERFORMANCE_METRICS

    // Tagging interface for logging without external dependencies
    public interface ILoggableMetrics {}

    public static class MetricsExtensions
    {
        private const string DATE_TIME_FORMAT_STRING = @"HH:mm:ss yyyy-MM-dd";
    
        private static string BuildOutput( object message, ILoggableMetrics caller = null) 
        {
            return GetDateTimeOutput()  + ( caller == null ?  "" : caller.GetType().ToString() + ": " ) + ( message == null ? "Null" : message.ToString() );
        }
    
        /// <summary>
        /// Builds the formatted DateTime output string to be prepended to log output
        /// </summary>
        private static string GetDateTimeOutput() 
        {
            return "[ " + DateTime.UtcNow.ToString( DATE_TIME_FORMAT_STRING ) + " ] ";
        } 

        public static void Log( this ILoggableMetrics caller, object message, UnityEngine.Object context = null )
        {
            #if !UNITY_EDITOR
            UnityEngine.Debug.Log( BuildOutput( message, caller ), context );
            #endif
        }   

        public static void LogWarning( this ILoggableMetrics caller, object message, UnityEngine.Object context = null )
        {
            #if !UNITY_EDITOR
            UnityEngine.Debug.LogWarning( BuildOutput( message, caller ), context );
            #endif
        } 

        public static void LogError( this ILoggableMetrics caller, object message, UnityEngine.Object context = null )
        {
            #if !UNITY_EDITOR
            UnityEngine.Debug.LogError( BuildOutput( message, caller ), context );
            #endif
        }    
    }
}

#endif