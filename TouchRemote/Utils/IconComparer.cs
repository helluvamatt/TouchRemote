using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Utils
{
    internal class IconComparer : IEqualityComparer<FontAwesomeIcon>, IComparer<FontAwesomeIcon>
    {
        public int Compare(FontAwesomeIcon x, FontAwesomeIcon y)
        {
            if (x == FontAwesomeIcon.None) return -1;
            if (y == FontAwesomeIcon.None) return 1;
            return string.Compare(x.ToString(), y.ToString());
        }

        public bool Equals(FontAwesomeIcon x, FontAwesomeIcon y)
        {
            return string.Equals(x.ToString(), y.ToString());
        }

        public int GetHashCode(FontAwesomeIcon obj)
        {
            return obj.GetHashCode();
        }
    }
}
