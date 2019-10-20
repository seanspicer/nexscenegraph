# NexSceneGraph

##### October 2019

NexSceneGraph is approaching a first set of milestone releases.   Currently we are working on the core API and exposing higher-level concepts so that developers new to 3D programming don't necessarily have to start by learning vertex layouts, shaders, pipelines, pipeline states, etc.   The current 0.2.x API makes it simple to create a shape such as a box, and apply PhongLighting (against per-face normals)

```C#
using System.Numerics;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph.MyFirstCube
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a basic shape
            var cubeShape = Box.Create(Vector3.Zero, 0.5f*Vector3.One);
            
            // Tessellate the shape into a drawable
            var hints = TessellationHints.Create();
            hints.NormalsType = NormalsType.PerFace;
            hints.ColorsType = ColorsType.ColorOverall;

            var cubeDrawable = 
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cubeShape, 
                    hints, 
                    new Vector3[] {Vector3.UnitX});
            
            // Create a material
            var cubeMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    50f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                false);

            // Assign the material by attaching the pipeline state
            // to the drawable.  This could also be done for any Geode
            cubeDrawable.PipelineState = cubeMaterial.CreatePipelineState();
            
            // Create an SDL Viewer
            var viewer = SimpleViewer.Create("My First Cube!");
            
            // Create a trackball manipulator and assign it to the viewer
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            // Create a geode and add the drawable to it.
            var cubeGeode = Geode.Create();
            cubeGeode.AddDrawable(cubeDrawable);
            
            // Create a root node (Group) and add the Geode as a child
            var root = Group.Create();
            root.AddChild(cubeGeode);
            
            // Set the root of the scene that the viewer is viewing
            viewer.SetSceneData(root);
            
            // Set the camera to view all extents.
            viewer.ViewAll();            
            
            // Run the app
            viewer.Run();
        }
    }
}
```

##### February 2019

NexSeceneGraph is an ongoing experiment to design a scene graph around modern low-level graphics APIs using .NET Core.  The genesis of this work arose from the need for an Open-Source, robust, Scene Graph API for scientific visualization applications in .NET.    An early decision was made to build upon [Veldrid](https://github.com/mellinoe/veldrid), by Eric Mellino - as this project has already accomplished most of the difficult work involved with low-level binding to backed APIs and presentation of the underlying features in a common API surface.   Common to Veldrid, our intent is to support the following backends:

* Direct3D 11
* Vulkan
* Metal
* OpenGL 3
* OpenGL ES 3

Developers familiar with scene graphs will recognize a strong API resemblence to [OpenSceneGraph](http://www.openscenegraph.org/) by Robert Osfield.  _This is by design_.  Much of the OpenSceneGraph API is very stable and well-accepted, and we attempt (as much as possible) to mirror OSG concepts in order to give developers familiar with it a smooth on-ramp to use of NexSceneGraph.

This code should be considered __early-alpha__.   The core APIs are still changing, and much work remains.   If you are interested in contributing, please look at the "TODO.txt" file in the repository, contact me, and start working on your feature.   All pull requests will be considered, but I very much appreciate a heads-up on what you are working on so that  I can coordinate muliple devlopers so that work at this early stage doesn't overlap too much.

## License

This code is licensed under the Apache 2.0 License

## Build instructions

NexSceneGraph  uses the standard .NET Core tooling. [Install the tools](https://www.microsoft.com/net/download/core) and build normally (`dotnet build`).

## Examples

There are a number of examples available highlighting current syntax.  These are evolving and should be considered in-flux at present.  Examples are in the "Examples" Solution Directory.   All examples use a common viewer that implements a trackball manipulator (left-mouse button) including zoom (right mouse button).

* __Hello Nsg:__  
  This example is the "Hello, world" of NexSceneGraph

* __Colored Cube:__  
  This example demonstrates construction of a Geometry Node and specifying of a basic shader

* __Culling Colored Cubes:__  
  This example demonstrates view frustum culling, which occurs on a per-primitive basis.  (Hint - check the frame rate as you zoom in)

* __Textured Cube:__  
  This example demonstrates texture mapping on a simple cube

* __Multi Textured Cube:__  
  This example demonstrates applying multiple textures to a geometry and using a shader to manipulate the final color

* __Billbord:__   
  This example demonstrate a simple billboard geometry node

* __Text Rendering:__   
  This example demonstrates a text geometry node

* __Switch Example:__   
  This example demonstrates the use of a switch geometry node

* __Transparency Sorting:__  
  This example demonstrates efficient sorting of transparent geometry.  Transparency sorting is performed on a per-primitive basis.

* __Mixed Transparent Opaque Geometry:__  
  This example demonstrates integrating both opaque and transparent geometry in a scene - this is often one of the most challenging rendering scenarios to get correct, and NexSceneGraph handles the common case (per-primitive sorting) out-of-the-box.
