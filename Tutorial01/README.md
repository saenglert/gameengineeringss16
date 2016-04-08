#Tutorial 01

##Exercise/Questions
Investigate how the vertice's coordinates relate to pixel positions within the output window.
 
 - What are the smallest and largest x- and y-values for vertices that can be displayed within the output window?

```
new float3(1,1,1);
new float3(0,0,0);
```

 - What happens to your geometry if you re-size the output window? 
```
The geometry is adjusting to the resize as realtive positions are used.
```
 - What happens if you change the z-values of your vertices (currently set to 0)?
```
Nothing as of yet because we are looking directly on to the z axis.
````
Understand how the `Triangles` are indices into the `Vertices` array.
 
 - What happens if you change the order of the indices in the `Triangles` array? Try to explain your observation.
```
Only counterclockwise definitions of triangles yield a graphical result.
```
 

 
