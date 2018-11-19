using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using libsv;

namespace sparkvial.tapes {
    public abstract class Tape : IEnumerable<Sample> {
        protected ulong minTimestamp = ulong.MaxValue;
        protected ulong maxTimestamp = ulong.MinValue;

        public abstract ulong MinTimestamp { get; }
        public abstract ulong MaxTimestamp { get; }

        protected void UpdateInfo(Sample value) {
            if (value.timestamp < MinTimestamp) {
                minTimestamp = value.timestamp;
            }
            if (value.timestamp > MaxTimestamp) {
                maxTimestamp = value.timestamp;
            }
        }

        public abstract IEnumerator<Sample> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class SourceTape : Tape {
        private LinkedList<Sample> data = new LinkedList<Sample>();

        public override ulong MinTimestamp { get => minTimestamp; }
        public override ulong MaxTimestamp { get => maxTimestamp; }

        public void Add(Sample value) {
            data.AddFirst(value);
            UpdateInfo(value);
        }

        public override IEnumerator<Sample> GetEnumerator() {
            return data.GetEnumerator();
        }
    }

    public class IdentityTape : Tape {
        public Tape sourceTape;

        public override ulong MinTimestamp { get => sourceTape.MinTimestamp; }
        public override ulong MaxTimestamp { get => sourceTape.MaxTimestamp; }

        public IdentityTape(Tape sourceTape) {
            this.sourceTape = sourceTape;
        }

        public override IEnumerator<Sample> GetEnumerator() {
            return sourceTape.GetEnumerator();
        }
    }

    // TODO
    public class CombineTape : Tape {
        public List<Tape> sourceTapes;
        public ulong debounceTime = 0;

        public override ulong MinTimestamp { get => sourceTapes.Min(t => t.MinTimestamp); }
        public override ulong MaxTimestamp { get => sourceTapes.Max(t => t.MaxTimestamp); }

        public CombineTape(List<Tape> sourceTapes) {
            this.sourceTapes = sourceTapes;
        }

        public override IEnumerator<Sample> GetEnumerator() {
            return new CombineTapeEnumerator();
        }
    }

    public class CombineTapeEnumerator : IEnumerator<Sample> {
        public List<Tape> sourceTapes;
        public List<ulong> lastSampleTimes;

        public Sample Current => throw new NotImplementedException();

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext() {
            throw new NotImplementedException();
        }

        public void Reset() {
            throw new NotImplementedException();
        }
    }
}
