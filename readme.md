ImageMesh
==========

Ever wanted to create Sky Spheres / Sky Boxes like in Homeworld 2? Well, this utility is just for you!
A simple utility for converting images to either a flat mesh or a sphere using only vertex colors.

If you are looking for information on this technique see: (https://simonschreibt.de/gat/homeworld-2-backgrounds/)

Features
=========
* Displacement: Grayscale Average, Red, Green, Blue, Red Green Average, Blue Green Average, and Red Blue Average. Due note sphere with displacement may have visible seams or not fully connected. Displacement is mainly recommended for flat only.
* Pixel Sampling Methods: Single Pixel, Average (with radius), Gaussian, Min (with radius), and Max (with radius)
* Decimation: Think of it like a tolerance for color ranges. It affects what pixels should be combined into one vertex based on the pixel sampling method. This also affects the number of vertices. 0 for no decimation.
* Image scaling from 100% to 25%
* Generate a sphere or a flat mesh
* Import previous .imesh files for viewing only
* Export to a simple ascii based .imesh file 
* Triangle indices order is Clockwise.

Mouse Controls
=================
Left Click and Drag: Pan Around

Right Click and Drag: Rotate Mesh

Middle Click and Drag: Zoom in / out.

Extra
======
The source includes both a reader and writer for C# for the .imesh format, as well as a reader for Unreal Engine 4. The C# class can be used in Unity 3D, but also requires the MeshG class as well. 

The Unreal Engine 4 reader supports Blueprints, since it is an Actor Component, and automatically handles the reading of the imesh file path you specify.

The C# class has a ParseMeshString function that does all the parsing for you and returns a MeshG object. As to how you load the string for C# that is up to you.

This does not build the mesh for you. This just reads in and parses the vertices, indices, vertex colors, and normals into lists. It is up to you to connect that to a procedural mesh renderer in your engine.

Builds
=======
A precompiled build is available for download for Windows: [ImageMesh](http://vantagetechnic.com/ImageMesh-0.1.0.zip)

Building It Yourself
=====================
The application relies only on OpenTK 3.0 and OpenTK Control 3.0. Which, both can be installed via Nuget. OpenTK is only used for the viewer / editor part.

The classes have been separated out from the form / viewer / editor. So, you can technically use the classes without any dependencies, if you so wish.

Unreal Engine 4 Caveats
=========================
You will not be able to see the .imesh files in the Unreal Engine Editor browser or be able to drop them in via the editor. You will need to place them in the directory you want manually. I highly recommend creating a directory for it. Then in the Unreal Project Settings -> Packaging make sure to include the folder as a Non-Asset Based Directory for either Copying or Packing.

Using the IMeshReader Component in Blueprints
-----
First you will need to use the ParseFile function to read in the file and parse it. Then you can access all of the following: Vertices, Normals, Indices, and Colors from the component. All of the lists are compatible with the Procedural Mesh Component. Plug them into a Procedural Mesh Component's Create Section and viola. If you want to see it realtime in the actor blueprint editor, then be sure to do this in the Construct Event rather then in Begin Play.

.imesh Format
================

```
vertices
x y z nx ny nz r g b
x y z nx ny nz r g b
...

triangles
idx1 idx2 idx3
idx1 idx2 idx3
...

```

The format doesn't care as to which comes first the vertices or the triangles. Triangles section is optional.

Anything following the "vertices" keyword is considered a vertex.

A vertex has the following format: position (x y z) normal (nx ny nz) color (r g b). Each vertex is separated by \r\n aka one vertex per line. The values are separated by a space. All values of the vertex are floats or doubles. 

Anything following the "triangles" keyword is considered a triangle.

A triangle has the following format: index1 index2 index3. The indices are uint32, uint64 or ulong. The three indices are separated via a space. Each triangle is separated by \r\n aka one triangle per line.

Licensing
==========
MIT License for anything not related to the S-Hull algorithm (this includes the .imesh format). S-Hull C# is licensed under GPL - see: [s-hull.org](http://s-hull.org)

How can I tell the difference between the two? Anything that is NOT in the SHull folder is MIT. Otherwise it falls under S-Hull GPL.