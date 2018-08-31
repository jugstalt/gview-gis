// HighResClock.cs
// 
// Copyright (C) 2003-2004 Ryan Seghers
//
// This software is provided AS IS. No warranty is granted, 
// neither expressed nor implied. USE THIS SOFTWARE AT YOUR OWN RISK.
// NO REPRESENTATION OF MERCHANTABILITY or FITNESS FOR ANY 
// PURPOSE is given.
//
// License to use this software is limited by the following terms:
// 1) This code may be used in any program, including programs developed
//    for commercial purposes, provided that this notice is included verbatim.
//    
// Also, in return for using this code, please attempt to make your fixes and
// updates available in some way, such as by sending your updates to the
// author.
// 

using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace RTools.Util
{
	/// <summary>
	/// This provides access to the Kernel32.dll high resolution clock API.  This
	/// is motivated by the need to have higher resolution than .Net's
	/// DateTime.Now, which is apparently about 10ms.  The effective resolution
	/// of HighResClock on a P4 1.4GHz is about 10us.
	/// <seealso cref="System.TimeSpan"/>
	/// <seealso cref="System.DateTime"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// WARNING: I don't have any particularly good way of verifying
	/// the accuracy of this class.  The best I've done is in the TestSelf()
	/// which is to compare the measured duration to Sleep(), which is loose.
	/// Also, I haven't tested this on any other PCs.  I understand
	/// that the high performance counter may not
	/// be available on all systems.
	/// </para>
	/// <para>The "ticks" in this class DO correspond to ticks
	/// in DateTime and TimeSpan, except in the Frequency property which is 
	/// explicitly the high performance counter frequency.</para>
	/// <para>
	/// It's relatively expensive to read this clock, compared to DateTime.Now.
	/// Although it's only about 10us on a P4 1.4GHz, it's faster
	/// to use DateTime.Now.
	/// </para>
	/// <para>
	/// This does a calibration to determine function call overhead time, 
	/// which is a static field.  
	/// This has a static constructor which calls Calibrate().
	/// Calibrate() temporarily boosts the calling process and thread priority
	/// to avoid being affected by other processes/threads.
	/// </para>
	/// <para>
	/// The duration calculations are adjusted by the function call overhead
	/// time calculated during Calibrate(), but Now, NowTicks, etc are not.
	/// </para>
	/// <para>
	/// You can use this just like DateTime for timing durations:
	/// <code>
	/// DateTime startTime = HighResClock.Now;
	/// // do something you want to time here
	/// TimeSpan duration = HighResClock.Now - startTime;
	/// Console.WriteLine("Duration: {0}ms", duration.TotalMilliseconds);
	/// </code> 
	/// </para>
	/// <para>
	/// To compensate for the overhead in the high resolution clock reads,
	/// you can use the following:
	/// <code>
	/// DateTime startTime = HighResClock.Now;
	/// // do something you want to time here
	/// TimeSpan duration = HighResClock.CalcTimeSpan(startTime);
	/// Console.WriteLine("Duration: {0}ms", duration.TotalMilliseconds);
	/// </code> 
	/// </para>
	/// </remarks>
	public class HighResClock
	{
		#region Dll Import

		/// <summary>
		/// The ticks returned by this function are not the same as
		/// .Net's ticks.
		/// </summary>
		[DllImport("kernel32", EntryPoint="QueryPerformanceCounter")]
		private static extern uint K32QueryPerformanceCounter(ref long t);

		/// <summary>
		/// This returns Kernel32 ticks/sec.
		/// </summary>
		[DllImport("kernel32", EntryPoint="QueryPerformanceFrequency")]
		private static extern uint K32QueryPerformanceFrequency(ref long t);

		#endregion

		#region Fields

		/// <summary>
		/// Frequency obtained from kernel32.  This is in kernel32's ticks 
		/// per second.
		/// </summary>
		static long perfFreq = 0;

		/// <summary>
		/// The minimum duration that this class is able to measure. 
		/// This is measured during Calibrate().
		/// This represents the number of ticks it takes to make
		/// a duration measurement, and is therefore the effective 
		/// resolution of this class.
		/// </summary>
		static long overheadTicks;

		/// <summary>
		/// This stores the offset between Kernel32's ticks and .Net's
		/// DateTime's ticks, so that we can produce accurate DateTime's.
		/// </summary>
		static long k32TicksOffset;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the tick count from the clock.
		/// </summary>
		/// <remarks>No adjustment is made to this due to OverheadTicks.</remarks>
		public static long NowTicks
		{
			get
			{
				if (perfFreq == 0) 
					throw new NotSupportedException("This system does not have a high performance counter.");

				long t = 0;
				K32QueryPerformanceCounter(ref t);
				// convert to .Net ticks
				long ticks = (long)(t / ((float)perfFreq / 1e7));
				return(ticks);
			}
		}

		/// <summary>
		/// Return a DateTime which represents the current time.  This is similar
		/// to DateTime.Now, but is higher resolution.
		/// <seealso cref="System.DateTime"/>
		/// </summary>
		/// <remarks>No adjustment is made to this due to OverheadTicks.</remarks>
		public static DateTime Now
		{
			get
			{
				return(new DateTime(NowTicks - k32TicksOffset));
			}
		}

		/// <summary>
		/// The minimum duration that this class is able to measure. 
		/// This is measured during Calibrate().
		/// This represents the number of ticks it takes to make
		/// a duration measurement, and is therefore the effective 
		/// resolution of this class.
		/// This is subtracted from all duration calculations, which should 
		/// compensate for the clock read overhead.
		/// </summary>
		public static long OverheadTicks
		{
			get { return(overheadTicks); }
		}

		/// <summary>
		/// This returns the number of high performance counter ticks (NOT .Net ticks) per second,
		/// as measured by Kernel32.dll's QueryPerformanceFrequency() function.
		/// </summary>
		public static long Frequency
		{
			get { return(perfFreq); }
		}

		#endregion

		#region Constructor

		/// <summary>Not supposed to instantiate this class.</summary>
		private HighResClock() {}

		/// <summary>Static constructor.</summary>
		static HighResClock()
		{
			Calibrate();
		}

		#endregion

		#region Main Public API

		/// <summary>
		/// Measure overhead for interop calls and clock resolution.
		/// The resultant values are stored in static fields.
		/// The resultant clock frequency and function call overhead
		/// measurement are used in ticks to ms conversion and 
		/// duration calculations, respectively.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is called from the static constructor, so you typically
		/// won't benefit from calling it, unless during construction
		/// there were other very high priority things going on that
		/// affected the timing of the statements in this method.
		/// </para>
		/// <para>
		/// This temporarily boosts this process and thread priority
		/// to avoid being affected by other processes/threads, so you 
		/// definitely don't want to call this a lot.
		/// </para>
		/// </remarks>
		public static void Calibrate()
		{
			K32QueryPerformanceFrequency(ref perfFreq);

			// boost process and thread priority temporarily
			ThreadPriority oldThreadPriority = Thread.CurrentThread.Priority;
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

			Process proc = Process.GetCurrentProcess();
			ProcessPriorityClass oldProcPriorityClass = proc.PriorityClass;
			proc.PriorityClass = ProcessPriorityClass.RealTime;

			try
			{
				// calc offset between our ticks and DateTime
				k32TicksOffset = NowTicks - DateTime.Now.Ticks;

				// calculate measurement overhead by taking a set of measurements
				long start, stop = 0;
				overheadTicks = Int64.MaxValue;

				for (int i = 0; i < 10; i++)
				{
					start = NowTicks;
					stop = NowTicks;
					long ticks = stop - start;
					if (ticks > 0 && ticks < overheadTicks) overheadTicks = ticks;
				}

				// handle case where no measurement gave us a difference
				// in case the overhead is under the perf counter resolution
				if (overheadTicks == Int64.MaxValue) overheadTicks = 0;

				//Console.WriteLine("perfFreq = {0}, overheadTicks = {1}, overheadMs = {2}", 
				//	perfFreq, overheadTicks, TicksToMs(overheadTicks));
			}
			finally
			{
				Thread.CurrentThread.Priority = oldThreadPriority;
				proc.PriorityClass = oldProcPriorityClass;
			}
		}

		/// <summary>
		/// Convert ticks to milliseconds. (A tick is 100 nanoseconds.)
		/// </summary>
		/// <remarks>No adjustment is made to this due to OverheadTicks.</remarks>
		/// <param name="ticks">The tick count.</param>
		/// <returns>float - the number of milliseconds.</returns>
		public static float TicksToMs(long ticks)
		{
			return((float)ticks / 10000);
		}

		/// <summary>
		/// Calculate the duration as a TimeSpan.
		/// <seealso cref="System.TimeSpan"/>
		/// </summary>
		/// <remarks>This subtracts OverheadTicks from the measured ticks
		/// to compensate for overhead.</remarks>
		/// <param name="startTicks">The starting tick count.</param>
		/// <param name="stopTicks">The stopTicks tick count.</param>
		/// <returns>A new TimeSpan.</returns>
		public static TimeSpan CalcTimeSpan(long startTicks, long stopTicks)
		{
			long ticks = stopTicks - startTicks;
			ticks -= overheadTicks;
			ticks = Math.Max(0, ticks);
			return(new TimeSpan(ticks));
		}

		/// <summary>
		/// Calculate the duration (to NowTicks) as a TimeSpan.
		/// <seealso cref="System.TimeSpan"/>
		/// </summary>
		/// <remarks>This subtracts OverheadTicks from the measured ticks
		/// to compensate for overhead.</remarks>
		/// <param name="startTicks">The starting tick count.</param>
		/// <returns>A new TimeSpan.</returns>
		public static TimeSpan CalcTimeSpan(long startTicks)
		{
			return(CalcTimeSpan(startTicks, NowTicks));
		}

		/// <summary>
		/// Calculate the duration (to NowTicks) as a TimeSpan.
		/// <seealso cref="System.TimeSpan"/>
		/// </summary>
		/// <remarks>This subtracts OverheadTicks from the measured ticks
		/// to compensate for overhead.</remarks>
		/// <param name="startTime">The starting DateTime.</param>
		/// <returns>A new TimeSpan.</returns>
		public static TimeSpan CalcTimeSpan(DateTime startTime)
		{
			return(CalcTimeSpan(startTime.Ticks, NowTicks));
		}

		/// <summary>
		/// Calculate the duration (to NowTicks) as a TimeSpan.
		/// <seealso cref="System.TimeSpan"/>
		/// </summary>
		/// <remarks>This subtracts OverheadTicks from the measured ticks
		/// to compensate for overhead.</remarks>
		/// <param name="startTime">The starting DateTime.</param>
		/// <param name="stopTime">The stopping DateTime.</param>
		/// <returns>A new TimeSpan.</returns>
		public static TimeSpan CalcTimeSpan(DateTime startTime, DateTime stopTime)
		{
			return(CalcTimeSpan(startTime.Ticks, stopTime.Ticks));
		}

		#endregion

		#region Test Methods

		/// <summary>
		/// A simple self test.
		/// </summary>
		/// <returns>bool - true for test passed, false for failed</returns>
		public static bool TestSelf()
		{
			//HighResClock.Calibrate();
			Console.WriteLine("Measurement overhead: {0} ticks", HighResClock.OverheadTicks);
			Console.WriteLine("Frequency: {0} cpuTicks/sec", HighResClock.Frequency);

			double realFreq = 1.4 * 1024 * 1024 * 1024;
			Console.WriteLine("1.4 GHz CPU clock freq / perf counter freq = {0:f3}",
				 realFreq/HighResClock.Frequency);

			// figure out how often high perf counter wraps
			double rangeSeconds = (double)long.MaxValue / HighResClock.Frequency;
			double rangeDays = rangeSeconds / (3600*24);
			double rangeYears = rangeDays/365;
			Console.WriteLine("Span of clock is {0:e3} s, {1:e3} d, {2:e3} years", 
				rangeSeconds, rangeDays, rangeYears);

			long start = HighResClock.NowTicks;
			long stop = HighResClock.NowTicks;
			long stop1 = HighResClock.NowTicks;

			Console.WriteLine("duration: {0} ms", HighResClock.CalcTimeSpan(start, stop).TotalMilliseconds);
			Console.WriteLine("duration1: {0} ms", HighResClock.CalcTimeSpan(start, stop1).TotalMilliseconds);
			Console.WriteLine("duration: {0} ms", HighResClock.CalcTimeSpan(start).TotalMilliseconds);

			// check against sleep
			start = HighResClock.NowTicks;
			int sleepTime = 100;
			Thread.Sleep(sleepTime);
			stop = HighResClock.NowTicks;
			double d = HighResClock.CalcTimeSpan(start, stop).TotalMilliseconds;
			Console.WriteLine("Slept {0} ms and that gave measured {1:f2} ms",
				sleepTime, d);
			Console.WriteLine("    (This is typically a little off (5ms?), probably due to "
				+ "thread scheduling latencies.)");

			// try TimeSpan
			TimeSpan span = HighResClock.CalcTimeSpan(start, stop);
			Console.WriteLine("The same duration as a time span is {0} ms", span.TotalMilliseconds);

			// try DateTime
			Console.WriteLine("My DateTime: {0},   .Net's DateTime: {1}", 
				HighResClock.Now, DateTime.Now);

			//
			// time something with DateTime
			//
			DateTime startDateTime = DateTime.Now;
			DateTime startHrc = HighResClock.Now;

			// kill some time
			double res = 0.0;
			for (int i = 0; i < 50000; i++) 
			{
				res += Math.Sin(Math.Cos(Math.Acos(1.0)));
			}

			span = DateTime.Now - startDateTime;
			TimeSpan spanHrc = HighResClock.Now - startHrc;
			Console.WriteLine("DateTime's timing: {0} ms", span.TotalMilliseconds);
			Console.WriteLine("HighResClock's timing: {0} ms", spanHrc.TotalMilliseconds);

			//
			// check DateTime's resolution
			//
			startDateTime = DateTime.Now;
			double maxMsGap = 100000;
			for (int i = 0; i < 100; i++)
			{
				double msGap = ((TimeSpan)(DateTime.Now - startDateTime)).TotalMilliseconds;
				Thread.Sleep(1);
				if ((msGap > 0) && (msGap < maxMsGap)) maxMsGap = msGap;
			}
			Console.WriteLine("DateTime's resolution is {0:f2} ms", maxMsGap);
			
			//
			// check HighResClock's DateTime resolution
			//
			startDateTime = HighResClock.Now;
			maxMsGap = 100000;
			for (int i = 0; i < 100; i++)
			{
				double msGap = ((TimeSpan)(HighResClock.Now - startDateTime)).TotalMilliseconds;
				Thread.Sleep(1);
				if ((msGap > 0) && (msGap < maxMsGap)) maxMsGap = msGap;
			}
			Console.WriteLine("HighResClock's DateTime resolution is {0} ms", maxMsGap);
			
			return(true);
		}

		#endregion
	}
}
