using System;
using System.Collections.Generic;
using System.Linq;
using Docms.Client.SeedWork;

namespace Docms.Client.FileTrees
{
    public class FileTreeEventShrinker
    {
        private List<FileTreeEvent> _events = new List<FileTreeEvent>();

        public IEnumerable<FileTreeEvent> Events => _events;

        public void Apply(FileTreeEvent ev)
        {
            var ts = new TypeSwitch()
                .Case((DocumentCreated x) => Apply(x))
                .Case((DocumentUpdated x) => Apply(x))
                .Case((DocumentMoved x) => Apply(x))
                .Case((DocumentDeleted x) => Apply(x));

            ts.Switch(ev);
        }

        public void Reset()
        {
            _events.Clear();
        }

        private void Apply(DocumentCreated ev)
        {
            var regEv = _events.FirstOrDefault(e => e.Path.ToString() == ev.Path.ToString());
            if (regEv is DocumentUpdated || regEv is DocumentMoved)
            {
                throw new InvalidOperationException();
            }

            if (regEv is DocumentDeleted)
            {
                _events.Remove(regEv);
                _events.Add(new DocumentUpdated(ev.Path));
            }
            else
            {
                _events.Add(ev);
            }
        }

        private void Apply(DocumentUpdated ev)
        {
            var regEv = _events.FirstOrDefault(e => e.Path.ToString() == ev.Path.ToString());
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv == null)
            {
                _events.Add(ev);
            }
        }

        private void Apply(DocumentMoved ev)
        {
            var regEv = _events.FirstOrDefault(e => e.Path.ToString() == ev.OldPath.ToString());
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv is DocumentMoved regMoved)
            {
                if (regMoved.OldPath.ToString() == ev.Path.ToString())
                {
                    _events.Remove(regEv);
                }
                else
                {
                    _events.Remove(regEv);
                    _events.Add(new DocumentMoved(ev.Path, regMoved.OldPath));
                    var regEvOld = _events.FirstOrDefault(e => e.Path.ToString() == regMoved.OldPath.ToString());
                    if (regEvOld != null)
                    {
                        if (regEvOld is DocumentUpdated || regEvOld is DocumentMoved || regEvOld is DocumentDeleted)
                        {
                            throw new InvalidOperationException();
                        }


                        _events.Remove(regEvOld);
                        _events.Add(new DocumentCreated(regEvOld.Path));
                    }
                }
            }
            else if (regEv is DocumentCreated)
            {
                _events.Remove(regEv);
                _events.Add(new DocumentCreated(ev.Path));
            }
            else if (regEv is DocumentUpdated)
            {
                _events.Remove(regEv);
                _events.Add(new DocumentDeleted(regEv.Path));
                _events.Add(new DocumentCreated(ev.Path));
            }
        }

        private void Apply(DocumentDeleted ev)
        {
            var regEv = _events.FirstOrDefault(e => e.Path.ToString() == ev.Path.ToString());
            if (regEv is DocumentDeleted)
            {
                throw new InvalidOperationException();
            }

            if (regEv is DocumentMoved regMoved)
            {
                if (regMoved.OldPath.ToString() == ev.Path.ToString())
                {
                    _events.Remove(regEv);
                }
                else
                {
                    _events.Remove(regEv);
                    _events.Add(new DocumentMoved(ev.Path, regMoved.OldPath));
                }
            }
            else if (regEv is DocumentCreated)
            {
                _events.Remove(regEv);
                _events.Add(new DocumentCreated(ev.Path));
            }
            else if (regEv is DocumentUpdated)
            {
                _events.Remove(regEv);
                _events.Add(new DocumentDeleted(regEv.Path));
                _events.Add(new DocumentCreated(ev.Path));
            }
        }
    }
}