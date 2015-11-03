
Unity PS4 Save Game API.

This example project demonstrates how to perform saving and loading of game state using the Unity PS4 Save Game API

Project Folder Structure.

	Plugins/PS4 - Contains the SavedGames native plugin.
	Editor/SonyPS4SavedGamesPublishData - Contains required data for publishing.
	StreamingAssets - Contains the icon images for use with save games.
	SonyAssemblies - Contains the SonyPS4SavedGames managed interface to the SavedGames plugin.
	SonyExample/SavedGames - Contains a Unity scene which runs the scripts.
	SonyExample/SavedGames/Scripts - Contains the example scripts.
	SonyExample/Utils - Contains various utility scripts for use by the example.

The SonyPS4SavedGames managed assembly defines the following namespaces...

Sony.PS4.SavedGame.Main		Contains methods for initialising and updating the plugin.
Sony.PS4.SavedGame.SaveLoad	Contains methods for saving and loading and managing saved games.

Sony.PS4.SavedGame.Main

	Methods.

		public static void Initialise()
			Initialise the plugin, this is required before you can save data (call once).
			
		public static void Terminate()
			Terminate the plugin and free all resources, this can be called if you know you aren't going to be needing the save data system for a while.
			
		public static void Update()
			Updates the plugin, call once each frame.

Sony.PS4.SavedGame.SaveLoad

	Structures.

		public struct SavedGameSlotParams	structure defining save data
		{
			public int userId;				The PS4 user id that is saving the data, when 0 the user that started the game is used
			private int reserved;
			public string titleId;			The game titleId, leave null to use the titleId defined in the publish settings
			public string dirName;			When silently loading, saving, or deleting this contains the name of the savegame to use (leave null if not silent)
			public string fileName;			The name of the file that is being saved in the save data 
			public string newTitle;			Text string that is displayed when creating a new save data
			public string title;			Text string that is displayed as the title of the save data
			public string subTitle;			Additional data that can be used to describe what is in the save data
			public string detail;			Further additonal detail to the save data 
			public string iconPath;			Path to the png icon to use for the save data
			public long sizeKiB;			Size of the data to save in KB
		}

	Events.

		OnGameSaved				Triggered when a game save has completed.
		OnGameLoaded			Triggered when a game load has completed.
		OnGameDeleted			Triggered when a game has been deleted.
		OnCanceled				Triggered when a save/load operation was canceled or aborted.
        OnSaveError				Triggered if a save failed, i.e. no space or missing device.
        OnLoadError				Triggered if a load failed, i.e. corrupt save slot.
		OnLoadNoData			Triggered if there was no data in the slot being loaded.

	Properties.

		public static bool IsDialogOpen
			Is the save/load dialog open? This also true when an error dialog is display at the end of an auto save/load.

		public static bool IsBusy
			Is the save/load process busy?

	Methods.

		public static bool LoadGame(SaveLoad.SavedGameSlotParams slotParams, bool useDialogs)
			Load data that have previously been saved, either using the sony dialog system, or sliently. Loaded can is retrieved using GetLoadedGame()
	
		public static bool SaveGame(byte[] data, SaveLoad.SavedGameSlotParams slotParams, bool useDialogs)
			Save data, either using the sony dialog system, or silently.

		public static bool Delete(SaveLoad.SavedGameSlotParams slotParams, bool useDialogs)
			Delete a saved game, either using the sony dialog system, or silently

		public static byte[] GetLoadedGame()
			Retrieve the data that was just loaded, use this method if you game only requires 1 save slot.
