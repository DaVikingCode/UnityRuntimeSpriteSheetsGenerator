using DaVikingCode.AssetPacker;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssetPackerExample : MonoBehaviour {
	
	public Image anim;

	AssetPacker assetPacker;
	
	void Start () {

		// We just copy and paste files, so you don't have to do it manually.
		CopyPasteFoldersAndPNG(Application.dataPath + "/RuntimeSpriteSheetsGenerator/Demos/Sprites", Application.persistentDataPath);

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		assetPacker = GetComponent<AssetPacker>();

		assetPacker.OnProcessCompleted.AddListener(LaunchAnimations);

		assetPacker.AddTexturesToPack(files);
		assetPacker.Process();
	}

	void LaunchAnimations() {

		StartCoroutine(LoadAnimation());
	}

	IEnumerator LoadAnimation() {

		Sprite[] sprites = assetPacker.GetSprites("walking");

		int i = 0;
		while (i < sprites.Length) {

			anim.sprite = sprites[i++];

			yield return new WaitForSeconds(0.1f);

			if (i == sprites.Length)
				i = 0;
		}
	}

	void CopyPasteFoldersAndPNG(string SourcePath, string DestinationPath) {

		foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

		foreach (string newPath in Directory.GetFiles(SourcePath, "*.png", SearchOption.AllDirectories))
			File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
	}

}
