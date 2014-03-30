﻿using UnityEngine;
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
	 void file_save(string data){
         string folder = Directory.GetCurrentDirectory() + "\\Saved Data\\";
         string file = Directory.GetCurrentDirectory () + "\\Saved Data\\"+player_name;
         
         if (!Directory.Exists(folder))
         {
             Directory.CreateDirectory(folder);
         }
         StreamWriter sw = new StreamWriter(file);
         if (!System.IO.File.Exists(file))
         {
             sw = new StreamWriter(file);
         }
		
		
		FileInfo Filesystem = new FileInfo(file);
		Filesystem.IsReadOnly = false;
		Filesystem.Refresh ();
		sw.WriteLine (data);
		sw.Close ();
		
	}
	public JSONNode file_load(){

		string file = Directory.GetCurrentDirectory () + "\\Saved Data\\"+player_name;
        StreamReader sr;
        try
        {
           sr = new StreamReader(file);
        }
        catch
        {
            create_new();
        }
        sr = new StreamReader(file);
		FileInfo myFileInfo = new FileInfo(file);
		myFileInfo.IsReadOnly = false;
		myFileInfo.Refresh ();
		var lines  = sr.ReadLine();
		sr.Close();
		var Data =JSONNode.LoadFromBase64(lines);
		return Data;
	}

    public void add_checkpoint(int level, string game_object, string boxes_checkpoint, Vector3 player_pos)
	{
		var data = file_load ();
		data ["array"] [1] ["Level"] = level.ToString ();
        data["array"][1]["checkpoint"] = game_object;
        data["array"][1]["boxes"] = boxes_checkpoint;
        data["array"][1]["player_pos"] = player_pos.ToString();
		data = data.SaveToBase64();
		file_save (data);	
	}

}