using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using SPACE_GRAPH_VIEW;

namespace SPACE_NodeSystem
{
	/* 
		Analyze Flow: 
			Take The Data (LOG.SaveLog or Visual Graph)
			Analyze flow of Script Manual to spot the incorrect.
	*/
	public class NodeManager : MonoBehaviour
	{
		[TextArea(minLines: 10, maxLines: 20)]
		[SerializeField] string _nodeRelation = @"start -> 0
0 -> 1
1 -> A
A -> B
B -> C
C -> 0";

		private void Awake()
		{
			Debug.Log("Awake(): " + this);
		}

		private void Update()
		{
			if(INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}

		IEnumerator STIMULATE()
		{
			#region frame_rate
			QualitySettings.vSyncCount = 3; // 20fps
			yield return null;
			#endregion

			LOG.H('='.repeat(100));
			// Gather + TOPO >>
			Gather();
			LOG.H("GATHER"); LOG.AddLog(MAP_NodeReg.Values.ToTable()); LOG.HEnd("GATHER");

			#region GraphNodeManager
			/*
			Called As: 
			|
				GraphNodeManager.InitClear();
				foreach (Node node in MAP_NodeReg.Values)
					GraphNodeManager.Instance.CreateGraphNodeId(node.id, node.name);
				foreach (Node node in MAP_NodeReg.Values)
					GraphNodeManager.Instance.InitRelationAtNodeId(
						node.id, 
						node.INP.map(_node => _node.id).ToList(),
						node.OUT.map(_node => _node.id).ToList(),
					);
			*/

			GraphNodeManager.InitClear();
			// initialize graph node
			foreach (Node node in MAP_NodeReg.Values)
				GraphNodeManager.Instance.CreateGraphNodeId(node.id, node.name);
			// initialize relation mapping
			foreach (Node node in MAP_NodeReg.Values)
				GraphNodeManager.Instance.InitRelationAtNodeId(
					node.id,
					node.INP.map(_node => _node.id).ToList(),
					node.OUT.map(_node => _node.id).ToList()
				);
			#endregion

			LOG.H("TOPO");
			foreach (var topology in Topology.GetAllTopology(this.MAP_NodeReg))
			{
				LOG.AddLog(topology.Region.ToTable(toString: false, name: "REGION<>"));
				LOG.AddLog(topology.Sorted.ToTable(toString: false, name: "Sorted<>"));
			}
			LOG.HEnd("TOPO");
			// << Gather + TOPO
			LOG.HEnd('='.repeat(100));

			// check >>
			/* 
				LOG.SaveLog(Topology.GetAllRegion(this.MAP_NodeReg)[1].ToTable(name: "region>>"));
				LOG.SaveLog(Topology.GetSortedRegionTopology(Topology.GetAllRegion(this.MAP_NodeReg)[1], this.MAP_NodeReg).ToTable(name: "sorted>>"));
			*/
			// << check

		}

		Dictionary<int, Node> MAP_NodeReg; // Main Node Store
		enum GameDataType
		{
			GameData
		}
		void Gather()
		{
			// parse IN here >>
			MAP_NodeReg = new Dictionary<int, Node>();
			string IN = this._nodeRelation;

			// ad >>
			if (IN.Length == 0)
			{
				Debug.Log("node relation is empty".colorTag("yellow"));
				return;
			}
			// << ad

			// initialize unique node
			IN.clean().split(@"\n").forEach(line =>
			{
				var ID = line.split(@" \-\> ");
				ID.forEach(id =>
				{
					if (MAP_NodeReg.Values.findIndex(node => node.name == id) == -1)
					{
						Node node = new Node(name: id);
						MAP_NodeReg[node.id] = node;
					}
				});
			});

			// node.INP, node.OUT
			IN.split(@"\n").forEach(line =>
			{
				var ID = line.split(@" \-\> "); // split pattern
				Node node_a = MAP_NodeReg.Values.find(node => node.name == ID[0]),
					 node_b = MAP_NodeReg.Values.find(node => node.name == ID[1]);
				node_a.OUT.Add(node_b);
				node_b.INP.Add(node_a);
			});
			// << parse IN here
		}
	}

	// Topology.GetAllTopology(Dictionary<int, Node> MAP_NodeReg) => List<Topology>
	#region Topology
	public class Topology
	{
		public int id;
		static int a_inc = 0;

		public List<Node> Region;
		public List<Node> Sorted;

		public Topology()
		{
			this.id = a_inc;
			a_inc += 1;
		}

		public static List<Topology> GetAllTopology(Dictionary<int, Node> MAP_NodeReg)
		{
			List<Topology> TOPO = new List<Topology>();

			foreach (var region in GetAllRegion(MAP_NodeReg))
			{
				Topology topology = new Topology()
				{
					Region = region,
					Sorted = GetSortedRegionTopology(region, MAP_NodeReg),
				};
				TOPO.Add(topology);
			}

			return TOPO;
		}

		// checked
		static List<List<Node>> GetAllRegion(Dictionary<int, Node> MAP_NodeReg)
		{
			#region example data
			/*
			example: NODE Data
				A -> B
				A -> C
				A -> D
				D -> B
				B -> C
				X -> Z
				Z -> Y
			*/
			#endregion
			#region init MAP_exploredReg, node explored status 
			// init node explored status >>
			var MAP_exploredReg = new Dictionary<int, bool>();
			foreach (Node node in MAP_NodeReg.Values)
				MAP_exploredReg[node.id] = false;
			// << init node explored status 
			#endregion


			List<List<Node>> REGION = new List<List<Node>>();
			#region REGION
			ITER.reset();
			while (true)
			{
				if (ITER.iter_inc(1e4)) break;
				//
				Node start_node = null;
				foreach (var kvp in MAP_exploredReg)
					if (kvp.Value == false) // not explored
					{
						start_node = MAP_NodeReg[kvp.Key]; break;
					}

				// all found
				if (start_node == null) { /*Debug.Log("all REGION found: no NE(not visited) node found"); */ break; }

				#region flood fill region, explore + RED BLUE approach
				List<Node> RED = new List<Node>() { start_node };
				List<Node> BLUE = new List<Node>();
				//
				while (true)
				{
					if (ITER.iter_inc(1e4)) break;

					// region complete
					if (RED.Count == 0) break; // region complete
					Node node = RED[0]; RED.RemoveAt(0);

					// neighbour approach already explored, #done to resolve
					if (MAP_exploredReg[node.id] == true)
						continue;

					// explore node >>
					#region resolved
					// Debug.Log(" // " + node.name); /* order: A B C D D C  when explored check is not done before adding to BLUE */  
					#endregion
					MAP_exploredReg[node.id] = true;
					BLUE.Add(node);

					// neighbour NE(not explored) >>
					foreach (Node neighbour in node.INP)
						#region resolved
						//if (MAP_exploredReg[neighbour.id] == false) // check done before adding to BLUE, so that it wont repeat twice 
						#endregion
						RED.Add(neighbour);

					foreach (Node neighbour in node.OUT)
						RED.Add(neighbour);
					// neighbour NE(not explored)

					// << explore node
				}
				#endregion

				REGION.Add(BLUE);
			}
			#endregion

			return REGION;
		}

		// Artur approach
		// checked, unless Region, MAP_NodeReg == null
		// if no 0 Deg found at the begining: return empty list
		// if no 0 Deg found in middle return BLUE till than
		static List<Node> GetSortedRegionTopology(List<Node> Region, Dictionary<int, Node> MAP_NodeReg)
		{
			// node.id, degree
			Dictionary<int, int> MAP_DegReg = new Dictionary<int, int>();

			// down stream
			foreach (Node node in Region)
				MAP_DegReg[node.id] = node.OUT.Count;
			//LOG.SaveLog(MAP_DegReg.ToTable(name: "MAP_DegReg"));

			List<Node> RED = new List<Node>();
			foreach (var kvp in MAP_DegReg)
				if (kvp.Value == 0)
					RED.Add(MAP_NodeReg[kvp.Key]);

			if (RED.Count == 0) { Debug.Log("No 0 Deg Node Found"); return new List<Node>(); }
			List<Node> BLUE = new List<Node>();

			ITER.reset();
			while (true)
			{
				if (ITER.iter_inc(1e5)) break;

				// exit:  no red left
				if (RED.Count == 0)
				{
					if (BLUE.Count < Region.Count)
						Debug.Log("incomplete region: still more BLUE.Count < Region.Count");
					else
					{
						// all node explored BLUE.Count == Region.Count
						// Debug.Log("all node explored, sorted");
					}
					break;
				}

				Node node = RED[0]; RED.RemoveAt(0);
				BLUE.Add(node);

				// downstream, deduct the degree of INP
				foreach (Node inp in node.INP)
				{
					MAP_DegReg[inp.id] -= 1;
					if (MAP_DegReg[inp.id] == 0) // degree reached 0
						RED.Add(inp);
				}
			}
			return BLUE;
		}
	} 
	#endregion
	public class Node
	{
		public int id;
		public string name = "";
		public HashSet<Node> INP, OUT;
		public List<Item> Q;

		static int a_inc = 0; // make sure unique Id is provided each time, 0 to 2^32
		public Node(string name = "")
		{
			this.INP = new HashSet<Node>();
			this.OUT = new HashSet<Node>();
			this.Q = new List<Item>();

			this.id = a_inc; a_inc += 1;

			this.name = name;
		}
		public override int GetHashCode()
		{
			//return base.GetHashCode();
			return this.id;
		}
		public override bool Equals(object obj)
		{
			// Debug.Log("Equated");
			Node node = (Node)obj;
			if (node != null)
				return node.id == this.id;
			return false;
			/*return base.Equals(obj);*/
		}
		public override string ToString()
		{
			//return base.ToString()
			return $"id: {this.id} name: {this.name} INP:{this.INP.Count} OUT: {this.OUT.Count} Q: {this.Q.Count}";
		}

		// move the last gl(0) to OUT if possible, slide [0] to gl(1) if possible , acceptFromIN
		public void moveQ()
		{
			move__attempt_move_item_to_Successor();			// move__attempt_move_item_to_Successor
			move__attempt_slide_items_within();				// move__attempt_slide_items_within
			move__attempt_move_item_from_Predecessor();		// move__attempt_move_item_from_Predecessor
		}

		int lastOutIndex = 0;
		void move__attempt_move_item_to_Successor()
		{
			if (Q.Count == 0)
				return; // no Item Exist in Q

			float futureHeadDist = CN.getFmove(Q[0]);
			if(futureHeadDist >= 1f)
			{
				for (int i0 = 1; i0 <= OUT.Count; i0 += 1)
				{
					int index = (lastOutIndex + i0) % OUT.Count;
					Node succ = OUT.ElementAt(index);

					if (succ.Q.gl(0).dist - (futureHeadDist % 1f) >= CN.minSpacing)
					{
						// remove item from this node, move to successor node
						Item head = Q[0]; Q.RemoveAt(0);
						head.dist = futureHeadDist % 1f; // UpdateTransform(head);
						succ.Q.Add(head);

						this.lastOutIndex = index; // modify the lastOutIndex
						return;
					}
				}
			}

			// not moved
			this.Q[0].dist = C.clamp(futureHeadDist, CN.e, 1f - CN.e);
			// UpdateTransform(head);
		}

		void move__attempt_slide_items_within()
		{
			if (this.Q.Count <= 1)
				return; // either empty or single-already handled in move__attempt_move_to_Successor(), or Just Got Head Item

			for(int i0 = 1; i0 < Q.Count; i0 += 1)
			{
				if (CN.minSpacingExist(Q[i0 - 1], Q[i0]) == true)
					Q[i0].dist = CN.getFmove(Q[i0]);

				// UpdateTransform(Q[i0])
			}
		}

		int lastInpIndex = 0;
		void move__attempt_move_item_from_Predecessor()
		{
			for(int i0 = 1; i0 <= INP.Count; i0 += 1)
			{
				int index = (lastInpIndex + i0) % INP.Count;
				Node pred = this.INP.ToList()[index];

				if (pred.Q.Count == 0)
					continue;

				Item predHead = pred.Q[0];
				if (CN.getFmove(predHead) >= 1f)
				{
					// CanAccept >>
					bool CanAccept = false;
					if (this.Q.Count == 0)
						CanAccept = true;
					else
						CanAccept = CN.minSpacingExist(this.Q.gl(0), predHead);
					// << CanAccept
					if(CanAccept == true)
					{
						predHead = pred.Q[0]; pred.Q.RemoveAt(0);
						predHead.dist = CN.getFmove(predHead) % 1f; // UpdateTransform(predHead);
						this.Q.Add(predHead);

						this.lastInpIndex = index; // modify the lastInpIndex
						return;
					}
				}
			}

			// If none could send, do nothing this frame (and next frame we start from lastInpIndex + 1 again)
		}

		public static class CN // CONSTANT NEW
		{
			public static float e = 1f / 1000;
			public static float minSpacing = 0.3f;
			public static float speed = 1f;
			public static float getFmove(Item item) // get future move, without % 1f
			{
				return item.dist + CN.speed * Time.deltaTime;
			} 
			public static bool minSpacingExist(Item successor, Item curr) // with % 1f
			{
				return (successor.dist - CN.getFmove(curr) % 1f) >= CN.minSpacing;
			}
		}
		
	}
	public class Item
	{
		public float dist = 0f;
	}
}
