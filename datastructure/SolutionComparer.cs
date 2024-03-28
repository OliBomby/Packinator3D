using System.Collections.Generic;
using System.Linq;

namespace Packinator3D.datastructure;

public class SolutionComparer : IEqualityComparer<Solution> {
    public bool Equals(Solution x, Solution y) {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        if (x.States.Count != y.States.Count) return false;
        for (var i = 0; i < x.States.Count; i++) {
            if (!x.States[i].Origin.Round().Equals(y.States[i].Origin.Round())) return false;
        }
        return true;
    }

    public int GetHashCode(Solution obj) {
        const int seed = 487;
        const int modifier = 31;

        unchecked
        {
            return obj.States.Aggregate(seed, (current, item) =>
                (current*modifier) + item.Origin.Round().GetHashCode());
        }
    }
}