Here's a complete specification for the probability distribution system:

Core Concepts:
1. Measurement Uncertainty: Every numeric value has ±EPSILON uncertainty (EPSILON = 1e-10)
2. Ranges: Closed intervals [a,b] where both bounds have ±EPSILON uncertainty
3. Minimum Range Width: All ranges must be at least 5*EPSILON wide to ensure clear separation
4. Domain Types:
   - Continuous: Uses ranges
   - DiscreteInteger: Uses single points that must be integers (within EPSILON)
   - Truth: Uses ranges or points, all values must be in [0,1]

Required Operations:
1. AddPoint(value, probability):
   - For DiscreteInteger and Truth domains only
   - Value must satisfy domain constraints
   - No duplicate points (within EPSILON)
   - Probability must be in [0,1]

2. AddRange(lowerBound, upperBound, probability):
   - For Continuous and Truth domains only
   - Range must be at least 5*EPSILON wide
   - Must satisfy domain constraints
   - No overlaps with existing ranges
   - Adjacent ranges must have small gap (> EPSILON) to prevent ambiguity
   - Probability must be in [0,1]

3. GetProbability(point):
   - Returns probability for the given point
   - For discrete domains: exact match within EPSILON
   - For continuous domains: point must be within a range (considering EPSILON)

4. GetProbability(lowerBound, upperBound):
   - Returns probability for the given range
   - For discrete domains: sum of probabilities of contained points
   - For continuous domains: sum of probabilities of overlapping ranges

5. GetContainingRange(point):
   - Returns index of range containing the point
   - If point could belong to multiple ranges due to uncertainty, choose range with closest boundary
   - If distances are equal, prefer lower index range
   - Throw if point not in any range

6. IsComplete():
   - Verifies distribution is properly defined
   - All probabilities must sum to 1 (within EPSILON)
   - For discrete domains: no gaps in integer sequence
   - For continuous domains: gaps between ranges must be small enough

7. GetQuantization():
   - Returns how the distribution is divided into ranges
   - For discrete domains: list of points
   - For continuous domains: list of ranges
   - Results ordered by position on number line

Properties/Invariants:
1. Total probability must never exceed 1 (within EPSILON)
2. Ranges must not overlap (considering EPSILON uncertainty)
3. Adjacent ranges must have small gap to prevent ambiguity
4. Points/ranges must satisfy domain type constraints
5. Every valid input point should belong to exactly one range/point (when possible)
6. All numeric comparisons must account for ±EPSILON uncertainty
