using System;
using System.Collections.Generic;

namespace Docms.Infrastructure.WebDav.DataModel
{
    public class PropertyName : IEquatable<PropertyName>
    {
        private readonly string _ns;
        private readonly string _localName;

        public PropertyName(string ns, string localName)
        {
            _ns = ns;
            _localName = localName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyName);
        }

        public bool Equals(PropertyName other)
        {
            return other != null &&
                   _ns == other._ns &&
                   _localName == other._localName;
        }

        public override int GetHashCode()
        {
            var hashCode = 920928167;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_ns);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_localName);
            return hashCode;
        }
    }
}
