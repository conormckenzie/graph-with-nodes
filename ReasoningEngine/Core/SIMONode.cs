using System;

namespace ReasoningEngine
{
    public class SIMONode : Node
    {
        public ProbabilityDistribution Distribution { get; private set; }
        public DomainInterpretation Interpretation { get; private set; }

        public SIMONode(long id, string content, DomainInterpretation interpretation) 
            : base(id, content, NodeType.SIMO)
        {
            Interpretation = interpretation;
            Distribution = new ProbabilityDistribution(MapInterpretationToDomainType(interpretation));
        }

        private DomainType MapInterpretationToDomainType(DomainInterpretation interpretation)
        {
            return interpretation switch
            {
                DomainInterpretation.Truth => DomainType.Truth,
                DomainInterpretation.ContinuousRange => DomainType.Continuous,
                DomainInterpretation.DiscreteRange => DomainType.DiscreteInteger,
                _ => throw new ArgumentException("Invalid interpretation")
            };
        }

        public void AddDistributionPoint(double value, double probability)
        {
            Distribution.AddPoint(value, probability);
        }

        public void AddDistributionRange(double lowerBound, double upperBound, double probability)
        {
            Distribution.AddRange(lowerBound, upperBound, probability);
        }
    }
}