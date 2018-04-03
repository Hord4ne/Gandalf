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
    public class ZielonkaAlgorithm
    {
        public ZielonkaResult DetermineWinningRegions(ParityGraph graph)
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
                result.SetPlayersRegions(satisfiedPlayer, result.GetPlayersRegions(satisfiedPlayer).Union(recursiveZielonkaForSatisfied.GetPlayersRegions(satisfiedPlayer)).ToList());
                result.SetPlayersRegions(notSatisfiedPlayer, new List<Node>());
            }
            else
            {
                var attractorOfNotSatisfiedVert = CalculateAttractor(recursiveZielonkaForSatisfied.GetPlayersRegions(notSatisfiedPlayer), graph, notSatisfiedPlayer);
                var recursiveZielonkaUnsatisfied = DetermineWinningRegions(graph.MakeSubGame(attractorOfNotSatisfiedVert));
                result.SetPlayersRegions(satisfiedPlayer, recursiveZielonkaUnsatisfied.GetPlayersRegions(satisfiedPlayer));
                result.SetPlayersRegions(notSatisfiedPlayer, recursiveZielonkaUnsatisfied.GetPlayersRegions(notSatisfiedPlayer).Union(attractorOfNotSatisfiedVert).ToList());
            }
            return result;
        }

        private HashSet<Node> CalculateAttractor(IEnumerable<Node> satisfingVert, ParityGraph graph, Player player)
        {
            var result = new HashSet<Node>();
            foreach (var vert in satisfingVert)
            {
                var partialRes = CalculateAttractorForSingleVert(vert, graph, player);
                result.Union(partialRes);
            }
            return result;
        }

        private HashSet<Node> CalculateAttractorForSingleVert(Node vert, ParityGraph graph, Player player)
        {
            throw new NotImplementedException();
        }

        public class ZielonkaResult
        {
            public List<Node> GoodWinningRegions { get; set; } = new List<Node>();
            public List<Node> BadWinningRegions { get; set; } = new List<Node>();
            public List<Node> GetPlayersRegions(Player player)
            {
                if (player == Player.Good)
                    return GoodWinningRegions;
                return BadWinningRegions;
            }
            public void SetPlayersRegions(Player player, List<Node> regions)
            {
                if (player == Player.Good)
                    GoodWinningRegions = regions;
                else
                    BadWinningRegions = regions;
            }
        }
    }

}
