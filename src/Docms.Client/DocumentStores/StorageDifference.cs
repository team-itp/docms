using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Docms.Client.DocumentStores
{
    public class StorageDifference
    {
        public StorageDifference(PathString path, DocumentNode storage1Document, DocumentNode storage2Document)
        {
            Path = path;
            Storage1Document = storage1Document;
            Storage2Document = storage2Document;
        }

        public PathString Path { get; }
        public DocumentNode Storage1Document { get; }
        public DocumentNode Storage2Document { get; }
    }
}
