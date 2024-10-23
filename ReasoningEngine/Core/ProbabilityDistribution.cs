using System;
using System.Collections.Generic;
using System.Linq;

namespace ReasoningEngine
{
    public class ProbabilityDistribution
    {
        public DomainType DomainType { get; private set; }
        private List<(double LowerBound, double UpperBound, double Probability)> Distribution { get; set; }
        private const double EPSILON = 1e-10; // For floating point comparisons

        public ProbabilityDistribution(DomainType domainType)
        {
            DomainType = domainType;
            Distribution = new List<(double, double, double)>();
        }

        public void AddPoint(double value, double probability)
        {
            if (probability < 0 || probability > 1)
                throw new ArgumentException("Probability must be between 0 and 1");

            if (DomainType == DomainType.DiscreteInteger && !IsInteger(value))
                throw new ArgumentException("Value must be an integer for DiscreteInteger domain");
            
            if (DomainType == DomainType.Truth && (value < 0 || value > 1))
                throw new ArgumentException("Value must be between 0 and 1 for Truth domain");

            if (DomainType == DomainType.DiscreteInteger || DomainType == DomainType.Truth)
            {
                // Check for duplicate points
                if (Distribution.Any(d => Math.Abs(d.LowerBound - value) < EPSILON))
                    throw new InvalidOperationException($"A probability is already defined for value {value}");

                Distribution.Add((value, value, probability));
                ValidateTotalProbability();
            }
            else
            {
                throw new InvalidOperationException("Use AddRange for Continuous domain");
            }
        }

        public void AddRange(double lowerBound, double upperBound, double probability)
        {
            // Basic validation
            if (probability < 0 || probability > 1)
                throw new ArgumentException("Probability must be between 0 and 1");

            // Width check must happen first
            if (upperBound - lowerBound < 5 * EPSILON)
                throw new ArgumentException("Range width must be at least 5*EPSILON");

            // Domain-specific validation
            switch (DomainType)
            {
                case DomainType.Truth:
                    if (lowerBound < 0 || upperBound > 1)
                        throw new ArgumentException("Truth domain bounds must be within [0,1]");
                    break;
                case DomainType.DiscreteInteger:
                    throw new InvalidOperationException("AddRange is only valid for Truth or Continuous domains");
                case DomainType.Continuous:
                    break;
            }

            // Check for too-close ranges
            if (Distribution.Any(d => 
                Math.Abs(lowerBound - d.UpperBound) < EPSILON || 
                Math.Abs(upperBound - d.LowerBound) < EPSILON))
            {
                throw new InvalidOperationException("Range boundaries too close to existing range");
            }

            // Check for overlapping ranges
            if (Distribution.Any(d => 
                (lowerBound <= d.UpperBound + EPSILON && upperBound >= d.LowerBound - EPSILON)))
            {
                throw new InvalidOperationException("New range overlaps with existing range");
            }

            Distribution.Add((lowerBound, upperBound, probability));
            ValidateTotalProbability();
        }

        public bool IsComplete()
        {
            if (Distribution.Count == 0)
                return false;

            switch (DomainType)
            {
                case DomainType.Truth:
                    return Math.Abs(GetTotalProbability() - 1) < EPSILON;

                case DomainType.DiscreteInteger:
                    var values = Distribution.Select(d => d.LowerBound).OrderBy(v => v).ToList();
                    // Check if values are consecutive integers
                    for (int i = 1; i < values.Count; i++)
                    {
                        if (Math.Abs(values[i] - values[i - 1] - 1) > EPSILON)
                            return false;
                    }
                    return Math.Abs(GetTotalProbability() - 1) < EPSILON;

                case DomainType.Continuous:
                    // Sort ranges by lower bound
                    var ranges = Distribution.OrderBy(d => d.LowerBound).ToList();
                    // Check for gaps between ranges
                    for (int i = 1; i < ranges.Count; i++)
                    {
                        if (Math.Abs(ranges[i].LowerBound - ranges[i - 1].UpperBound) > EPSILON)
                            return false;
                    }
                    return Math.Abs(GetTotalProbability() - 1) < EPSILON;

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        public double GetProbability(double value)
        {
            switch (DomainType)
            {
                case DomainType.Truth:
                case DomainType.DiscreteInteger:
                    var point = Distribution.FirstOrDefault(d => Math.Abs(d.LowerBound - value) < EPSILON);
                    return point == default ? 0 : point.Probability;

                case DomainType.Continuous:
                    var range = Distribution.FirstOrDefault(d => 
                        value >= d.LowerBound - EPSILON && value <= d.UpperBound + EPSILON);
                    return range == default ? 0 : range.Probability;

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        public double GetProbability(double lowerBound, double upperBound)
        {
            if (lowerBound >= upperBound)
                throw new ArgumentException("Upper bound must be greater than lower bound");

            switch (DomainType)
            {
                case DomainType.Truth:
                case DomainType.DiscreteInteger:
                    return Distribution
                        .Where(d => d.LowerBound >= lowerBound - EPSILON && d.UpperBound <= upperBound + EPSILON)
                        .Sum(d => d.Probability);

                case DomainType.Continuous:
                    // For now, only handle cases where query range exactly matches stored ranges
                    // Could be extended to handle partial overlaps if needed
                    return Distribution
                        .Where(d => Math.Abs(d.LowerBound - lowerBound) < EPSILON && 
                                  Math.Abs(d.UpperBound - upperBound) < EPSILON)
                        .Sum(d => d.Probability);

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        public IReadOnlyList<(double LowerBound, double UpperBound, double Probability)> GetDistribution()
        {
            return Distribution.AsReadOnly();
        }

        private double GetTotalProbability()
        {
            return Distribution.Sum(d => d.Probability);
        }

        private void ValidateTotalProbability()
        {
            double total = GetTotalProbability();
            if (total > 1 + EPSILON)
                throw new InvalidOperationException("Total probability cannot exceed 1");
        }

        private bool IsInteger(double value) => Math.Abs(value % 1) < EPSILON;

        /// <summary>
        /// Returns a description of how the probability distribution is quantized.
        /// For discrete domains (Truth and DiscreteInteger), returns the set of points with non-zero probability.
        /// For continuous domains, returns the ranges over which the probability density is defined.
        /// The returned values are ordered by position on the number line.
        /// </summary>
        public IReadOnlyList<(double Start, double End)> GetQuantization()
        {
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound))
                .OrderBy(range => range.LowerBound)
                .ToList();

            return result.AsReadOnly();
        }

        public IReadOnlyList<(double Start, double End, double Probability)> GetQuantizationWithProbabilities()
        {
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound, d.Probability))
                .OrderBy(range => range.LowerBound)
                .ToList();

            return result.AsReadOnly();
        }

        /// <summary>
        /// Returns the index of the range containing the given point.
        /// For points that could belong to multiple ranges due to uncertainty,
        /// assigns the point to the range with the closest boundary.
        /// </summary>
        public int GetContainingRange(double point)
        {
            var possibleRanges = new List<(int Index, double Distance)>();

            // Check each range
            for (int i = 0; i < Distribution.Count; i++)
            {
                var range = Distribution[i];
                
                // Check if point is definitely in this range
                if (point + EPSILON >= range.LowerBound && point - EPSILON <= range.UpperBound)
                {
                    // Calculate distances to both bounds
                    double distToLower = Math.Abs(point - range.LowerBound);
                    double distToUpper = Math.Abs(point - range.UpperBound);
                    double minDist = Math.Min(distToLower, distToUpper);
                    
                    possibleRanges.Add((i, minDist));
                }
            }

            if (!possibleRanges.Any())
                throw new ArgumentException("Point is not contained in any range");

            // If point could be in multiple ranges, choose the one with the closest boundary
            // If distances are equal, prefer the lower index range
            return possibleRanges
                .OrderBy(r => r.Distance)
                .ThenBy(r => r.Index)
                .First().Index;
        }

        /// <summary>
        /// Returns a list of indices of ranges that might contain the given point,
        /// taking into account measurement uncertainty.
        /// </summary>
        public List<int> GetCoveringRanges(double point)
        {
            var coveringRanges = new List<int>();

            for (int i = 0; i < Distribution.Count; i++)
            {
                var range = Distribution[i];
                
                // Check if point might be in this range
                if (point - EPSILON <= range.UpperBound + EPSILON && 
                    point + EPSILON >= range.LowerBound - EPSILON)
                {
                    coveringRanges.Add(i);
                }
            }

            return coveringRanges;
        }
    }
}