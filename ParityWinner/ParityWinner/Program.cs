using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParityWinner
{
    public class Program
    {
        static void Main(string[] args)
        {
            var basicInfo = Console.ReadLine().Split(' ');
            var n0 = int.Parse(basicInfo[0]);
            var n1 = int.Parse(basicInfo[1]);
            var m = int.Parse(basicInfo[2]);
            var graph = new ParityGraph();
            for (var i = 0; i < n0; i++)
            {
                var vertexInfo = Console.ReadLine().Split(' ');
                graph.LoadVertexToGraph(int.Parse(vertexInfo[0]), int.Parse(vertexInfo[1]), Player.Good);
            }
            for (var i = 0; i < n1; i++)
            {
                var vertexInfo = Console.ReadLine().Split(' ');
                graph.LoadVertexToGraph(int.Parse(vertexInfo[0]), int.Parse(vertexInfo[1]), Player.Good);
            }
            for (var i = 0; i < m; i++)
            {
                var edgeInfo = Console.ReadLine().Split(' ');
                graph.AddEgde(int.Parse(edgeInfo[0]), int.Parse(edgeInfo[1]));
            }
            var startingIndex = int.Parse(Console.ReadLine());
            var startingNode = graph.NormalStructure[startingIndex].First();
            var result = ZielonkaAlgorithm.DetermineWinningRegions(graph);
            Console.WriteLine(result.GoodWinningRegions.Contains(startingNode) ? "0" : "1");
            Console.ReadKey();
        }
    }

    public enum Player { Good, Bad}

    public class Node
    {
        public Player Owner { get; set; }
        public int Index { get; set; }
        public int Color { get; set; }

        public override bool Equals(object obj)
        {
            var casted = obj as Node;
            if (casted == null)
                return false;
            return this.Index == casted.Index;
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }

    public class ParityGraph
    {
        public Dictionary<int,List<Node>> NormalStructure { get; set; }
        public Dictionary<int,List<Node>> ReversedStructure { get; set; }
        public int InitialVertex { get; set; }
        public ParityGraph()
        {
            this.NormalStructure = new Dictionary<int, List<Node>>();
            this.ReversedStructure = new Dictionary<int, List<Node>>();
        }

        public void LoadVertexToGraph(int index, int color, Player owner)
        {
            this.NormalStructure.Add(index, new List<Node> { new Node { Index = index, Color = color, Owner = owner } });
            this.ReversedStructure.Add(index, new List<Node> { new Node { Index = index, Color = color, Owner = owner } });
        }

        public void AddEgde(int from, int to)
        {
            this.NormalStructure[from].Add(this.NormalStructure[to].First());
            this.ReversedStructure[to].Add(this.ReversedStructure[from].First());
        }

        public bool IsEmpty()
        {
            return !this.NormalStructure.Keys.Any();
        }

        internal ParityGraph MakeSubGame(HashSet<Node> attractorOfSatisfingVert)
        {
            var result = new ParityGraph();
            var indexesToDel = new HashSet<int>(attractorOfSatisfingVert.Select(x => x.Index));
            var newNormalStructure = new Dictionary<int, List<Node>>();
            var newReversedStructure = new Dictionary<int, List<Node>>();
            foreach (var key in result.NormalStructure.Keys)
            {
                if (!indexesToDel.Contains(key)) {
                    newNormalStructure[key] = this.NormalStructure[key].Where(x => !attractorOfSatisfingVert.Contains(x)).ToList();
                    newReversedStructure[key] = this.ReversedStructure[key].Where(x => !attractorOfSatisfingVert.Contains(x)).ToList();
                }
            }
            result.NormalStructure = newNormalStructure;
            result.ReversedStructure = newReversedStructure;
            return result;
        }
    }
    public static class ZielonkaAlgorithm
    {
        public static ZielonkaResult DetermineWinningRegions(ParityGraph graph)
        {
            var result = new ZielonkaResult();
            if (graph.IsEmpty())
                return result;
            var vert = graph.NormalStructure.Values.Select(x => x.First());
            var maxColor = vert.Max(x => x.Color);
            var satisfiedPlayer = maxColor % 2 == 0 ? Player.Good : Player.Bad;
            var notSatisfiedPlayer = satisfiedPlayer == Player.Good ? Player.Bad : Player.Good;
            var satisfingVert = vert.Where(x => x.Color == maxColor);
            var attractorOfSatisfingVert = CalculateAttractor(satisfingVert, graph, satisfiedPlayer);
            var recursiveZielonkaForSatisfied = DetermineWinningRegions(graph.MakeSubGame(attractorOfSatisfingVert));
            if (!recursiveZielonkaForSatisfied.GetPlayersRegions(notSatisfiedPlayer).Any())
            {
                result.SetPlayersRegions(satisfiedPlayer, result.GetPlayersRegions(satisfiedPlayer).Union(recursiveZielonkaForSatisfied.GetPlayersRegions(satisfiedPlayer)));
                result.SetPlayersRegions(notSatisfiedPlayer, new List<Node>());
            }
            else
            {
                var attractorOfNotSatisfiedVert = CalculateAttractor(recursiveZielonkaForSatisfied.GetPlayersRegions(notSatisfiedPlayer), graph, notSatisfiedPlayer);
                var recursiveZielonkaUnsatisfied = DetermineWinningRegions(graph.MakeSubGame(attractorOfNotSatisfiedVert));
                result.SetPlayersRegions(satisfiedPlayer, recursiveZielonkaUnsatisfied.GetPlayersRegions(satisfiedPlayer));
                result.SetPlayersRegions(notSatisfiedPlayer, recursiveZielonkaUnsatisfied.GetPlayersRegions(notSatisfiedPlayer).Union(attractorOfNotSatisfiedVert));
            }
            return result;
        }

        private static HashSet<Node> CalculateAttractor(IEnumerable<Node> satisfingVert, ParityGraph graph, Player player)
        {
            var nodesInAttractor = new HashSet<Node>(satisfingVert);
            bool fixedPoint = false;
            while (!fixedPoint)
            {
                var size = nodesInAttractor.Count;
                var tmpAttractor = new HashSet<Node>(nodesInAttractor);
                foreach (var vert in tmpAttractor)
                {
                    var playersNeighbours = graph.ReversedStructure[vert.Index].Where(x => x.Owner == player);
                    var enemiesNieghbours = graph.ReversedStructure[vert.Index].Where(x => x.Owner != player);
                    //we add all of our nodes and the ones of our enemy that have no choice but to end in our attr vert
                    nodesInAttractor.UnionWith(playersNeighbours);
                    nodesInAttractor.UnionWith(enemiesNieghbours.Where(c=>graph.NormalStructure[c.Index].Skip(1).All(z => nodesInAttractor.Contains(z))));
                }
                fixedPoint = size == nodesInAttractor.Count;
            }
            return nodesInAttractor;
        }

        private static HashSet<Node> CalculateAttractorForSingleVert(Node vert, ParityGraph graph, Player player)
        {
            throw new NotImplementedException();
        }

        public class ZielonkaResult
        {
            public HashSet<Node> GoodWinningRegions { get; set; } = new HashSet<Node>();
            public HashSet<Node> BadWinningRegions { get; set; } = new HashSet<Node>();
            public HashSet<Node> GetPlayersRegions(Player player)
            {
                if (player == Player.Good)
                    return GoodWinningRegions;
                return BadWinningRegions;
            }
            public void SetPlayersRegions(Player player, IEnumerable<Node> regions)
            {
                if (player == Player.Good)
                    GoodWinningRegions = new HashSet<Node>(regions);
                else
                    BadWinningRegions = new HashSet<Node>(regions);
            }
        }
    }

}
