using UnityEngine;

public class StartupScript : MonoBehaviour
{
	void Awake()
	{
		new EntryPoint().Execute();
	}
}
