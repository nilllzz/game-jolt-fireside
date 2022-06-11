# Game Jolt Fireside

General purpose support library for Game Jolt Firesides. Allows interacting with realtime services Grid and Chat offered by Game Jolt for Firesides.

## Using the library

If you want to include this library in your .net project:

-   Go to the [tags](https://github.com/nilllzz/game-jolt-fireside/tags) of this repository
-   Look for the latest release and download the `GameJoltFireside.dll` file
-   In your Visual Studio Project, add a Project Reference, click "Browse..." and select the `GameJoltFireside.dll` file you downloaded

Alternatively, if you don't want to rely on prebuilt binaries or want to make changes to the library code:

### Building the project

-   Clone this repository
-   Open the .sln file in Visual Studio (at least version 2022 required)
-   Make your changes, then set the build type to "Release"
-   Build the the project/solution, then you can access the `GameJoltFireside.dll` from the bin folder in the project folder
-   In your Visual Studio Project, add a Project Reference, click "Browse..." and select the `GameJoltFireside.dll` file you built (It is usually a good idea to copy the .dll file into your project structure, but selecting it directly from the bin folder works too)

### Building with the project as part of your solution

-   Clone this repository
-   Copy the "GameJoltFireside" folder (containing the .csproj file) into your project's folder structure
-   In your Visual Studio Project, add an Existing Project to your solution and pick the .csproj file of this library
-   Then add a Project Reference from your project to the GameJoltFireside project that you just added to your solution

## Library api

This section of the readme explains the different functions of the library. It offers three main services:

-   Grid (realtime notifications)
-   Chat (Game Jolt chat)
-   SiteApi (Game Jolt's http api)

Note: this api makes heavy use of the async/await programming concept in C#. Make sure you are somewhat familiar with how it works before diving in.

### Grid

The Grid service allows subscribing to realtime notifications that are distributed to the user on Game Jolt.

To connect to the Grid service, use the `GameJoltFireside.Services.Grid.GridClient` class. To connect to it, you need to authenticate with your Game Jolt account. Read the "Authentication" section of this readme on how to do that.

Example code:

```
// This example connects to Grid, joins the user notification channel and then sets up an event handler on
// the "new notification" event. It then logs what kind of notification the user received.

var gridClient = new GridClient(user: userConfig);
await gridClient.Connect();

var notificationChannel = gridClient.CreateChannelNotifications();
await notificationChannel.Join();

notificationChannel.NewNotification += (UserNotificationChannel.NewNotificationPayload payload) => {
    var item = payload.notification_data.event_item;

    Console.WriteLine($"Received notification of type {item.type}.");
};
```

### Chat

The Chat service allows connecting to Game Jolt's chat system to retrieve messages sent in chat rooms.

To connect to the Chat service, use the `GameJoltFireside.Services.Chat.ChatClient` class. To connect to it, you need to authenticate with your Game Jolt account. Read the "Authentication" section of this readme on how to do that. _(I am planning to allow connecting to the Chat service with a guest account to connect to public chat rooms that don't require authentication, but that is not implemented yet.)_

Example code:

```
// This example connects to the Chat service, then joins a room channel and sets up an event handler to log every new message sent to the room.

var chatClient = new ChatClient(user: userConfig);
await chatClient.Connect();

var roomChannel = chatClient.CreateChannelRoom(<chat room id>);
var response = await roomChannel.Join();

// The `response` object contains room information and recent messages the room gets bootstrapped with.

roomChannel.Message += (ChatMessage message) => {
    Console.WriteLine($"Received message: {message.GetMessageDocument().ToString()}");
};
```

### SiteApi

The SiteApi service allows interfacing with Game Jolt's http based web api. It is a thin wrapper around making an http request, with typed response data.

Note that depending on which endpoint you are calling, it may require an authenticated user. Read the "Authentication" section of this readme on how to do that.

Example code:

```
var apiClient = new SiteApiClient(user: userConfig);

// The resulting payloadData contains only the actual result payload.
// This call will throw an exception if the http request does not succeed (http status 200).
var payloadData = await apiClient.get(<api endpoint>);

// To get the full response even if it fails, use this:
var payload = await apiClient.RequestGet(<api endpoint>);
```

## Authentication

The services in this api require you to authenticate with your Game Jolt account to function properly.

First thing you'll need is obviously a Game Jolt account that you can log in to using a web browser. If you don't have one, go here: https://gamejolt.com/join.

Next, this library uses your username, user id and auth token to authenticate. Game Jolt currently does not offer a service like OAuth to authenticate third party libraries or apps, so this library needs to instead rely on using the authentication token Game Jolt gives you.

Here's how to collect the information needed:

-   **username**: Your username on Game Jolt, the portion after the "@". For example: The username for this user https://gamejolt.com/@nilllzz is "nilllzz"
-   **user id**: This is the numeric id associated with a Game Jolt account. This number is not surfaced on Game Jolt's UI directly. To find it, you will need to look at an XHR request the website makes to the server.
    -   In your browser, open https://gamejolt.com
    -   Press F12 to open the dev tools
    -   Click on the "Network" tab, toggle "XHR", and then with the dev tools opens, refresh the page.
    -   Find a request made to the `/site-api/web/dash/home` endpoint and select it.
    -   The response for that request will start with `{"ver":"some number","user":{"id":<YOUR USER ID>,...`
    -   Copy the user id from `<YOUR USER ID>`
-   **auth token**: Similar to the user id, this information is not exposed by Game Jolt's UI. However, this is expected. Your auth token is SENSITIVE INFORMATION that is a stand-in for your Game Jolt account password that browsers use so you don't have to re-enter your password with every page you visit. To find this value:
    -   In your browser, open https://gamejolt.com
    -   Press F12 to open the dev tools
    -   Click on the "Application" tab (Edge, Chrome) or "Storage" tab (Firefox)
    -   In that tab, find the "Cookies" section
    -   Find an entry called "frontend", and copy its value. It should be a 26 character long alphanumeric string.
    -   Note: if you log out of Game Jolt and log back in, your auth token may change. If you find that you cannot authenticate with Game Jolt using this library, check that you are still using your current auth token.

Once you have your username, user id and auth token, you can authenticate using this library.

Example code:

```
// Using the global config instance:
UserConfig.Instance.Username = "your username";
UserConfig.Instance.UserId = your user id;
UserConfig.Instance.AuthCookie = "your auth token";

var grid = new GridClient(); // The global instance is used by default.
```

```
// Using a new config instance:
var userConfig = new UserConfig();
userConfig.Username = "your username";
userConfig.UserId = your user id;
userConfig.AuthCookie = "your auth token";

var grid = new GridClient(user: userConfig); // Services allow you to pass in a user config option, which they will use instead of the global instance.
```
