using System;
using System.Collections.Generic;
using System.Linq;
using Docms.Client.SeedWork;

namespace Docms.Client.FileStorage
{
    public class LocalFileEventShrinker
    {
        private Dictionary<string, List<LocalFileEvent>> _events = new Dictionary<string, List<LocalFileEvent>>();
        private TypeSwitch _ts;

        public IEnumerable<LocalFileEvent> Events => _events.SelectMany(e => e.Value).OrderBy(e => e.Timestamp);

        public LocalFileEventShrinker()
        {
            _ts = new TypeSwitch()
                .Case<DocumentCreated>(x => Apply(x))
                .Case<DocumentUpdated>(x => Apply(x))
                .Case<DocumentMoved>(x => Apply(x))
                .Case<DocumentDeleted>(x => Apply(x));
        }

        public void Apply(LocalFileEvent ev)
        {
            _ts.Switch(ev);
        }

        public void Reset()
        {
            _events.Clear();
        }

        private void Apply(DocumentCreated ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is DocumentUpdated || regEv is DocumentMoved)
            {
                throw new InvalidOperationException();
            }

            if (regEv is DocumentDeleted)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentUpdated(ev.Path));
            }
            else
            {
                AddEvent(ev);
            }
        }

        private void Apply(DocumentUpdated ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(ev);
            }
            else if (regEv is DocumentMoved regMoved)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentDeleted(regMoved.OldPath));
                AddEvent(new DocumentCreated(ev.Path));
            }
        }

        private void Apply(DocumentMoved ev)
        {
            var regEv = GetLastEvent(ev.OldPath);
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(ev);
            }
            else if (regEv is DocumentMoved regMoved)
            {
                if (regMoved.OldPath.ToString() == ev.Path.ToString())
                {
                    RemoveEvent(regEv);
                }
                else
                {
                    RemoveEvent(regEv);
                    AddEvent(new DocumentMoved(ev.Path, regMoved.OldPath));
                }
            }
            else if (regEv is DocumentCreated)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentCreated(ev.Path));
            }
            else if (regEv is DocumentUpdated)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentDeleted(regEv.Path));
                AddEvent(new DocumentCreated(ev.Path));
            }
        }

        private void Apply(DocumentDeleted ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(new DocumentDeleted(ev.Path));
            }
            else if (regEv is DocumentMoved regMoved)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentDeleted(regMoved.OldPath));
            }
            else if (regEv is DocumentCreated)
            {
                RemoveEvent(regEv);
            }
            else if (regEv is DocumentUpdated)
            {
                RemoveEvent(regEv);
                AddEvent(new DocumentDeleted(ev.Path));
            }
        }

        private LocalFileEvent GetLastEvent(PathString path)
        {
            if (_events.TryGetValue(path.ToString(), out var evList))
            {
                return evList.LastOrDefault();
            }
            return null;
        }

        private void AddEvent(LocalFileEvent ev)
        {
            var path = ev.Path.ToString();
            if (_events.TryGetValue(path, out var evList))
            {
                evList.Add(ev);
            }
            else
            {
                _events.Add(path, new List<LocalFileEvent>() { ev });
            }
        }

        private void RemoveEvent(LocalFileEvent ev)
        {
            var path = ev.Path.ToString();
            if (_events.TryGetValue(path, out var evList))
            {
                evList.Remove(ev);
            }
        }
    }
}