# TRIDINET

![Tridinet Logo](https://tridinet.com/untitled.png "Tridinet")

Tridinet is a network of functional 3d objects that can be used to create 3d Worlds that are both functional, interactive and flexible, to understand tridinet let's look at the layers

## Presentation Layer

This layer is responsible for presenting the 3d objects to the user, it is responsible for the rendering of the 3d objects and covers 3d mesh, material, shader, texture and dynamic UI, the format of tridinet files is .tridinet

## Functional Layer

This layer encapsulates the functional attributes of the 3d Object driven by a declarative script called Trscript, Trscript enables developers to inject functional attributes into the 3d objects such as spawning new tridinet objects, embedded and displaying data, animations and more.

## Network Layer

This layer deals with the deployment and distribution of the 3d Objects via tridinet repositories and live servers,

### Repository
The tridinet repository is the store for your tridinet objects, the repository is identified by a manifest, the manifest repreents the state of the 3d files in the repository and their urls.

## Live Server
This server is responsible for delivering dynamic content to users interacting with your tridinet objects such as manipulating the transform of the objects as well as injecting dynamic scripts into the objects.

# USAGE

- Dowanload and import sdk into your unity project
- Create tridinet objects from your models by right clicking on them, go to tridinet option and choose "Create"
- Click on the Tridinet tab above and in the drop down select Script Editor, you can now spawn the tridinet objects into your scene and start building
- Alternatiively you can click on Editor from the drop down, sign or register to get access to pre existing repositories  from which you can start building your world.

## TR SCRIPT

TrScript is delcarative which means you don't write the implementation details, you only set the properties of the functions and it would be executed

Current functions range from
- Spawning new tridinet objects
- Rotations
- Display UI menus

## Example of TrScript

```
Spawn: {
	target=f17af63e-bf34-45cf-a5d4-61ce9381583e;
	num = 1;
	offset = 1;
	postion=(0,1,0);
	scale=(100,100,100);
	rotation=(-90,0,0);
	direction = left;
}:
Data:{
	Description="This is some test Description for testing";
}:
|
Rotate: {
	axis=(0,1,0);
	angle=0; 
	loop=True;
}:
Spawn: {
	target=87c8acda-cd75-4a04-87a3-d94ba2ac7e46;
	num = 7;
	offset = 10;
	position= (10,0,0);
	scale=(100,100,100);
	rotation=(-90,0,0);
	direction = left;
	style=radial;
    layer=2;
}:
Spawn: {
	target=fff1ab35-90f1-4181-b31e-9b7f9a69e964;
	num = 10;
	offset = 30;
	position= (10,0,0);
	scale=(5,5,5);
	rotation=(-90,0,0);
	direction = left;
	style=radial;
}:

Display: {
	Text=(20, 40, "Text", inline, 20, bold);
	Image = (20, 40, "imagelink", inline);
	Text =(20, 50, ref[Description], block, 20, regular);
    InputField=(20, 30, UserName);
    InputField=(20, 30, Password);
    InputField=(20, 30, Email);
    Toggle=(Save);
    OnClick=Call[Register](actionName, inline, "url", POST, body, UserName, Password, Email, params);
	Response=Register(@Run[Custom, Spawn])
	OnClick = Open(actionName, inline, url);
    OnClick = Run(actionName, block, Custom, Spawn);
}:

```

TrScript documentation coming soon....

To Download the Browser and sdk visit [tridinet.com](https://tridinet.com)