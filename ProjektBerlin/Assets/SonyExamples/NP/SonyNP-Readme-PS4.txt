
-- UnityNpToolkit Example Project. --

This example project demonstrates how to use the UnityNpToolkit API to interface a Unity application with the Playstation Network.

The API currently supports the following NP services...

	Friends,
	Matching,
	Ranking,
	Trophies,
	Messaging with session and game invites,
	Word filtering/comment sanitizing,
	Title User Storage (TUS),
	Title Small Storage (TSS),
	Commerce.

In addition to the above the plugin also provides support for the following...

	NP dialogs including friends list and user profile.
	Posting messages to facebook.
	Posting message to Twitter.

-- Project Folder Structure --

Editor - Contains useful editor scripts for initialising the required player settings for the example project.
Editor/SonyNPPS4PublishData - Contains the param.sfx file and an example trophy pack with source data.
Plugins/PS4 - Contains the UnityNPToolkit native plugin.
SonyAssemblies - Contains the SonyNP managed interface to the UnityNpToolkit plugin.
SonyExample/NP - Contains a Unity scene which runs the scripts.
SonyExample/NP/Scripts - Contains the Sony NP example scripts.

The SonyNP managed assembly defines the following namespaces...

Sony.NP.Main		Contains methods for initializing and updating the plugin.
Sony.NP.System		Triggers events for app resume, connection up/down, etc. Also contains methods for checking connection bandwidth.
Sony.NP.User		Contains methods for signing in to the Playstation Network, obtaining the users profile and setting online presence.
Sony.NP.WordFilter	Contains methods for censoring and sanitizing strings.
Sony.NP.Friends		Contains methods for obtaining a friends lists.
Sony.NP.Facebook	Contains methods for posting messages to facebook.
Sony.NP.Matching	Contains methods for creating, modifying, joining and finding network sessions.
Sony.NP.Ranking		Contains methods for submitting scores to a ranking server, obtaining lists of ranks, friend ranks and own rank.
Sony.NP.Trophies	Contains methods for awarding trophies and retrieving trophy info.
Sony.NP.TusTss		Contains methods for getting/setting 'Title User Storage' data and variables, and for retrieving 'Title Small Storage' data.
Sony.NP.Commerce	Contains methods for accessing the Playstation Title store, in-game store, getting and consuming entitlements, etc.

Please refer to the example scripts for details on using the API.

-- Requirements For Facebook Integration --

To use Facebook integration you need to visit https://developers.facebook.com/ and create an app which binds to your Sony applications title ID, this gets you an app ID which is used below.


To configure UnityNpExampleProject to use the correct settings; load the project into Unity and from the "Custom Tools" menu select "Set Publish Settings for PS4"

