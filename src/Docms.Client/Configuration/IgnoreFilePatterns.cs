using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Docms.Client.Types;

namespace Docms.Client.Configurations
{
    public class IgnoreFilePatterns : List<string>
    {
        public static readonly IgnoreFilePatterns Default = new IgnoreFilePatterns() { "Thumbs.db" };
        public bool IsMatch(PathString path)
        {
            return this.Any(pattern => Regex.IsMatch(path.ToString(), pattern));
        }
    }
}