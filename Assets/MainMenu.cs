using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public void PlayGexis()
	{
		PlayerPrefs.SetInt("game_type", 6);
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		SceneManager.LoadScene("Game");
	}

	public void PlaySeptis()
	{
		PlayerPrefs.SetInt("game_type", 7);
		SceneManager.LoadScene("Game");
	}

	public void PlayOctis()
	{
		PlayerPrefs.SetInt("game_type", 8);
		SceneManager.LoadScene("Game");
	}
	
	public void QuitGame()
	{
		Debug.Log("Quit in progress!");
		Application.Quit();
	}
}
