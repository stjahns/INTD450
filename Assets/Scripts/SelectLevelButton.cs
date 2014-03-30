using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SelectLevelButton : MenuButton
{
	public int level;
	public TextMesh text;
	public LevelLoader loader;
	public SpriteRenderer levelBlockedSprite;

	private int savedLevel;

	override public void Start()
	{
		base.Start();
		text.text = level.ToString();

        try
        {
            Save_Load load = new Save_Load();
            load.player_name = "player";
            var data = load.file_load();
            savedLevel = Mathf.Max(1, System.Convert.ToInt32(data["array"][1]["Level"]));
        }
        catch 
        {
            savedLevel = 0; 
        }

        if (savedLevel >= level)
		{
			// hide the red x thing, we've reached this level
			levelBlockedSprite.enabled = false;
		}
		else
		{
			// disable button function, level is blocked
			collider.enabled = false;
		}
	}

	override public void OnMouseUp()
	{
		base.OnMouseUp();
		loader.LoadLevel(level);
	}
}
