using System.Collections.Concurrent;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Rationals;
using MalodyEffectPoint = qua2ma.Malody.MalodyEffectPoint;
using MalodyFile = qua2ma.Malody.MalodyFile;
using MalodyFileMeta = qua2ma.Malody.MalodyFileMeta;
using MalodyFileSong = qua2ma.Malody.MalodyFileSong;
using MalodyHitObject = qua2ma.Malody.MalodyHitObject;
using MalodyMetaKeymode = qua2ma.Malody.MalodyMetaKeymode;
using MalodyTimingPoint = qua2ma.Malody.MalodyTimingPoint;

namespace qua2ma;

public class QuaToMalodyConverter
{
    public static IComparer<TimingPointInfo> TimingPointComparer = new TimingPointRelationalComparer();
    private readonly ConcurrentDictionary<string, string> _pathReplacements;
    private readonly List<float> _prefixBeats = [];
    private readonly Qua _qua;

    public QuaToMalodyConverter(Qua qua, ConcurrentDictionary<string, string> pathReplacements)
    {
        _qua = qua;
        _pathReplacements = pathReplacements;
        MalodyFile = new MalodyFile
        {
            Meta = new MalodyFileMeta
            {
                Background = GetPath(_qua.BackgroundFile),
                Creator = _qua.Creator,
                Id = -1,
                Keymode = new MalodyMetaKeymode
                {
                    Keymode = _qua.GetKeyCount()
                },
                Mode = 0,
                PreviewTime = _qua.SongPreviewTime,
                Song = new MalodyFileSong
                {
                    Artist = _qua.Artist,
                    Title = _qua.Title
                },
                Version = qua.DifficultyName
            }
        };
    }

    public MalodyFile MalodyFile { get; }

    public async Task Generate()
    {
        GeneratePrefixBeats();
        await Task.WhenAll([
            Task.Run(GenerateTimingPoints),
            Task.Run(GenerateNotes),
            Task.Run(GenerateSVs)
        ]);
    }

    private void GenerateSVs()
    {
        MalodyFile.EffectPoints = [];
        foreach (var sv in _qua.SliderVelocities)
        {
            MalodyFile.EffectPoints.Add(new MalodyEffectPoint
            {
                Beat = TimeToBeat(sv.StartTime),
                Scroll = sv.Multiplier
            });
        }
    }

    private void GenerateNotes()
    {
        MalodyFile.Hitobjects = [];
        foreach (var hitObject in _qua.HitObjects)
        {
            MalodyFile.Hitobjects.Add(new MalodyHitObject
            {
                Beat = TimeToBeat(hitObject.StartTime),
                BeatEnd = hitObject.IsLongNote ? TimeToBeat(hitObject.EndTime) : null,
                Column = hitObject.Lane - 1
            });
        }

        MalodyFile.Hitobjects.Add(new MalodyHitObject
        {
            Beat = [0, 0, 1],
            Sound = GetPath(_qua.AudioFile),
            Type = 1
        });
    }

    private void GenerateTimingPoints()
    {
        MalodyFile.TimingPoints = [];
        foreach (var timingPoint in _qua.TimingPoints)
        {
            MalodyFile.TimingPoints.Add(new MalodyTimingPoint
            {
                Bpm = timingPoint.Bpm,
                Beat = TimeToBeat(timingPoint.StartTime)
            });
        }
    }

    private void GeneratePrefixBeats()
    {
        if (_qua.TimingPoints.Count == 0) return;
        _prefixBeats.Clear();
        _prefixBeats.Add(0);
        if (_qua.TimingPoints[0].StartTime > 0)
            _qua.TimingPoints.Insert(0, new TimingPointInfo
            {
                Bpm = 60,
                Hidden = true,
                StartTime = 0
            });
        var previousTimingPoint = _qua.TimingPoints[0];
        foreach (var timingPoint in _qua.TimingPoints.Skip(1))
        {
            var beatsPassed = (timingPoint.StartTime - previousTimingPoint.StartTime) /
                              previousTimingPoint.MillisecondsPerBeat;
            _prefixBeats.Add(_prefixBeats[^1] + beatsPassed);
            previousTimingPoint = timingPoint;
        }
    }

    private List<int> TimeToBeat(float time)
    {
        var tpIndex = _qua.TimingPoints.BinarySearch(new TimingPointInfo { StartTime = time }, TimingPointComparer);
        if (tpIndex < 0)
            tpIndex = ~tpIndex - 1;
        if (tpIndex < 0)
            return [0, 0, 1];

        var tp = _qua.TimingPoints[tpIndex];
        var tpBeat = _prefixBeats[tpIndex];
        var beat = tpBeat + (time - tp.StartTime) / tp.MillisecondsPerBeat;
        var fraction = Rational.Approximate(beat % 1);
        return [(int)beat, (int)fraction.Numerator, (int)fraction.Denominator];
    }

    private string GetPath(string path)
    {
        return _pathReplacements.GetValueOrDefault(path, path);
    }

    private sealed class TimingPointRelationalComparer : IComparer<TimingPointInfo>
    {
        public int Compare(TimingPointInfo x, TimingPointInfo y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.StartTime.CompareTo(y.StartTime);
        }
    }
}