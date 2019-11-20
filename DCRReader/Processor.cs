using System.Collections.Generic;
using System.Linq;

namespace DCRReader
{
    public class Processor
    {
        HashSet<string> events = new HashSet<string>();
        HashSet<Relation> relations = new HashSet<Relation>();
        HashSet<string> included = new HashSet<string>();
        HashSet<string> enabled = new HashSet<string>();
        HashSet<string> pending = new HashSet<string>();
        HashSet<string> executed = new HashSet<string>();
        HashSet<string> _included = new HashSet<string>();
        HashSet<string> _enabled = new HashSet<string>();
        HashSet<string> _pending = new HashSet<string>();
        HashSet<string> _executed = new HashSet<string>();
        public const string resp = "response";
        public const string inc = "include";
        public const string exc = "exclude";
        public const string cond = "conditon";
        public const string mile = "milestone";

        public void execute(string id)
        {
            if (enabled.Contains(id))
            {
                SetExecuted(id);
                SetNotPending(id);
                relations.ToList<Relation>().FindAll(r => r.source == id && r.type == resp).ForEach(r => SetPending(r.target));
                relations.ToList<Relation>().FindAll(r => r.source == id && r.type == inc).ForEach(r => SetIncluded(r.target));
                relations.ToList<Relation>().FindAll(r => r.source == id && r.type == exc).ForEach(r => SetExcluded(r.target));
            }

        }

        public void Enable()
        {
            events.ToList<string>().ForEach(e => SetEnabled(e));
            events.ToList<string>().FindAll(e => !included.Contains(e)).ForEach(e => SetDisabled(e));
            foreach (Relation r in relations)
            {
                switch (r.type)
                {
                    case cond:
                        if (!executed.Contains(r.source))
                        {
                            SetDisabled(r.target);
                        }
                        break;
                    case mile:
                        if (pending.Contains(r.source))
                        {
                            SetDisabled(r.target);
                        }
                        break;
                }
            }
        }



        #region small functions
        public void AddEvent(string id)
        {
            events.Add(id);
        }

        public void AddRelation(string target, string source, string type)
        {
            relations.Add(new Relation { target = target, source = source, type = type });
        }

        public void SetIncluded(string id)
        {
            if (!events.Contains(id)) return;
            included.Add(id);
        }

        public void SetExcluded(string id)
        {
            if (!events.Contains(id)) return;
            included.Remove(id);
        }

        public void SetEnabled(string id)
        {
            if (!events.Contains(id)) return;
            enabled.Add(id);
        }

        public void SetDisabled(string id)
        {
            if (!events.Contains(id)) return;
            enabled.Remove(id);
        }

        public void SetExecuted(string id)
        {
            if (!events.Contains(id)) return;
            executed.Add(id);
        }

        public void SetPending(string id)
        {
            if (!events.Contains(id)) return;
            pending.Add(id);
        }

        public void SetNotPending(string id)
        {
            if (!events.Contains(id)) return;
            pending.Remove(id);
        }

        public void Load()
        {
            executed.Clear();
            pending.Clear();
            enabled.Clear();
            included.Clear();
            _executed.ToList<string>().ForEach(e => executed.Add(e));
            _enabled.ToList<string>().ForEach(e => enabled.Add(e));
            _pending.ToList<string>().ForEach(e => pending.Add(e));
            _included.ToList<string>().ForEach(e => included.Add(e));
        }

        public void Save()
        {
            _executed.Clear();
            _pending.Clear();
            _enabled.Clear();
            _included.Clear();
            executed.ToList<string>().ForEach(e => _executed.Add(e));
            enabled.ToList<string>().ForEach(e => _enabled.Add(e));
            pending.ToList<string>().ForEach(e => _pending.Add(e));
            included.ToList<string>().ForEach(e => _included.Add(e));
        }

        public override bool Equals(object obj)
        {
            var processor = obj as Processor;
            return processor != null &&
                   EqualityComparer<HashSet<string>>.Default.Equals(included, processor.included) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(enabled, processor.enabled) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(pending, processor.pending) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(executed, processor.executed);
        }

        public override int GetHashCode()
        {
            var hashCode = 1437070554;
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(included);
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(enabled);
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(executed);
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(pending);
            return hashCode;
        }
        #endregion
    }

    class Relation
    {
        public string target { get; set; }
        public string source { get; set; }
        public string type { get; set; }
    }
}
