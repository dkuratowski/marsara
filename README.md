# The Mar Sara Project

The Mar Sara Project is an initiative for the retro-style reproduction of Blizzard's well known StarCraft. The name of this reproduction is RetroCraft.
The game is still under development, but some of the basic units and buildings of the Terran race have been implemented.

## Prerequisites

RetroCraft is based on .NET 6. In order to build and run the project you have to [download the .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
The rendering plugin of RetroCraft has a dependency to [MonoGame](https://www.monogame.net/) and [ImageSharp](https://sixlabors.com/products/imagesharp/), therefore it is cross-platform and can run on both MacOS (Catalina 10.15 and up), Windows (8.1 and up) and Linux.
Because of the MonoGame dependency, RetroCraft requires at least OpenGL 2.0 with the `ARB_framebuffer_object` extension or alternatively at least OpenGL 3.0.

## Build and Run

1. Clone this repository to a local workspace folder (e.g.: `~/repositories/marsara/`)
2. In terminal go to the `src/RC.App.Starter/` folder inside your workspace (e.g.: `~/repositories/marsara/src/RC.App.Starter/`)
3. Build the project with `dotnet build`.
4. Go to the `src/RC.App.Starter/bin/Debug/net6.0/` folder inside your workspace (e.g.: `~/repositories/marsara/src/RC.App.Starter/bin/Debug/net6.0/`).
5. Execute the created executable (`./rc` on Mac/Linux, `rc` on Windows).
6. Enjoy!

## Gameplay Video

[Watch](https://youtu.be/PlYDIE9Pk44) on Youtube.

## Screenshots

![Optional Text](../master/docs/screenshots/mainmenu.png)
![Optional Text](../master/docs/screenshots/registrypage.png)
![Optional Text](../master/docs/screenshots/gameplay1.png)
![Optional Text](../master/docs/screenshots/gameplay2.png)
![Optional Text](../master/docs/screenshots/gameplay3.png)
![Optional Text](../master/docs/screenshots/gameplay4.png)
![Optional Text](../master/docs/screenshots/gameplay5.png)
![Optional Text](../master/docs/screenshots/gameplay6.png)
![Optional Text](../master/docs/screenshots/gameplay7.png)
![Optional Text](../master/docs/screenshots/gameplay8.png)
