using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Docms.Client.Types;

namespace Docms.Client.FileSystem
{
    public class IgnoreFilePatterns : List<string>
    {
        public static readonly IgnoreFilePatterns Default = new IgnoreFilePatterns() { "THUMBS.DB", ".DOCMS", ".TMP" };
        public bool IsMatch(PathString path)
        {
            return this.Any(pattern => Regex.IsMatch(path.ToString().ToUpperInvariant(), pattern));
        }
    }
}