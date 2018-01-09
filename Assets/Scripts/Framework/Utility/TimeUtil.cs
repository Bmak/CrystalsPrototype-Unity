using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;


public enum TimeType
{
	Moment,
	Second,
	Minute,
	Hour,
	Day
}

public class TimeUtil
{
    // Number of DateTime ticks in a second (one tick is 100 nanoseconds)
	public const long HOURS_IN_DAY = 24;
    public const long DATETIME_TICKS_IN_SECOND = 10000000;
    public const long SECONDS_IN_MINUTE = 60;
	public const long SECONDS_IN_HOUR = SECONDS_IN_MINUTE * 60;
	public const long SECONDS_IN_DAY = SECONDS_IN_HOUR * HOURS_IN_DAY;
    public const long MILLISECONDS_IN_SECOND = 1000;
	private const long MOMENTS_LIMIT = 5;

    // Unix epoch never changes, so hold a pre-constructed reference to it.
    public static readonly DateTime EPOCH_DATE_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	// Start initial stopwatch. Will be executed before this field is referenced for the first time.
	// Ensures this value is non-null in the case Reset() is not executed prior to first use.
	private static Stopwatch _stopwatch = Stopwatch.StartNew();

	public static Stopwatch Stopwatch {
		get {  return _stopwatch; }
	}

	public static void Reset() {
		_stopwatch = Stopwatch.StartNew();
	}

    /// <summary>
    /// Converts a number of seconds into one of several time units, looking for the highest whole unit
    /// </summary>
    /// <param name="seconds">The length of time in seconds</param>
    /// <param name="substituteMomentsAgo">Flag indicating that small times should be replaced by "moments ago"</param>
    /// <returns>Time-type that represent the highest whole value of the available time unit</returns>
    public static TimeType RelativeTimeType(long seconds, bool substituteMomentsAgo = true)
    {
        // Prevent negative values
        seconds = seconds >= 0 ? seconds : 0;

        if (seconds <= MOMENTS_LIMIT && substituteMomentsAgo)
        {
            return TimeType.Moment;
        }
        
        if (seconds < SECONDS_IN_MINUTE)
        {
            return TimeType.Second;
        }
        
        if (seconds < SECONDS_IN_HOUR)
        {
            return TimeType.Minute;
        }
        
        if (seconds < SECONDS_IN_DAY)
        {
            return TimeType.Hour;
        }
        
        return TimeType.Day;
    }

    /// <summary>
    /// Converts a number of seconds into a floored long value, where the floor is the highest whole unit of whatever time unit we can reach with that many seconds
    /// </summary>
    /// <param name="seconds">The length of time in seconds</param>
    /// <returns>Number of units in time-type relative units</returns>
    public static long RelativeTimeFloorValue(long seconds)
    {
        // Prevent negative values
        seconds = seconds >= 0 ? seconds : 0;

        if (seconds < SECONDS_IN_MINUTE)
        {
            return seconds;
        }
        
        if (seconds < SECONDS_IN_HOUR)
        {
            return seconds / SECONDS_IN_MINUTE;
        }
        
        if (seconds < SECONDS_IN_DAY)
        {
            return seconds / SECONDS_IN_HOUR;
        }       
        return seconds / SECONDS_IN_DAY;
    }
	
	/// <summary>
    /// Converts a number of seconds into the remainder of a floored long value, where the floor is the highest whole unit of whatever time unit we can reach with that many seconds
    /// </summary>
    /// <param name="seconds">The length of time in seconds</param>
    /// <returns>Number of units remaining after a floor operation in time-type relative units</returns>
    public static long RelativeTimeFloorValueRemainder(long seconds)
    {
        // Prevent negative values
        seconds = seconds >= 0 ? seconds : 0;

        if (seconds < SECONDS_IN_MINUTE)
        {
            return seconds;
        }
        
        if (seconds < SECONDS_IN_HOUR)
        {
            return seconds % SECONDS_IN_MINUTE;
        }
        
        if (seconds < SECONDS_IN_DAY)
        {
            return seconds % SECONDS_IN_HOUR;
        }       
        return seconds % SECONDS_IN_DAY;
    }

    public static TimeSpan GetCurrentTimeSpan() {
        return DateTime.UtcNow - EPOCH_DATE_TIME;
    }

	/// <summary>
	/// Converts a Unix UTC timestamp to a DateTime
	/// </summary>
	/// <returns>Unix UTC timestamp as long</returns>
	public static DateTime UnixToDateTime(long timestamp)
	{
		return EPOCH_DATE_TIME.AddSeconds(timestamp);
	}

    // dateTime should be in UTC
    public static long DateTimeToUnix(DateTime dateTime)
    {
        TimeSpan t = dateTime - EPOCH_DATE_TIME;
        return (long)t.TotalSeconds; 
    }

	public static void InitializeRandomSeed() {
		UnityEngine.Random.InitState(unchecked((int)DateTime.Now.Ticks));
	}

    // This should only be called during auth. All other times, the local time zone should be retrieved from the playerDO.
    public static int GetLocalTimeZoneOffsetMinutes()
    {
        return Convert.ToInt32(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes);
    }
}
