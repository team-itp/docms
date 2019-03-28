using Docms.Client.Documents;
using Docms.Client.Operations;
using System.Collections.Generic;
using System.Threading;

namespace Docms.Client.Operations
{
    public class DetermineDiffOperationResult
    {
        public List<(DocumentNode local, DocumentNode remote)> Diffs { get; } = new List<(DocumentNode local, DocumentNode remote)>();
        public void Add(DocumentNode local, DocumentNode remote)
        {
            Diffs.Add((local, remote));
        }
    }

    public class DetermineDiffOperation : SyncOperationBase
    {
        private ApplicationContext context;

        public DetermineDiffOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var localDocuments = context.LocalStorage.Root.ListAllDocuments();
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
            var result = new DetermineDiffOperationResult();
            using (var le = localDocuments.GetEnumerator())
            using (var re = remoteDocuments.GetEnumerator())
            {
                var ln = le.MoveNext();
                var rn = re.MoveNext();
                while (ln && rn)
                {
                    var lv = le.Current;
                    var rv = re.Current;

                    var comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                    while (comp != 0)
                    {
                        if (comp > 0)
                        {
                            result.Add(lv, null);
                            ln = le.MoveNext();
                            if (!ln)
                            {
                                break;
                            }
                            lv = le.Current;
                            comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                        }
                        else if (comp < 0)
                        {
                            result.Add(null, rv);
                            rn = re.MoveNext();
                            if (!rn)
                            {
                                break;
                            }
                            rv = re.Current;
                            comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                        }
                    }
                    if (HasDirefference(lv, rv))
                    {
                        result.Add(lv, rv);
                    }

                    comp = lv.Path.ToString().CompareTo(rv.Path.ToString());

                    ln = le.MoveNext();
                    rn = re.MoveNext();
                }
                if (ln)
                {
                    result.Add(le.Current, null);
                    while (le.MoveNext())
                    {
                        result.Add(le.Current, null);
                    }
                }
                if (rn)
                {
                    result.Add(null, re.Current);
                    while (re.MoveNext())
                    {
                        result.Add(null, re.Current);
                    }
                }
            }
            context.CurrentTask.Next(result);
        }

        private bool HasDirefference(DocumentNode local, DocumentNode remote)
        {
            if (local.FileSize == remote.FileSize
                && local.Hash == remote.Hash)
            {
                return false;
            }
            return true;
        }
    }
}