using NUnit.Framework;
using ReasoningEngine;
using System;

namespace ReasoningEngineTests
{
    [TestFixture]
    public class NodeTypeTests
    {
        [Test]
        public void TestSIMONodeCreation()
        {
            var simoNode = new SIMONode(1, "Test SIMO Node", DomainInterpretation.Truth);
            
            Assert.That(simoNode, Is.Not.Null);
            Assert.That(simoNode.Type, Is.EqualTo(NodeType.SIMO));
            Assert.That(simoNode.Id, Is.EqualTo(1));
            Assert.That(simoNode.Content, Is.EqualTo("Test SIMO Node"));
            Assert.That(simoNode.Interpretation, Is.EqualTo(DomainInterpretation.Truth));
            Assert.That(simoNode.Distribution, Is.Not.Null);
        }

        [Test]
        public void TestSIMONodeDistributionAddition_Truth()
        {
            var simoNode = new SIMONode(1, "Truth Node", DomainInterpretation.Truth);
            
            // Valid truth value
            Assert.DoesNotThrow(() => simoNode.AddDistributionPoint(0.5, 1.0));
            
            // Invalid truth values
            Assert.Throws<ArgumentException>(() => simoNode.AddDistributionPoint(-0.1, 1.0));
            Assert.Throws<ArgumentException>(() => simoNode.AddDistributionPoint(1.1, 1.0));
        }

        [Test]
        public void TestSIMONodeDistributionAddition_DiscreteRange()
        {
            var simoNode = new SIMONode(1, "Discrete Node", DomainInterpretation.DiscreteRange);
            
            // Valid discrete values
            Assert.DoesNotThrow(() => simoNode.AddDistributionPoint(1.0, 0.5));
            Assert.DoesNotThrow(() => simoNode.AddDistributionPoint(2.0, 0.3));
            
            // Invalid non-integer value
            Assert.Throws<ArgumentException>(() => simoNode.AddDistributionPoint(1.5, 0.2));
        }

        [Test]
        public void TestSIMONodeDistributionAddition_ContinuousRange()
        {
            var simoNode = new SIMONode(1, "Continuous Node", DomainInterpretation.ContinuousRange);
            
            // Valid range
            Assert.DoesNotThrow(() => simoNode.AddDistributionRange(1.0, 2.0, 0.5));
            
            // Invalid ranges
            Assert.Throws<ArgumentException>(() => simoNode.AddDistributionRange(2.0, 1.0, 0.5)); // Upper bound less than lower bound
            Assert.Throws<InvalidOperationException>(() => simoNode.AddDistributionPoint(1.0, 0.5)); // Cannot use AddPoint for continuous range
        }

        [Test]
        public void TestMISONodeCreation()
        {
            var misoNode = new MISONode(1, "Test MISO Node");
            
            Assert.That(misoNode, Is.Not.Null);
            Assert.That(misoNode.Type, Is.EqualTo(NodeType.MISO));
            Assert.That(misoNode.Id, Is.EqualTo(1));
            Assert.That(misoNode.Content, Is.EqualTo("Test MISO Node"));
            Assert.That(misoNode.SingleOutputEdgeId, Is.Null);
        }

        [Test]
        public void TestMISONodeSingleOutputEdge()
        {
            var misoNode = new MISONode(1, "Test MISO Node");
            
            // Set initial edge
            Assert.DoesNotThrow(() => misoNode.SetSingleOutputEdge(100));
            Assert.That(misoNode.SingleOutputEdgeId, Is.EqualTo(100));
            
            // Attempt to set second edge without clearing
            Assert.Throws<InvalidOperationException>(() => misoNode.SetSingleOutputEdge(200));
            
            // Clear edge
            misoNode.ClearSingleOutputEdge();
            Assert.That(misoNode.SingleOutputEdgeId, Is.Null);
            
            // Set new edge after clearing
            Assert.DoesNotThrow(() => misoNode.SetSingleOutputEdge(200));
            Assert.That(misoNode.SingleOutputEdgeId, Is.EqualTo(200));
        }

        [Test]
        public void TestMISONodeEdgeChange()
        {
            var misoNode = new MISONode(1, "Test MISO Node");
            
            // Change when no edge exists
            misoNode.ChangeSingleOutputEdge(100);
            Assert.That(misoNode.SingleOutputEdgeId, Is.EqualTo(100));
            
            // Change existing edge
            misoNode.ChangeSingleOutputEdge(200);
            Assert.That(misoNode.SingleOutputEdgeId, Is.EqualTo(200));
        }

        [Test]
        public void TestNodeTypeInheritance()
        {
            var simoNode = new SIMONode(1, "SIMO Node", DomainInterpretation.Truth);
            var misoNode = new MISONode(2, "MISO Node");
            
            Assert.That(simoNode, Is.InstanceOf<Node>());
            Assert.That(simoNode, Is.InstanceOf<NodeBase>());
            Assert.That(misoNode, Is.InstanceOf<Node>());
            Assert.That(misoNode, Is.InstanceOf<NodeBase>());
        }

        [Test]
        public void TestVersionCompatibility()
        {
            var simoNode = new SIMONode(1, "SIMO Node", DomainInterpretation.Truth);
            var misoNode = new MISONode(2, "MISO Node");
            
            Assert.That(simoNode.Version, Is.EqualTo(2)); // Should inherit Node's version
            Assert.That(misoNode.Version, Is.EqualTo(2)); // Should inherit Node's version
        }
    }
}