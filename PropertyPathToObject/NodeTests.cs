using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PropertyPathToObject {
    [TestClass]
    public class NodeTests {
        [TestMethod]
        public void A() {
            var node = Node.From(new[] { "A" });
            Assert.AreEqual(1, node.Count());
            Assert.AreEqual("A", node.Single().Name);
            Assert.AreEqual(0, node.Single().SubNodes.Count());
        }

        [TestMethod]
        public void B() {
            var node = Node.From(new[] { "B" });
            Assert.AreEqual(1, node.Count());
            Assert.AreEqual("B", node.Single().Name);
            Assert.AreEqual(0, node.Single().SubNodes.Count());
        }

        [TestMethod]
        public void AB() {
            var node = Node.From(new[] { "A.B" });
            Assert.AreEqual(1, node.Count());
            Assert.AreEqual("A", node.Single().Name);
            Assert.AreEqual(1, node.Single().SubNodes.Count());
            Assert.AreEqual("B", node.Single().SubNodes.Single().Name);
        }

        [TestMethod]
        public void ABC() {
            var node = Node.From(new[] { "A.B.C" });
            Assert.AreEqual(1, node.Count());
            Assert.AreEqual("A", node.Single().Name);
            Assert.AreEqual(1, node.Single().SubNodes.Count());
            Assert.AreEqual("B", node.Single().SubNodes.Single().Name);
            Assert.AreEqual(1, node.Single().SubNodes.Single().SubNodes.Count());
            Assert.AreEqual("C", node.Single().SubNodes.Single().SubNodes.Single().Name);
        }

        [TestMethod]
        public void ABandCD() {
            var node = Node.From(new[] { "A.B", "C.D" });
            Assert.AreEqual(2, node.Count());
            Assert.AreEqual("A", node.ElementAt(0).Name);
            Assert.AreEqual(1, node.ElementAt(0).SubNodes.Count());
            Assert.AreEqual("B", node.ElementAt(0).SubNodes.Single().Name);
            Assert.AreEqual("C", node.ElementAt(1).Name);
            Assert.AreEqual(1, node.ElementAt(1).SubNodes.Count());
            Assert.AreEqual("D", node.ElementAt(1).SubNodes.Single().Name);
        }

        [TestMethod]
        public void ABandAC() {
            var node = Node.From(new[] { "A.B", "A.C" });
            Assert.AreEqual(1, node.Count());
            Assert.AreEqual("A", node.Single().Name);
            Assert.AreEqual(2, node.Single().SubNodes.Count());
            Assert.AreEqual("B", node.Single().SubNodes.ElementAt(0).Name);
            Assert.AreEqual("C", node.Single().SubNodes.ElementAt(1).Name);
            Debug.WriteLine(Node.FormatNodesAsJson(node));
        }

        [TestMethod]
        public void Empty() {
            var node = Node.From(new[] { "" });
            Assert.AreEqual(0, node.Count());
        }
    }

    public class Node {
        public static IEnumerable<Node> From(IEnumerable<string> propertyPaths) =>
            From(propertyPaths.Select(s => s.Split('.', StringSplitOptions.RemoveEmptyEntries)));

        private static IEnumerable<Node> From(IEnumerable<IEnumerable<string>> propertyPaths) {
            foreach (var path in propertyPaths.Where(p => p.Any()).GroupBy(p => p.First())) {
                yield return new Node(path.Key, path.Select(p => p.Skip(1)));
            }
        }

        private Node(string name, IEnumerable<IEnumerable<string>> subNodes) {
            Name = name;
            SubNodes = From(subNodes);
        }

        public string Name { get; private set; }
        public IEnumerable<Node> SubNodes { get; private set; }

        public override string ToString() => FormatNodesAsJson(new[] { this });
        public static string FormatNodesAsJson(IEnumerable<Node> nodes) => FormatNodesAsJson(nodes, 1);
        private static string FormatNodesAsJson(IEnumerable<Node> nodes, int level) {
            string padding(int extra) => new string('\t', level + extra);
            if (!nodes.Any())
                return "\"\"";
            return $"{{\n{padding(0)}{string.Join($", \n{padding(0)}", nodes.Select(n => FormatNodeAsJson(n, level)))}\n{padding(-1)}}}";
        }

        private static string FormatNodeAsJson(Node n, int level) => $"\"{n.Name}\": {FormatNodesAsJson(n.SubNodes, level + 1)}";
    }
}