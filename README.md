# Game Jolt Fireside

General purpose support library for Game Jolt Firesides. Allows interacting with realtime services Grid and Chat offered by Game Jolt for Firesides.

## Using the library

If you want to include this library in your .net project:

 - Go to the [tags](https://github.com/nilllzz/game-jolt-fireside/tags) of this repository
 - Look for the latest release and download the `GameJoltFireside.dll` file
 - In your Visual Studio Project, add a Project Reference, click "Browse..." and select the `GameJoltFireside.dll` file you downloaded

Alternatively, if you don't want to rely on prebuilt binaries or want to make changes to the library code:

### Building the project
 - Clone this repository
 - Open the .sln file in Visual Studio (at least version 2022 required)
 - Make your changes, then set the build type to "Release"
 - Build the the project/solution, then you can access the `GameJoltFireside.dll` from the bin folder in the project folder
 - In your Visual Studio Project, add a Project Reference, click "Browse..." and select the `GameJoltFireside.dll` file you built (It is usually a good idea to copy the .dll file into your project structure, but selecting it directly from the bin folder works too)

### Building with the project as part of your solution
 - Clone this repository
 - Copy the "GameJoltFireside" folder (containing the .csproj file) into your project's folder structure
 - In your Visual Studio Project, add an Existing Project to your solution and pick the .csproj file of this library
 - Then add a Project Reference from your project to the GameJoltFireside project that you just added to your solution
