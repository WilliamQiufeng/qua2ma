using Quaver.API.Maps.Structures;

namespace qua2ma;

public readonly record struct PreciseSv(double Time, double Velocity)
{
    public static (double, List<PreciseSv>) From(float initialSv, List<SliderVelocityInfo> svs)
    {
        const float trackRounding = 100f;
        if (svs.Count == 0)
            return (initialSv, []);

        var result = new List<PreciseSv>();
        var currentValue = (long)(svs[0].StartTime * initialSv * trackRounding);

        for (var i = 1; i < svs.Count; i++)
        {
            var multiplier = svs[i - 1].Multiplier;
            if (float.IsNaN(multiplier)) multiplier = 0;

            var startTime = svs[i - 1].StartTime;
            var endTime = svs[i].StartTime;
            var newValue =
                currentValue + (long)((endTime - startTime) * multiplier * trackRounding);

            var newMultiplier =
                ((double)newValue - currentValue) 
                / trackRounding
                / ((double)endTime - startTime);
            if (double.IsNaN(newMultiplier)) newMultiplier = 0;

            if (result.LastOrDefault() is {} last)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (last.Time == startTime)
                {
                    result.RemoveAt(result.Count - 1);
                }
            }
            
            result.Add(new PreciseSv(startTime, multiplier));
        }

        var lastMultiplier = svs.Last().Multiplier;
        if (float.IsNaN(lastMultiplier)) lastMultiplier = 0;

        var lastStartTime = svs.Last().StartTime;
        result.Add(new PreciseSv(lastStartTime, lastMultiplier));
        return (initialSv, result);
    }
}