using System.Collections.Generic;
using System.Linq;

namespace DCRReader
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Processor
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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
        List<string> executionTrace = new List<string>();
        List<string> _executionTrace = new List<string>();
        public const string resp = "response";
        public const string inc = "include";
        public const string exc = "exclude";
        public const string cond = "conditon";
        public const string mile = "milestone";

        public void Execute(string id)
        {
            if (enabled.Contains(id))
            {
                SetExecuted(id);
                SetNotPending(id);
                executionTrace.Add(id);
                relations.ToList<Relation>().FindAll(r => r.Source == id && r.Type == resp).ForEach(r => SetPending(r.Target));
                relations.ToList<Relation>().FindAll(r => r.Source == id && r.Type == inc).ForEach(r => SetIncluded(r.Target));
                relations.ToList<Relation>().FindAll(r => r.Source == id && r.Type == exc).ForEach(r => SetExcluded(r.Target));
            }
            Enable();
        }

        public void Enable()
        {
            events.ToList<string>().ForEach(e => SetEnabled(e));
            events.ToList<string>().FindAll(e => !included.Contains(e)).ForEach(e => SetDisabled(e));
            foreach (Relation r in relations)
            {
                switch (r.Type)
                {
                    case cond:
                        if (!executed.Contains(r.Source))
                        {
                            SetDisabled(r.Target);
                        }
                        break;
                    case mile:
                        if (pending.Contains(r.Source))
                        {
                            SetDisabled(r.Target);
                        }
                        break;
                }
            }
        }



        #region small functions
        public void AddEvent(string id)
        {
            events.Add(id);
            Enable();
        }

        public void AddRelation(string target, string source, string type)
        {
            relations.Add(new Relation { Target = target, Source = source, Type = type });
            Enable();
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
            executionTrace.Clear();
            _executed.ToList<string>().ForEach(e => executed.Add(e));
            _enabled.ToList<string>().ForEach(e => enabled.Add(e));
            _pending.ToList<string>().ForEach(e => pending.Add(e));
            _included.ToList<string>().ForEach(e => included.Add(e));
            _executionTrace.ForEach(e => executionTrace.Add(e));
            Enable();
        }

        public void Save()
        {
            _executed.Clear();
            _pending.Clear();
            _enabled.Clear();
            _included.Clear();
            _executionTrace.Clear();
            executed.ToList<string>().ForEach(e => _executed.Add(e));
            enabled.ToList<string>().ForEach(e => _enabled.Add(e));
            pending.ToList<string>().ForEach(e => _pending.Add(e));
            included.ToList<string>().ForEach(e => _included.Add(e));
            executionTrace.ForEach(e => _executionTrace.Add(e));
        }

        public override bool Equals(object obj)
        {
            return obj is Processor processor &&
                   EqualityComparer<HashSet<string>>.Default.Equals(included, processor.included) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(enabled, processor.enabled) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(pending, processor.pending) &&
                   EqualityComparer<HashSet<string>>.Default.Equals(executed, processor.executed);
        }


        //public override int GetHashCode()
        //{
        //    var hashCode = 1437070554;
        //    hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(included);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(enabled);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(executed);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(pending);
        //    return hashCode;
        //}
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public string GetHashCode()
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {
            string hash = string.Empty;
            //List<string> inc = included.ToList<string>();
            //List<string> ena = enabled.ToList<string>();
            //List<string> pen = pending.ToList<string>();
            //List<string> exe = executed.ToList<string>();
            //inc.Sort();
            //ena.Sort();
            //pen.Sort();
            //exe.Sort();
            //inc.ForEach(s => hash += s);
            //ena.ForEach(s => hash += s);
            //pen.ForEach(s => hash += s);
            //exe.ForEach(s => hash += s);
            executionTrace.ForEach(s => hash += s);
            return hash;
        }
        #endregion
    }

    class Relation
    {
        public string Target { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
    }
}
