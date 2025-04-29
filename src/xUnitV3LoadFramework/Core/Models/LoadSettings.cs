using System;

namespace xUnitV3LoadFramework.Core.Models
{
	public class LoadSettings
	{
		public int Concurrency { get; set; }
		public TimeSpan Duration { get; set; }
		public TimeSpan Interval { get; set; }
	}
}