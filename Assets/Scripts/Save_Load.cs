using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;



public class Save_Load 
{
	public int score = 0;
	public int level = 0;
	public string player_name = "";

	public void create_new()
	{
		var data = JSONNode.Parse("{\"Player name\":\""+player_name+"\", \"array\":[1,{\"data\":\"value\"}]}");
		data ["array"] [1] ["name"] = player_name;
		data ["array"] [1] ["Score"] = score.ToString();
		data ["array"] [1] ["Level"] = level.ToString();
		data ["array"] [1] ["checkpoint"] = "Null";
		data["array"][1]["boxes"] = "Null";
		data ["array"]  [1]["player_pos"] = "Null";
		data = data.SaveToBase64();	
		file_save (data);
	}

	void file_save(string data)
	{
		string folder = Path.Combine(Directory.GetCurrentDirectory(), "Saved Data");
		string file = Path.Combine(folder, player_name);

		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}

		StreamWriter sw = new StreamWriter(file);

		FileInfo Filesystem = new FileInfo(file);
		Filesystem.IsReadOnly = false;
		Filesystem.Refresh ();
		sw.WriteLine (data);
		sw.Close ();

	}

	public JSONNode file_load()
	{
		string folder = Path.Combine(Directory.GetCurrentDirectory(), "Saved Data");
		string file = Path.Combine(folder, player_name);

		if (!System.IO.File.Exists(file))
		{
			create_new();
		}

		StreamReader sr;
		sr = new StreamReader(file);
		FileInfo myFileInfo = new FileInfo(file);
		myFileInfo.IsReadOnly = false;
		myFileInfo.Refresh ();
		var lines = sr.ReadLine();
		sr.Close();
		var Data =JSONNode.LoadFromBase64(lines);
		return Data;
	}

	public void add_checkpoint(int level, string game_object, string boxes_checkpoint, Vector3 player_pos, bool resetComponents = false)
	{
		var data = file_load ();
		data ["array"] [1] ["Level"] = level.ToString ();
		data["array"][1]["checkpoint"] = game_object;
		data["array"][1]["boxes"] = boxes_checkpoint;
		data["array"][1]["player_pos"] = player_pos.ToString();

		// Additional state saving...
		if (resetComponents)
		{
			ResetComponents<ChainComponent>(data["array"][1]);
			ResetComponents<SteakComponent>(data["array"][1]);
		}
		else
		{
			SaveComponents<ChainComponent>(data["array"][1]);
			SaveComponents<SteakComponent>(data["array"][1]);
		}

		data = data.SaveToBase64();
		file_save (data);	
	}

	void ResetComponents<T>(JSONNode data) where T : Object, SaveableComponent
	{
		// clear existing data
		data[typeof(T).Name] = new JSONClass();
	}

	void SaveComponents<T>(JSONNode data) where T : Object, SaveableComponent
	{
		T[] components = Object.FindObjectsOfType<T>() as T[];

		// clear existing data
		data[typeof(T).Name] = new JSONClass();

		foreach (T component in components)
		{
			component.SaveState(data[typeof(T).Name]);
		}

		if (data[typeof(T).Name].Count == 0)
		{
			data.Remove(typeof(T).Name);
		}
	}

	public void LoadComponents<T>(JSONNode data) where T : Object, SaveableComponent
	{
		T[] components = Object.FindObjectsOfType<T>() as T[];

		foreach (T component in components)
		{
			component.LoadState(data[typeof(T).Name]);
		}
	}
}
