using System.Collections.Generic;
using System.Linq;
using libsv;
using MoreLinq;

namespace sparkvial.tapes {
    public class CacheTape : IdentityTape {
        public LinkedList<Sample> cache = new LinkedList<Sample>();
        public ulong timeToLive = 0;
        public int maxEntries = 0;

        public override ulong MinTimestamp { get => minTimestamp; }
        public override ulong MaxTimestamp { get => maxTimestamp; }

        public CacheTape(Tape sourceTape) : base(sourceTape) { }

        private new void UpdateInfo(Sample smp) {
            base.UpdateInfo(smp);
            //minTimestamp = cache.Select(s => s.timestamp).Min();
            //maxTimestamp = cache.Select(s => s.timestamp).Min();
        }

        private bool ShouldClean(ulong currTimestamp) {
            if (timeToLive != 0 && cache.Count != 0 && cache.Last.Value.timestamp < MaxTimestamp - timeToLive) {
                return true;
            }
            if (maxEntries != 0 && cache.Count > maxEntries) {
                return true;
            }
            return false;
        }

        private void Clean(ulong currTimestamp) {
            while (ShouldClean(currTimestamp)) {
                cache.RemoveLast();
            }
            if (cache.Count != 0) {
                UpdateInfo(cache.Last.Value);
            }
        }
        
        private void Synchronize() {
            lock (this) {
                var src = sourceTape.GetEnumerator();
                LinkedListNode<Sample> currentNode = null;
                if (cache.Count != 0) {
                    var target = cache.First.Value;
                    while (src.MoveNext() && src.Current != target) {
                        var value = src.Current;
                        if (currentNode == null) {
                            currentNode = cache.AddFirst(value);
                        } else {
                            currentNode = cache.AddAfter(currentNode, value);
                        }
                        UpdateInfo(value);
                    }
                } else {
                    while (src.MoveNext() && !ShouldClean(MaxTimestamp)) {
                        var value = src.Current;
                        if (currentNode == null) {
                            currentNode = cache.AddFirst(value);
                        } else {
                            currentNode = cache.AddAfter(currentNode, value);
                        }
                        UpdateInfo(value);
                    }
                }
                Clean(MaxTimestamp);
            }
        }

        public override IEnumerator<Sample> GetEnumerator() {
            if (sourceTape != null && MaxTimestamp != sourceTape.MaxTimestamp) {
                Clean(MaxTimestamp);
                Synchronize();
            }
            return cache.GetEnumerator();
        }
    }
}
