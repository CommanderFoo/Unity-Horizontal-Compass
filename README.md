A basic horizontal compass that allows you to add waypoints to it.

It's not perfect, and I'm sure there are better ones out there, but hopefully it helps someone get a starting point.

![Screen Shot](https://i.imgur.com/3eOLtlF.png)

The project has an example of adding 2 waypoints to the compass.  It's pretty simple.

    // First param is a unique key for this waypoint so you can access it later (i.e remove it).
    // Second param is the waypoint with icon and target.
    
    Horizontal_Compass.instance.add("test", new Waypoint(){
    			
    	icon = this.icon,
    	target = this.obj
    
    });

There are a few 3rd party assets I used.

 - http://dotween.demigiant.com
    
	Used to fade in and out the waypoints when they get close to ends of the compass.
   
 - https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126
 
	Used for the text on the compass (N, E, S, W).	
 
 - https://bitbucket.org/UnityUIExtensions/unity-ui-extensions
 
 	I used the mask effect for the ends of the compass just to give it a better look.
 	