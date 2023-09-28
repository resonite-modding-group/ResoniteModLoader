# Problem Solving Techniques

Resonite has many different ways to solve problems including Components, ProtoFlux, HTTP connections to an external server, refhacking, plugins, and mods. Some of these methods are supported, while others are not. We'll do a quick rundown of the methods and where they're applicable.

## Components and ProtoFlux

Using Components and/or ProtoFlux to address problems is generally the preferred approach, as more advanced techniques are likely to be overkill.

## HTTP Connections

Resonite provides ProtoFlux nodes to communicate with an external server via HTTP GET, POST, and websockets. This is great for:

- Heavy data processing that ProtoFlux isn't well suited for
- Advanced data persistence (for simple things, consider using Cloud Variables)
- Connecting ProtoFlux across multiple sessions

## Plugins

Plugins allow you to add new components and ProtoFlux nodes, but at the cost of breaking multiplayer compatibility. If you like multiplayer, they aren't going to give you much quality-of-life because they require everyone in a session to use the exact same set of plugins. Plugins are great for automating menial tasks that you do very infrequently, for example monopacking is nightmarish to do by hand but can be done with a single button via a plugin.

## Mods

Mods do **not** let you add new components and ProtoFlux nodes, but they do work in multiplayer. They are limited in what they can do without breaking multiplayer compatibility. You can imagine them as a "controlled desync". They are well-suited for minor quality-of-life tweaks, for example preventing your client from rendering motion blur. Making a larger feature with a mod isn't a great option, as you cannot rely on other clients also having the mod. 

## RefHacking

RefHacking is a method that at considerable performance cost can get you an extremely sketchy and fragile but working component access. Refhacking is not supported and will break in the future, but sometimes it is the only way to do certain things without waiting for real component access.

Consider keeping your component access ideas as a to-do list and create them when there is proper support in place. This approach avoids potential disruptions caused by the breakdown of your creations.