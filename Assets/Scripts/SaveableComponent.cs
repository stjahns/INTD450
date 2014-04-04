using SimpleJSON;

public interface SaveableComponent
{
	void SaveState(JSONNode data);
	void LoadState(JSONNode data);
}
