using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;



public class Save_Load 
{
	public int score = 0;
	public int level = 0;
	public string player_name = "";
	 
	public Save_Load()
	{
		//Empty for Now
	}

	public void create_new()
	{
		var data = JSONNode.Parse("{\"Player name\":\""+player_name+"\", \"array\":[1,{\"data\":\"value\"}]}");
		data ["array"] [1] ["name"] = player_name;
		data ["array"] [1] ["Score"] = score.ToString();
		data ["array"] [1] ["Level"] = level.ToString();
		data ["array"] [1] ["checkpoint"] = "Null";
		data = data.SaveToBase64();	
		file_save (data);
	}
	 void file_save(string data){
		string file = Directory.GetCurrentDirectory () + "\\Saved Data\\"+player_name;
		StreamWriter sw = new StreamWriter (file);
		FileInfo Filesystem = new FileInfo(file);
		Filesystem.IsReadOnly = false;
		Filesystem.Refresh ();
		sw.WriteLine (data);
		sw.Close ();
		
	}
	public JSONNode file_load(){

		string file = Directory.GetCurrentDirectory () + "\\Saved Data\\"+player_name;
		StreamReader sr = new StreamReader(file);
		FileInfo myFileInfo = new FileInfo(file);
		myFileInfo.IsReadOnly = false;
		myFileInfo.Refresh ();
		var lines  = sr.ReadLine();
		sr.Close();
		var Data =JSONNode.LoadFromBase64(lines);
		return Data;
	}

    public void add_checkpoint(int level, string game_object)
	{
		var data = file_load ();
		data ["array"] [1] ["Level"] = level.ToString ();
        data["array"][1]["checkpoint"] = game_object;
		data = data.SaveToBase64();
		file_save (data);	
	}

}