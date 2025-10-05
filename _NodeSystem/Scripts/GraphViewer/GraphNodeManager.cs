using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using SPACE_GRAPH_VIEW.SPACE_DirGraphNode; // subspace

namespace SPACE_GRAPH_VIEW
{
	/*
	Call From External As:
		.InitClear();
		foreach (Node node in MAP_NodeReg.Values)
			.Instance.CreateGraphNodeId(node.id, node.name);
		foreach (Node node in MAP_NodeReg.Values)
			.Instance.InitRelationAtNodeId(
				node.id, 
				node.INP.map(_node => _node.id).ToList(),
				node.OUT.map(_node => _node.id).ToList(),
			);
	*/

	public class GraphNodeManager : MonoBehaviour
	{
		#region README
		[TextArea(3, 10)]
		[SerializeField] string README = $@"0. Attach {typeof(GraphNodeManager).Name} to Empty Obj
1. Assets/LOG/GameData.txt Have Node Relation in seprated by ->
2. pfGraphNode is Ref.prefab or Loaded Auto From Resources/.prefab
(note that all fields are config @scale 1f, 20fps)";
		#endregion

		public static GraphNodeManager Instance; // Instance Approach: Movements Depend On SerilizedFields
		private void Awake()
		{
			Debug.Log("Awake(): " + this);

			pfGraphNode = Resources.Load<GameObject>(nameof(pfGraphNode));
			if (this.pfGraphNode == null)
				Debug.LogError(this.ToString() + ": pfGraphNode is null in");
			GraphNodeManager.Instance = this;
			MAP_GraphNodeReg = new Dictionary<int, GraphNode_IO>();
			// check >>
			//Distribute_initialize();
			// << check
		}
		
		private void Update()
		{
			// Distribute every frame
			UpdateDistribute();

			DRAW_prev.dt = Time.deltaTime;
			foreach (GraphNode_IO node in MAP_GraphNodeReg.Values)
				node.draw();
		}

		// done external >>
		#region done external
		/*
		TODO Call From External As: 
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


		static Dictionary<int, GraphNode_IO> MAP_GraphNodeReg;
		// clear game objects and initialize MAP_Reg
		// can be static
		public static void InitClear()
		{
			if (MAP_GraphNodeReg != null)
			{
				foreach (var Obj in MAP_GraphNodeReg.Values.map(val => val.gameObject))
					GameObject.Destroy(Obj);
			}
			MAP_GraphNodeReg = new Dictionary<int, GraphNode_IO>();
		}

		// can't be static: depends on [SerielizeField]
		public void CreateGraphNodeId(int id, string name)
		{
			GameObject GNode_Obj = GameObject.Instantiate(this.pfGraphNode, C.PrefabHolder);

			// random possition within initial_dist_from_worldCenter radius
			GNode_Obj.transform.position =
				new Vector2(
					Random.Range(-1000, 1000),
					Random.Range(-1000, 1000)).normalized * Random.Range(1, 1000) / 1000f * initial_dist_from_worldCenter;

			GraphNode_IO GNode = GNode_Obj.GetComponent<GraphNode_IO>();
			GNode.Init(id, name); // similar to node constructor initialize

			MAP_GraphNodeReg[id] = GNode; 
		}

		/// <summary>
		/// <typeparam name="INP_refId"> INP_ref: all the node.id refered in .INP</typeparam>,
		/// <typeparam name="OUT_refId"> OUT_ref: all the node.id refered in .OUT</typeparam>
		/// </summary>
		// can't be static: depends on [SerielizeField]
		public void InitRelationAtNodeId(int id, List<int> INP_refId, List<int> OUT_refId)
		{
			GraphNode_IO node = MAP_GraphNodeReg[id];
			// map INP, OUT relation to node at id
			foreach (GraphNode_IO _inp in INP_refId.map(_id => MAP_GraphNodeReg[_id]))
				node.INP.Add(_inp);
			foreach (GraphNode_IO _out in OUT_refId.map(_id => MAP_GraphNodeReg[_id]))
				node.OUT.Add(_out);
		}

		#endregion
		// << done external

		[Header("prefab Foreach GraphNode")]
		[SerializeField] GameObject pfGraphNode;
		[Header("vals considered for scale of 1f")]
		[SerializeField] float initial_dist_from_worldCenter = 5f;    // How strongly nodes repel each other
		[SerializeField] int init_iter_UpdateDistribute = 10;   // number of iter done at starting after InitRelation
		[Tooltip("Higher The repulsionStrength, Faster the Regain to Stable Alignment, Since It Shall Be Pushed By Non Cluster Nodes Too")]
		[SerializeField] float repulsionStrength = 0.07f;    // How strongly nodes repel each other
		[SerializeField] float attractionStrength = 0.03f;  // How strongly connected nodes attract each other
		[Tooltip("Higher The Dampening Factor, Faster the Regain to Stable Alignment")]
		[SerializeField] float dampingFactor = 4f;         // Damping to prevent oscillation
		[SerializeField] float minDistance = 1.5f;           // Minimum distance between nodes
		[SerializeField] float maxMovement = 1f;          // Maximum movement per frame
		[SerializeField] float gravityStrength = 1f;
		[Tooltip("Make Sure Bound Size More Than initial_dist_from_center")]
		[SerializeField] Vector2 boundMin = new Vector2(-7, -7);
		[Tooltip("Make Sure Bound Size More Than initial_dist_from_center")]
		[SerializeField] Vector2 boundMax = new Vector2(+7, +7);
		//
		// should be non static since the Ref from serielize field is used
		void UpdateDistribute()
		{
			var NODE = MAP_GraphNodeReg.Values.map(val => val).ToList();
			// Force-directed foreach GraphNode
			Dictionary<GraphNode_IO, Vector2> forces = new Dictionary<GraphNode_IO, Vector2>();

			// 0. Initialize forces for all nodes
			foreach (GraphNode_IO node in NODE)
				forces[node] = Vector2.zero;

			float dt = Time.deltaTime * 20;
			#region accumulate and apply forces 1, 2, 3(if too close)
			// 1. Calculate repulsion forces (nodes repel each other)
			for (int i = 0; i < NODE.Count; i++)
			{
				GraphNode_IO nodeA = NODE[i];

				for (int j = i + 1; j < NODE.Count; j++)
				{
					GraphNode_IO nodeB = NODE[j];

					Vector2 direction = (Vector2)(nodeA.transform.position - nodeB.transform.position);
					float distance = direction.magnitude;

					// Avoid division by zero or very small values
					if (distance < 0.001f)
					{
						direction = Random.insideUnitCircle.normalized * 0.001f;
						distance = 0.001f;
					}

					// Calculate repulsion force (inverse square law)
					float repulsionForce = repulsionStrength / (distance * distance);

					// Apply the force to both nodes in opposite directions
					Vector2 forceVector = direction.normalized * repulsionForce;
					forces[nodeA] += forceVector;
					forces[nodeB] -= forceVector;
				}
			}

			// 1. Calculate attraction forces (connected nodes attract each other)
			foreach (GraphNode_IO node in NODE)
			{
				// Attraction force for nodes in the IN list
				foreach (GraphNode_IO connectedNode in node.INP)
				{
					Vector2 direction = (Vector2)(connectedNode.transform.position - node.transform.position);
					float distance = direction.magnitude;

					// Calculate attraction force (linear with distance)
					float attractionForce = attractionStrength * distance;

					// Apply the force to the current node toward the connected node
					Vector2 forceVector = direction.normalized * attractionForce;
					forces[node] += forceVector;
				}

				// Attraction force for nodes in the OUT list
				foreach (GraphNode_IO connectedNode in node.OUT)
				{
					Vector2 direction = (Vector2)(connectedNode.transform.position - node.transform.position);
					float distance = direction.magnitude;

					// Calculate attraction force (linear with distance)
					float attractionForce = attractionStrength * distance;

					// Apply the force to the current node toward the connected node
					Vector2 forceVector = direction.normalized * attractionForce;
					forces[node] += forceVector;
				}
			}

			// 2. Apply the calculated forces to move each node
			foreach (GraphNode_IO node in NODE)
			{
				Vector2 force = forces[node];

				// Apply damping to prevent oscillation
				force *= dampingFactor;

				// Limit maximum movement per frame
				if (force.magnitude > maxMovement)
				{
					force = force.normalized * maxMovement;
				}

				// Update node position
				Vector3 newPosition = node.transform.position + new Vector3(force.x, force.y, 0) * dt;

				// Apply the new position
				node.transform.position = newPosition;
			}

			// 3. Optional: Check if any nodes are too close and push them apart
			for (int i = 0; i < NODE.Count; i++)
			{
				GraphNode_IO nodeA = NODE[i];

				for (int j = i + 1; j < NODE.Count; j++)
				{
					GraphNode_IO nodeB = NODE[j];

					Vector2 direction = (Vector2)(nodeA.transform.position - nodeB.transform.position);
					float distance = direction.magnitude;

					if (distance < minDistance)
					{
						// Push nodes apart to maintain minimum distance
						Vector2 pushVector = direction.normalized * (minDistance - distance) * 0.5f;
						nodeA.transform.position += new Vector3(pushVector.x, pushVector.y, 0) * dt; 
						nodeB.transform.position -= new Vector3(pushVector.x, pushVector.y, 0) * dt;
					}
				}
			} 
			#endregion

			// 4. limit to bounds
			foreach(GraphNode_IO node in NODE)
				node.transform.position = new Vector2()
				{
					x = C.clamp(node.transform.position.x, boundMin.x, boundMax.x),
					y = C.clamp(node.transform.position.y, boundMin.y, boundMax.y),
				};
		}
	}
}
