# NexSceneGraph

##### February 2019

NexSeceneGraph is an ongoing experiment to design a scene graph around modern low-level graphics APIs using .NET Core.  The genesis of this work arose from the need for an Open-Source, robust, Scene Graph API for scientific visualization applications in .NET.    An early decision was made to build upon [Veldrid](https://github.com/mellinoe/veldrid), by Eric Mellinoe - as this project has already accomplished of the difficult work involved with low-level binding to backed APIs and presentation of the underlying features in a common API surface.   Common to Veldird, our intent is to support the following backends:

* Direct3D 11
* Vulkan
* Metal
* OpenGL 3
* OpenGL ES 3

Developers familiar with scene graphs will recognize a strong API resemblence to [OpenSceneGraph](http://www.openscenegraph.org/) by Robert Osfield - this is by design.  Much of the OpenSceneGraph API is very stable and well-accepted, and we attempt (as much as possible) to mirror OSG concepts in order to give developers familiar with it a smooth on-ramp to use of NexSceneGraph.

This code should be considered __early-alpha__.   The core APIs are still changing, and much work remains.   If you are interested in contributing, please look at the "TODO.txt" file in the repository, contact me, and start working on your feature.   All pull requests will be considered, but I very much appreciate a heads-up on what you are working on so that  I can coordinate muliple devlopers so that work at this early stage doesn't overlap too much.

### Build instructions

NexSceneGraph  uses the standard .NET Core tooling. [Install the tools](https://www.microsoft.com/net/download/core) and build normally (`dotnet build`).

### Examples

There are a number of examples available highlighting current syntax.  These are evolving and should be considered in-flux at present.  Examples are in the "Examples" Solution Directory.   All examples use a common viewer that implements a trackball manipulator (left-mouse button) including zoom (right mouse button).

* __Hello Nsg:__  
  This example is the "Hello, world" of NexSceneGraph

* Colored Cube:
  This example demonstrates construction of a Geometry Node and specifying of a basic shader

* Culling Colored Cubes"
  This example demonstrates view frustum culling, which occurs on a per-primitive basis.  (Hint - check the frame rate as you zoom in)

* Textured Cube:
  This example demonstrates texture mapping on a simple cube

* Multi Textured Cube:
  This example demonstrates applying multiple textures to a geometry and using a shader to manipulate the final color

* Billbord:
  This example demonstrate a simple billboard geometry node

* Text Rendering:
  This exmaple demonstrates a text geometry node

* Switch Example:
  This example demonstrates the use of a switch geometry node

* Transparency Sorting:
  This example demonstrates efficient sorting of transparent geometry.  Transparency sorting is performed on a per-primitive basis.

* Mixed Transparent Opaque Geometry:
  This example demonstrates integrating both opaque and transparent geometry in a scene - this is often one of the most challenging rendering scenarios to get correct, and NexSceneGraph handles the common case (per-primitive sorting) out-of-the-box.
