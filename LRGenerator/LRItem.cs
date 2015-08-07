using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class LRItem
    {
        public ProductionRule Rule { get; }
        public int Marker { get; }
        public bool IsKernel { get; }
        public int Length { get { return Rule.Length; } }
        public HashSet<Terminal> Lookaheads { get; }

        public LRItem(ProductionRule rule, int marker, IEnumerable<Terminal> lookaheads = null, bool? isKernel = null)
        {
            Rule = rule;
            Marker = marker;
            IsKernel = isKernel ?? marker != 0;
            Lookaheads = new HashSet<Terminal>(lookaheads ?? Enumerable.Empty<Terminal>());
        }

        int? hash;
        public override int GetHashCode()
        {
            if (!hash.HasValue)
            {
                hash = 17;
                hash = 23 * hash + Marker;
                hash = 23 * hash + Rule.GetHashCode();
            }
            return hash.Value;
        }

        public override bool Equals(object obj)
        {
            var t = obj as LRItem;

            if (t == null)
                return false;

            if (t.Marker != Marker)
                return false;

            return (object.ReferenceEquals(Rule, t.Rule));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            for (var i = 0; i < Length; i++)
            {
                var marker = (i == Marker ? " *" : "");

                sb.Append($"{marker} {Rule.Symbols[i]}");
            }

            if (Marker >= Length)
                sb.Append(" *");

            if (Lookaheads.Count > 0)
            {
                var lookaheads = string.Join(", ", Lookaheads);
                sb.Append($"({lookaheads})");
            }

            return $"{Rule.Production} ->{sb}";
        }
    }
}
