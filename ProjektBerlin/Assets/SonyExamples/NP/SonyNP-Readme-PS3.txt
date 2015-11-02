
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
	Commerce,
	Auth Ticketing.

In addition to the above the plugin also provides support for the following...

	Posting messages to facebook.

-- Project Folder Structure --

Editor - Contains useful editor scripts for initialising the required player settings for the example project.
Editor/SonyNPPS3PublishSettings - Contains the param.sfx file and an example trophy pack with source data.
Plugins/PS3 - Contains the UnityNPToolkit native plugin.
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
Sony.NP.Messaging	Contains methods for showing game/session invites and messages, also for sending game/session invites and messages and in-game data messages.
Sony.NP.Ranking		Contains methods for submitting scores to a ranking server, obtaining lists of ranks, friend ranks and own rank.
Sony.NP.Trophies	Contains methods for awarding trophies and retrieving trophy info.
Sony.NP.TusTss		Contains methods for getting/setting 'Title User Storage' data and variables, and for retrieving 'Title Small Storage' data.
Sony.NP.Commerce	Contains methods for accessing the Playstation Title store, in-game store, getting and consuming entitlements, etc.
Sony.NP.Ticketing	Contains methods for requesting an auth-ticket and getting information from a ticket.

Please refer to the example scripts for details on using the API.

-- Requirements For Facebook Integration --

To use Facebook integration you need to visit https://developers.facebook.com/ and create an app which binds to your Sony applications title ID, this gets you an app ID which is used below.

Additionally for the facebook API to work with NP SNS when running a 'PC Hosted' build you need to do the following...

The Title ID used must match the Title ID in your PARAM.SFO

You must turn on PARAM.SFO mapping on your development tool.

You can do this by rightclicking your tool in the target manager and going to Properties.

Under "Target --> TM Properties", set PARAM.SFO mapping = yes.

set "Use ELF directory for PARAM.SFO mapping" = no

set "PARAM.SFO path" to the location of the PARAM.SFO for your project, this will have been written to the build target folder when you did "build" or "build & run" in the Unity editor.

Under XMB Settings, you will have to set "Game Type (Debugger)" to "PARAM.SFO".

If running an installed package be sure to reset the settings made above to their defaults.

See https://ps3.scedev.net/forums/thread/215312/ & https://ps3.scedev.net/forums/thread/225038/

-- Required Player Settings --

Before building the example project you need to fill in the 'Publish Settings' section of the PS3 Player Settings as follows...

-- Package Parameters --

Click the 'Config (param.sfx)' browse button and select the param.sfx file located in Assets/Editor/SonyNpPS3PublishData folder.

-- Trophy pack --

To enable trophies in the example go to the PS3 player settings, then in the 'Publish Settings' section click the 'Trophy Pack' browse button and select the trophy pack located in the Assets/Editor/SonyNpPS3PublishData folder.

-- NP Communications --

The example will not do much without a valid NP communications ID, NP pass phrase and NP signature, to set these go to the PS3 player settings and copy the values shown below into each of the sections in 'Publish Settings'...

'NP Communications ID'
NPWR06132_00

'NP Communications Passphrase'
0x6e,0x07,0x60,0x9a,0x21,0x61,0x63,0xbf,
0xf3,0x9a,0x68,0x63,0x97,0xec,0x32,0x9e,
0x22,0x6f,0x9e,0xbc,0x0c,0xf9,0xa2,0x17,
0x53,0x9e,0x94,0x32,0xe5,0x26,0x8b,0x06,
0x20,0x2b,0xa2,0x7e,0x45,0x66,0x61,0xa8,
0xbd,0xfe,0x97,0xe2,0xdd,0xd5,0x37,0x87,
0xbe,0x4c,0x28,0x15,0x66,0x22,0xf1,0xe3,
0x30,0xa7,0xc2,0xae,0xe6,0x38,0x45,0xfe,
0x92,0xf1,0x67,0xf7,0xbb,0xfa,0x9b,0x13,
0x87,0x6c,0x55,0x00,0x9c,0x5e,0x16,0x81,
0xe0,0xc6,0x65,0x1b,0x73,0x3c,0xfe,0x51,
0xf5,0x14,0xf0,0xc9,0x41,0x88,0x75,0x07,
0x07,0xb3,0x17,0xdf,0xd1,0x56,0x08,0x05,
0xa6,0x9c,0xd6,0xaa,0x3d,0xaf,0xe1,0x91,
0x52,0xc1,0x10,0xbc,0xac,0xe2,0xdd,0xca,
0x34,0xc9,0x92,0xfc,0x14,0x2d,0x2a,0xce

'NP Communications Signature'
0xb9,0xdd,0xe1,0x3b,0x01,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x23,0xcf,0x94,0x38,
0x3b,0xcb,0xe8,0xbc,0xde,0x13,0x60,0x7c,
0x1a,0x9b,0x2b,0xfe,0x90,0x4d,0x04,0x7c,
0x6d,0xbb,0x1b,0xc3,0x84,0xd3,0x03,0xff,
0xe6,0xef,0xb5,0xa1,0x01,0x63,0xd3,0x14,
0x97,0xef,0x7a,0x45,0xa5,0x74,0x6f,0x4a,
0x1c,0x6f,0x3f,0x2c,0x64,0xa6,0x0e,0x1d,
0xe6,0x11,0x00,0xc3,0x76,0xbb,0x4e,0xae,
0xf0,0xc1,0x9c,0xff,0x55,0xa2,0x77,0xc9,
0xff,0x75,0xb3,0xcb,0x3d,0x38,0x3d,0xbc,
0x02,0x64,0xcd,0x3f,0xef,0x80,0x38,0xaf,
0x72,0xea,0x65,0x8a,0x2e,0xf9,0xe9,0xad,
0x85,0x7d,0xf2,0xbe,0x7e,0x79,0x14,0x7a,
0x16,0xba,0xc3,0x59,0x5a,0xf4,0x23,0x1b,
0xe8,0x32,0x9f,0x28,0xa9,0x36,0x0d,0x38,
0xa5,0x89,0xc2,0x91,0x85,0xae,0x83,0xe0,
0x12,0x86,0x86,0x2a,0x61,0x1f,0x7d,0xc1,
0x9e,0xd3,0x91,0x88,0x4b,0x26,0xbc,0x81,
0x94,0xa6,0x2a,0x84,0xc5,0xfe,0x8d,0xba
