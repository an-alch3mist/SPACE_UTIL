# How To Get GraphView

0. Attach GraphNodeManager.cs to Empty GameObject

1. Called Externally As: 
```cs
	.InitClear();
		// initialize graph node
		foreach (Node node in MAP_NodeReg.Values)
			.Instance.CreateGraphNodeId(node.id, node.name); // instantiated inside C.PrefabHolder
		// initialize node to inp, node to out relation mapping
		foreach (Node node in MAP_NodeReg.Values)
			.Instance.InitRelationAtNodeId(
				node.id,
				node.INP.map(_node => _node.id).ToList(),
				node.OUT.map(_node => _node.id).ToList()
			);
```
2. `From Here On Everything Auto.`
`
	that is, UpdateDistribution is Called Each Frame inside GraphNodeManager Update() and GraphNode_IO.gameObject are Distribured, and Arrow from each node to its out_node is visualized  
`