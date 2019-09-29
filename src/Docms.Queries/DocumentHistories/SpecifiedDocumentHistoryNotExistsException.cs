using System;

namespace Docms.Queries.DocumentHistories
{
    public class SpecifiedDocumentHistoryNotExistsException : Exception
    {
        public SpecifiedDocumentHistoryNotExistsException(string message) : base(message)
        {
        }
    }
}
