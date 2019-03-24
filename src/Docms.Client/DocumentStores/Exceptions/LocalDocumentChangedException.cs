using Docms.Client.Documents;
using Docms.Client.Types;
using System;

namespace Docms.Client.DocumentStores
{
    [Serializable]
    internal class LocalDocumentChangedException : Exception
    {
        public PathString Path { get; }
        public DocumentNode Document { get; }

        public LocalDocumentChangedException(PathString path, DocumentNode document) : base($"ローカルファイル{path}が変更されています。")
        {
            Path = path;
            Document = document;
        }
    }
}