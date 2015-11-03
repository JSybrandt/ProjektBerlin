
UnityNpToolkit Example Project.

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

	NP dialogs including friends list and user profile.
	Posting messages to facebook.
	Posting message to Twitter.

Project Folder Structure

Editor - Contains useful editor scripts for initialising the required player settings for the example project.
Editor/SonyNPVitaPublishSettings - Contains the param.sfx file and an example trophy pack with source data.
Plugins/PSVita - Contains the UnityNPToolkit native plugin.
SonyAssemblies - Contains the SonyNP managed interface to the UnityNpToolkit plugin.
SonyExample/NP - Contains a Unity scene which runs the scripts.
SonyExample/NP/Scripts - Contains the Sony NP example scripts.
StreamingAssets - Contains raw files for use at runtime, i.e. Twitter images.

The SonyNP managed assembly defines the following namespaces...

Sony.NP.Main		Contains methods for initializing and updating the plugin.
Sony.NP.System		Triggers events for app resume, connection up/down, etc. Also contains methods for checking connection bandwidth.
Sony.NP.Dialogs		Provides an API for displaying NP dialogs, includes Friends List, Shared Play History and Profile.
Sony.NP.User		Contains methods for signing in to the Playstation Network, obtaining the users profile and setting online presence.
Sony.NP.WordFilter	Contains methods for censoring and sanitizing strings.
Sony.NP.Friends		Contains methods for obtaining a friends lists.
Sony.NP.Facebook	Contains methods for posting messages to facebook.
Sony.NP.Twitter		Contains methods for posting messages to Twitter.
Sony.NP.Matching	Contains methods for creating, modifying, joining and finding network sessions.
Sony.NP.Messaging	Contains methods for showing game/session invites and messages, also for sending game/session invites and messages and in-game data messages.
Sony.NP.Ranking		Contains methods for submitting scores to a ranking server, obtaining lists of ranks, friend ranks and own rank.
Sony.NP.Trophies	Contains methods for awarding trophies and retrieving trophy info.
Sony.NP.TusTss		Contains methods for getting/setting 'Title User Storage' data and variables, and for retrieving 'Title Small Storage' data.
Sony.NP.Commerce	Contains methods for accessing the Playstation Title store, in-game store, getting and consuming entitlements, etc.
Sony.NP.Ticketing	Contains methods for requesting an auth-ticket and getting information from a ticket.

Please refer to the example scripts for details on using the API.

Commerce - Some useful links...

PlayStation®Store - https://psvita.scedev.net/docs/vita-en,NP_Commerce-Overview-vita,PlayStationregStore/1/
PlayStation®Store Submission Guideline - https://psvita.scedev.net/projects/publishing/dl/dl/792/4832/PSStore_Submission-Guideline_e.pdf
Title Management - https://psvita.scedev.net/titlemanagement/titles
Network Platform Management Tool - https://npmt.sp-int.tools.playstation.net/
NP Customer Service Tool - https://ncst.sp-int.tools.playstation.net/CSTool.vm
PSN service configuration - https://psvita.scedev.net/psn
Development Support Functions - https://psvita.scedev.net/docs/vita-en,NP_Commerce-Programming_Guide-vita,Development_Support_Functions/1/

Requirements For Facebook Integration

To use Facebook integration you need to visit https://developers.facebook.com/ and create an app which binds to your Sony applications title ID, this gets you an app ID which is used below.

Requirements For Twitter Integration

For Twitter integration to work the "Use Tw Dialog" flag must be set in the "ATTRIBUTE" member of the param.sfx/sfo,

e.g. <param key="ATTRIBUTE">33554432</param>

Or if not using a param.sfx file make sure that "Allow Twitter Dialog" is ticked in the PS Vita player settings.

For the "Use Tw Dialog" or "Allow Twitter Dialog" flags to work you must build a PSVita package and install it on the Vita, once this has been done Twitter will work with normal 'PC Hosted' builds.

Required Player Settings

Before building the example project you need to fill in the 'Publish Settings' section of the PSVita Player Settings as follows...

Package Parameters

Click the 'Config (param.sfx)' browse button and select the param.sfx file located in Assets/Editor/VitaPublishData folder.

Trophy pack

To enable trophies in the example go to the PS Vita player settings, then in the 'Publish Settings' section click the 'Trophy Pack' browse button and select the trophy pack located in the Assets/Editor/VitaPublishData folder.

Game Boot Message and/or Game Joining Presence.

If you game supports this then tick the 'Game Boot Message and/or Game Joining Presence' option in the publish settings, if it does not then be sure and un-tick it.

NP Communications

The example will not do much without a valid NP communications ID, NP pass phrase and NP signature, to set these go to the PS Vita player settings and copy the values shown below into each of the sections in 'Publish Settings'...

'NP Communications ID'
NPWR05558_00

'NP Communications Passphrase'
0xb3,0x95,0x05,0xc5,0x1e,0xf4,0xcf,0x66,
0xd2,0x21,0x16,0xe8,0x39,0x61,0xe1,0xf2,
0x96,0x12,0x99,0xf2,0x8f,0x0f,0x2d,0x16,
0x6c,0x99,0xf6,0x87,0xc9,0x38,0x56,0xc3,
0x73,0xd1,0xaa,0xeb,0x53,0x76,0x8e,0xae,
0xdc,0x86,0x8d,0x10,0xe3,0x01,0x2c,0x49,
0x2e,0x0d,0x88,0x3c,0x53,0xfb,0xfe,0x05,
0x0c,0xa7,0x5b,0x62,0x90,0xea,0x60,0x16,
0x99,0x89,0xca,0x10,0x5d,0x25,0x71,0x4e,
0x29,0xb9,0x62,0x21,0xe9,0xdd,0xd2,0x97,
0xe5,0x47,0x38,0x9e,0x0d,0xdb,0x6e,0x5a,
0x8e,0x12,0xc7,0xf0,0x18,0x4a,0xb7,0xa6,
0x20,0xe9,0xe1,0x12,0xcb,0x44,0x36,0x05,
0x3a,0x86,0xd4,0x43,0xf6,0x99,0xcc,0xe2,
0x45,0xfc,0x64,0x25,0x0c,0x61,0x51,0xe7,
0xcc,0xfc,0xc7,0x31,0xb9,0x1f,0x34,0xcd

'NP Communications Signature'
0xb9,0xdd,0xe1,0x3b,0x01,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0xca,0xb0,0xab,0x0f,
0x86,0xb4,0x53,0x52,0xe8,0x28,0x36,0x91,
0xea,0x2e,0xa6,0x33,0x4d,0xbd,0x9f,0x68,
0x16,0x8a,0xba,0x2b,0xe1,0xbc,0x06,0xb2,
0x84,0xa4,0xdd,0x61,0x9e,0xb0,0xb8,0xfb,
0xf8,0xcf,0xee,0x7e,0x1d,0xd2,0xc1,0xc2,
0xc9,0x8a,0x7b,0x0a,0xa5,0x59,0xd1,0xd2,
0x18,0xd5,0xea,0xb9,0x07,0x1c,0xbb,0x64,
0x98,0x9a,0xc2,0xe1,0x98,0x66,0xb0,0x7c,
0xee,0xb0,0x2e,0x16,0x58,0x63,0x77,0xab,
0x8d,0x68,0x52,0x38,0x4c,0x4a,0xf3,0x8a,
0x7c,0x93,0x33,0xec,0x37,0xdf,0x66,0xe0,
0x17,0x92,0xde,0xdd,0x36,0x05,0xd4,0x85,
0x18,0x1b,0x5f,0x1e,0x23,0x78,0x36,0x21,
0x72,0x69,0xbd,0x3d,0x7f,0x16,0xba,0x2b,
0x14,0x6d,0x87,0xac,0x4d,0x08,0xc3,0xd1,
0x8b,0x9b,0x7e,0x0b,0xd4,0x90,0xf5,0x6c,
0xe3,0x2e,0x08,0xba,0x11,0xb8,0xb0,0x81,
0x2a,0x5b,0xdf,0x33,0x68,0x55,0x94,0x6c
