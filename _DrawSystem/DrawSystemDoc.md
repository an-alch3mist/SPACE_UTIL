# To Draw Obj In Runtime(works both in editor/build since it uses real mesh verts/tris)

## Reason For Creation:
	- traditional Debug.DrawLine/Gizomos requires a certain kinda approach to draw, which comes with caviates:
		0. should draw every single frame or set a Time duration which draw and dissapear after
		1. is'nt visible in final build (say gotta draw certain prototypes/visualize that involve lines ray etc)

	The Line.create().... // uses almost same performance as traditional Debug.DrawLine or Gizmos but visible in viewPort in editor and final build.

## Line
* used as follows:
```cs
// at any instant
void Start()
{
	Line.create(id: "unique").setA(a).setB(b).setCol(color).setE(1e-1);
}

// in update loop
void Update()
{
	Line.create(id: "uniqueA").setA(a).setB(b).setCol(color).setE(1e-1); // still there shall be just one line
	Line.create(id: "uniqueB").setA(a).setN(n).setCol(color).setE(1e-1); // still there shall be just one line
}
```

## Cube(a simple mesh cube with diffuse mat(builtinRender/Urp Lit) depending on kind of project)
* // TODO
* 