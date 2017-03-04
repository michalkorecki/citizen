using System;

namespace Citizen.Statistics
{
	public static class MovingAverage
	{
		public static TimeSpan[] Compute(TimeSpan[] items)
		{
			const int periods = 20;
			TimeSpan[] results = new TimeSpan[items.Length];
			TimeSpan sum = TimeSpan.Zero;
			TimeSpan? previous = null;
			var weight = 2.0 / (periods + 1);
			for (var i = 0; i < items.Length; i++)
			{
				if (previous == null)
				{
					sum += items[i];

					if (i < periods - 1)
					{
						results[i] = TimeSpan.FromSeconds(sum.TotalSeconds / (i + 1));
						continue;
					}

					var sma = TimeSpan.FromSeconds(sum.TotalSeconds / periods);
					previous = sma;
					results[i] = sma;
					continue;
				}

				var emaSeconds = items[i].TotalSeconds * weight + previous.Value.TotalSeconds * (1 - weight);
				var ema = TimeSpan.FromSeconds(emaSeconds);
				results[i] = ema;
				previous = ema;
			}

			return results;
		}
	}
}
