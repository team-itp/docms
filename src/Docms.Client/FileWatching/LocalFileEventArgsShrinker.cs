using System;
using System.Collections.Generic;
using System.Linq;
using Docms.Client.SeedWork;

namespace Docms.Client.FileWatching
{
    public class LocalFileEventArgsShrinker
    {
        private Dictionary<string, List<LocalFileEventArgs>> _events = new Dictionary<string, List<LocalFileEventArgs>>();
        private TypeSwitch _ts;

        public IEnumerable<LocalFileEventArgs> Events => _events.SelectMany(e => e.Value).OrderBy(e => e.Timestamp);

        public LocalFileEventArgsShrinker()
        {
            _ts = new TypeSwitch()
                .Case<FileCreatedEventArgs>(x => Apply(x))
                .Case<FileModifiedEventArgs>(x => Apply(x))
                .Case<FileMovedEventArgs>(x => Apply(x))
                .Case<FileDeletedEventArgs>(x => Apply(x));
        }

        public void Apply(LocalFileEventArgs ev)
        {
            _ts.Switch(ev);
        }

        public LocalFileEventArgs Dequeue()
        {
            var next = Events.FirstOrDefault();
            if (next == null)
            {
                return null;
            }
            RemoveEvent(next);
            return next;
        }

        public void Reset()
        {
            _events.Clear();
        }

        private void Apply(FileCreatedEventArgs ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is FileModifiedEventArgs || regEv is FileMovedEventArgs)
            {
                throw new InvalidOperationException();
            }

            if (regEv is FileDeletedEventArgs)
            {
                RemoveEvent(regEv);
                AddEvent(new FileModifiedEventArgs(ev.Path));
            }
            else
            {
                AddEvent(ev);
            }
        }

        private void Apply(FileModifiedEventArgs ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is FileDeletedEventArgs)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(ev);
            }
            else if (regEv is FileMovedEventArgs regMoved)
            {
                RemoveEvent(regEv);
                AddEvent(new FileDeletedEventArgs(regMoved.FromPath));
                AddEvent(new FileCreatedEventArgs(ev.Path));
            }
        }

        private void Apply(FileMovedEventArgs ev)
        {
            var regEv = GetLastEvent(ev.FromPath);
            if (regEv is FileDeletedEventArgs)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(ev);
            }
            else if (regEv is FileMovedEventArgs regMoved)
            {
                if (regMoved.FromPath.ToString() == ev.Path.ToString())
                {
                    RemoveEvent(regEv);
                }
                else
                {
                    RemoveEvent(regEv);
                    AddEvent(new FileMovedEventArgs(ev.Path, regMoved.FromPath));
                }
            }
            else if (regEv is FileCreatedEventArgs)
            {
                RemoveEvent(regEv);
                AddEvent(new FileCreatedEventArgs(ev.Path));
            }
            else if (regEv is FileModifiedEventArgs)
            {
                RemoveEvent(regEv);
                AddEvent(new FileDeletedEventArgs(regEv.Path));
                AddEvent(new FileCreatedEventArgs(ev.Path));
            }
        }

        private void Apply(FileDeletedEventArgs ev)
        {
            var regEv = GetLastEvent(ev.Path);
            if (regEv is FileDeletedEventArgs)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                AddEvent(new FileDeletedEventArgs(ev.Path));
            }
            else if (regEv is FileMovedEventArgs regMoved)
            {
                RemoveEvent(regEv);
                AddEvent(new FileDeletedEventArgs(regMoved.FromPath));
            }
            else if (regEv is FileCreatedEventArgs)
            {
                RemoveEvent(regEv);
            }
            else if (regEv is FileModifiedEventArgs)
            {
                RemoveEvent(regEv);
                AddEvent(new FileDeletedEventArgs(ev.Path));
            }
        }

        private LocalFileEventArgs GetLastEvent(PathString path)
        {
            if (_events.TryGetValue(path.ToString(), out var evList))
            {
                return evList.LastOrDefault();
            }
            return null;
        }

        private void AddEvent(LocalFileEventArgs ev)
        {
            var path = ev.Path.ToString();
            if (_events.TryGetValue(path, out var evList))
            {
                evList.Add(ev);
            }
            else
            {
                _events.Add(path, new List<LocalFileEventArgs>() { ev });
            }
        }

        private void RemoveEvent(LocalFileEventArgs ev)
        {
            var path = ev.Path.ToString();
            if (_events.TryGetValue(path, out var evList))
            {
                evList.Remove(ev);
            }
        }
    }
}