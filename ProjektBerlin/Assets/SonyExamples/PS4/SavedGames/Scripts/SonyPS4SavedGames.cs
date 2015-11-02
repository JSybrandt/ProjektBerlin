using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class SonyPS4SavedGames : MonoBehaviour, IScreen
{
    int levelNum = 0;
    int score = 0;

	const int kSaveDataMaxSize = 64*1024;

	MenuStack menuStack;
    MenuLayout menuMain;
	System.Random rand;
	Texture2D savedataIcon = null;
	
    // Class containing game data and methods for reading/writing from/to a byte buffer, the data could be almost anything.
    class GameData
    {
        public int level;
        public int score;

        public GameData()
        {
  
        }

        public byte[] WriteToBuffer()
        {
            System.IO.MemoryStream output = new MemoryStream(kSaveDataMaxSize);
            System.IO.BinaryWriter writer = new BinaryWriter(output);
            writer.Write(level);
            writer.Write(score);
             writer.Close();
            return output.GetBuffer();
        }

        public void ReadFromBuffer(byte[] buffer)
        {
            System.IO.MemoryStream input = new MemoryStream(buffer);
            System.IO.BinaryReader reader = new BinaryReader(input);
            level = reader.ReadInt32();
            score = reader.ReadInt32();

            reader.Close();
        }
    }
    
    void Start()
	{
		menuMain = new MenuLayout(this, 450, 34);
		menuStack = new MenuStack();
		menuStack.SetMenu(menuMain);

		Sony.PS4.SavedGame.Main.OnLog += OnLog;
        Sony.PS4.SavedGame.Main.OnLogWarning += OnLogWarning;
        Sony.PS4.SavedGame.Main.OnLogError += OnLogError;
        Sony.PS4.SavedGame.SaveLoad.OnGameSaved += OnSavedGameSaved;
        Sony.PS4.SavedGame.SaveLoad.OnGameLoaded += OnSavedGameLoaded;
        Sony.PS4.SavedGame.SaveLoad.OnGameDeleted += OnSavedGameDeleted;		
        Sony.PS4.SavedGame.SaveLoad.OnCanceled += OnSavedGameCanceled;
        Sony.PS4.SavedGame.SaveLoad.OnSaveError += OnSaveError;
        Sony.PS4.SavedGame.SaveLoad.OnLoadError += OnLoadError;
        Sony.PS4.SavedGame.SaveLoad.OnLoadNoData += OnLoadNoData;

		Sony.PS4.SavedGame.Main.Initialise();
		
		rand = new System.Random(0);

    }

	public void Process(MenuStack stack)
	{
		if (stack.GetMenu() == menuMain)
		{
			MainMenu();
		}
	}	
	
	
	void Update ()
    {
        Sony.PS4.SavedGame.Main.Update();
	}
	
	
	
    void MainMenu()
    {
        menuMain.Update();

        if (menuMain.AddItem("Save Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuSave(true); }
        if (menuMain.AddItem("Load Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuLoad(true); }
        if (menuMain.AddItem("Delete Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) {  MenuDelete(true); }
        if (menuMain.AddItem("AutoSave Data Exists", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuExists(); }
        if (menuMain.AddItem("AutoSave Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuSave(false); }
        if (menuMain.AddItem("AutoLoad Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuLoad(false); }
        if (menuMain.AddItem("Delete Auto Data", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) {  MenuDelete(false); }
        if (menuMain.AddItem("SaveData Details", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { MenuDetails(); }

        if (menuMain.AddItem("initialise save game system", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { Sony.PS4.SavedGame.Main.Initialise(); }
        if (menuMain.AddItem("terminate save game system", !Sony.PS4.SavedGame.SaveLoad.IsBusy)) { Sony.PS4.SavedGame.Main.Terminate(); }

    }

	void SetupGameParams(ref Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams  saveParams)
	{
		saveParams.userId=0;	// by passing a userId of 0 we use the default user that started the title
		saveParams.titleId=null; // by passing null we use the game's title id from the publishing settings
		saveParams.dirName=""; //"unitySaveGames";
		saveParams.fileName="unitySaveData";
		saveParams.sizeKiB = kSaveDataMaxSize/1024;		// is this max size of the file or the folder ?
	}
	
    void MenuSave(bool useDialogs)
    {
        // Construct some game data to save.
        GameData data = new GameData();

        data.level = 1;
        data.score = 123456789;
        byte[] bytes = data.WriteToBuffer();

        Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams slotParams = new Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams();
		SetupGameParams(ref slotParams);
		
		if (useDialogs==false)	
		{
			slotParams.dirName="autoSaveData"; // if we are autosaving we need to provide a directory 'slot' to save it in
			slotParams.title = "PS4 AutoSaveData";
			slotParams.newTitle = "";		// displayed in the box that can be selected to create a new savegame
			slotParams.subTitle = "Autosave file";
			slotParams.detail = "The autosave file for the Save Game test project ";
			slotParams.iconPath = Application.streamingAssetsPath + "/SaveIcon.png";
		}
		else
		{		
			bool saveGameNameIsValid = false;
			while(saveGameNameIsValid == false)
			{
				int randomNumber = rand.Next(1, 100);
				slotParams.title = "Unity PS4 Example SaveGame" + randomNumber;
				slotParams.newTitle = "A new SaveGame " + randomNumber;		// displayed in the box that can be selected to create a new savegame
				slotParams.subTitle = "This can contain summary of the save game " + randomNumber;
				slotParams.detail = "This can contain much more detail about the save game " + randomNumber;
				slotParams.iconPath = Application.streamingAssetsPath + "/SaveIcon.png";
				slotParams.dirName = "SAVEDATA" + randomNumber;

				OnScreenLog.Add( "Checking if " + slotParams.title + " ( " + slotParams.dirName + " ) exists.");
				uint result = Sony.PS4.SavedGame.SaveLoad.Exists(slotParams);
				if (result != 0)
				{
					//OnScreenLog.Add( System.String.Format("SaveData DOESN'T exist" ));
					saveGameNameIsValid = true;
				}
			}
		}

        OnScreenLog.Add("Saving Game... " + slotParams.title);
        OnScreenLog.Add(" level: " + data.level);
        OnScreenLog.Add(" score: " + data.score);
        OnScreenLog.Add(" icon: " + slotParams.iconPath);
		OnScreenLog.Add(" Directory: " + slotParams.dirName);

		Sony.PS4.SavedGame.SaveLoad.SaveGame(bytes,  slotParams, useDialogs);
    }

	
	// trivial example of how to test to see if the auto save data exists
    void MenuExists()
    {
        Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams slotParams = new Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams();
		SetupGameParams(ref slotParams);
		slotParams.dirName="autoSaveData"; // if we are autosaving we need to provide a directory 'slot' to save it in

		uint result = Sony.PS4.SavedGame.SaveLoad.Exists(slotParams);
		if (result == 0)
		{
			OnScreenLog.Add( System.String.Format("autoSaveData DOES exist" ));
		}
		else if (result == 0x809f0008)
		{
			OnScreenLog.Add( System.String.Format("autoSaveData DOES NOT exist" ));
		}
		else
		{
			OnScreenLog.Add( System.String.Format("autoSaveData Exists returned 0x{0:X} " , result) );
		}
    }	
	
	// trivial example of how to get existing save data details
    void MenuDetails()
    {
        Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams slotParams = new Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams();
        Sony.PS4.SavedGame.SaveLoad.SaveGameSlotDetails slotDetails; // = new Sony.PS4.SavedGame.SaveLoad.SaveGameSlotDetails();
		SetupGameParams(ref slotParams);
		slotParams.dirName="autoSaveData"; // if we want regular saves, rather than the autosave slot, then use dirnames like: "SAVEDATA00" here
	
		uint result = Sony.PS4.SavedGame.SaveLoad.GetDetails(slotParams, out slotDetails);
		
		if (result == 0)
		{
			OnScreenLog.Add( System.String.Format("title:{0}", slotDetails.title ));
			OnScreenLog.Add( System.String.Format("subtitle:{0}", slotDetails.subTitle ));
			OnScreenLog.Add( System.String.Format("detail:{0}", slotDetails.detail ));
			OnScreenLog.Add( System.String.Format("userParam 0x{0:X} " , slotDetails.userParam) );

			long sceToDotNetTicks = 10;	// sce ticks are microsecond, .net are 100 nanosecond
			DateTime saveDateTime = new DateTime((long)slotDetails.time * sceToDotNetTicks);
			
			OnScreenLog.Add( System.String.Format("time of savedata {0} " , saveDateTime.ToString()) );
			savedataIcon = slotDetails.icon;
		}
		else if (result == 0x809f0008)
		{
			OnScreenLog.Add( System.String.Format("SaveData DOES NOT exist" ));
		}
		else
		{
			OnScreenLog.Add( System.String.Format("SaveData Exists returned 0x{0:X} " , result) );
		}
    }	
		
	
	
    void MenuLoad(bool useDialogs)
    {
		Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams slotParams = new Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams();
		SetupGameParams(ref slotParams);
		
		if (useDialogs==false)	
		{
			slotParams.dirName="autoSaveData"; // if we are autosaving we need to provide a directory 'slot' to save it in
		}
		
		Sony.PS4.SavedGame.SaveLoad.LoadGame(slotParams, useDialogs);
    }

    void MenuDelete(bool useDialogs)
    {
		Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams slotParams = new Sony.PS4.SavedGame.SaveLoad.SavedGameSlotParams();
		SetupGameParams(ref slotParams);
		
		if (useDialogs==false)	
		{
			slotParams.dirName="autoSaveData"; // if we are autosaving we need to provide a directory 'slot' to save it in
		}
		
		Sony.PS4.SavedGame.SaveLoad.Delete(slotParams, useDialogs);
    }
	
	

	void OnGUI()
	{
 		MenuLayout activeMenu = menuStack.GetMenu();
		activeMenu.GetOwner().Process(menuStack);


        ShowDialogState();

		// if we have a save data icon from the details menu ... then show it
		if (savedataIcon != null)
		{
		    GUI.DrawTexture(new Rect(64, 64, savedataIcon.width, savedataIcon.height), savedataIcon);
		}
		
		
    }

    void ShowDialogState()
    {
		GUIStyle style = GUI.skin.GetStyle("Label");
        style.fontSize = 16;
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = false;

        // Is a dialog open?
        string state = Sony.PS4.SavedGame.SaveLoad.IsDialogOpen ? "Dlg Open" : "Dlg Closed";
        GUI.Label(new Rect(Screen.width - 100, 0, Screen.width - 1, style.lineHeight * 2), state, style);
    
        // Is the save game process busy, i.e. saving or loading.
        state = Sony.PS4.SavedGame.SaveLoad.IsBusy ? "Busy" : "Not Busy";
        GUI.Label(new Rect(Screen.width - 100, style.lineHeight * 2, Screen.width - 1, style.lineHeight * 2), state, style);
    }

	void OnLog(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add(msg.Text);
	}

    void OnLogWarning(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("WARNING: " + msg.Text);
	}

    void OnLogError(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("ERROR: " + msg.Text);
	}

    void OnSavedGameSaved(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Game Saved!");
	}

    void OnSavedGameDeleted(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Game deleted!");
	}
	
	
    void OnSavedGameLoaded(Sony.PS4.SavedGame.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Game Loaded...");
        byte[] bytes = Sony.PS4.SavedGame.SaveLoad.GetLoadedGame();
        if (bytes != null)
        {
            // Read the game data from the buffer.
            GameData data = new GameData();
            data.ReadFromBuffer(bytes);
            OnScreenLog.Add(" level: " + data.level);
            OnScreenLog.Add(" score: " + data.score);
        }
        else
        {
            OnScreenLog.Add(" ERROR: No data");
        }
    }
    
    void OnSavedGameCanceled(Sony.PS4.SavedGame.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Canceled");
    }

    void OnSaveError(Sony.PS4.SavedGame.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Failed to save");
    }

    void OnLoadError(Sony.PS4.SavedGame.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Failed to load");
    }

    void OnLoadNoData(Sony.PS4.SavedGame.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Nothing to load");
    }


}
