using System.Collections.Generic;
using System.Linq;

namespace TouchRemote.Model.Persistence
{
    internal class InflateResult
    {
        private InflateResult()
        {
            _Errors = new List<string>();
        }

        private List<string> _Errors;

        public IEnumerable<string> Errors => _Errors;

        public bool Success => _Errors.Count == 0;

        public static InflateResult Default => new InflateResult();

        public static InflateResult WithError(string error)
        {
            return Default.WithAdditional(error);
        }

        public InflateResult WithAdditional(string error)
        {
            _Errors.Add(error);
            return this;
        }

        public InflateResult Append(string prefix, InflateResult other)
        {
            _Errors.AddRange(other.Errors.Select((cur) => prefix + cur));
            return this;
        }

        public InflateResult Append(InflateResult other)
        {
            _Errors.AddRange(other.Errors);
            return this;
        }
    }
}
