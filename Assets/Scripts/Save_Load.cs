using UnityEngine;
using SimpleJSON;
using System.IO;


public class Save_Load : MonoBehaviour
{
	public int score = 0;
	public int level = 0;
	public string player_name = "";
	 
	
	
	public bool create_new()
	{
		bool flag = false;
		var data = JSONNode.Parse("{\"Player name\":\""+player_name+"\", \"array\":[1,{\"data\":\"value\"}]}");
		data ["array"] [1] ["name"] = player_name;
		data["array"][1]["Score"] = score.ToString();
		data ["array"] [1] ["Level"] = level.ToString();
		data = data.SaveToBase64();	
		file_save (data);
		return flag;
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
	public string file_load(){
		
		string file = Directory.GetCurrentDirectory () + "\\Saved Data\\"+player_name;
		StreamReader sr = new StreamReader(file);
		FileInfo myFileInfo = new FileInfo(file);
		myFileInfo.IsReadOnly = false;
		myFileInfo.Refresh ();
		var lines  = sr.ReadLine();
		sr.Close();
		var Data =JSONNode.LoadFromBase64(lines);
		return lines;
	}

}