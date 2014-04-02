using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SelectLevelButton : MenuButton
{
	public int level;
	public TextMesh text;
	public LevelLoader loader;

	public SpriteRenderer levelScreen;

	private int savedLevel;

	override public void Start()
	{
		base.Start();
		text.text = level.ToString();
		text.renderer.sortingLayerName = "UI";
		text.renderer.sortingOrder = 10;

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

        if (savedLevel < level)
		{
			if (levelScreen)
			{
				levelScreen.enabled = false;
			}

			// disable button function, level is blocked
			collider.enabled = false;

			// make everything translucent
			foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
			{
				Color color = renderer.color;
				color.a = 0.7f;
				renderer.color = color;
			}
		}
	}

	override public void OnMouseUp()
	{
		base.OnMouseUp();
		loader.LoadLevel(level);
	}
}
