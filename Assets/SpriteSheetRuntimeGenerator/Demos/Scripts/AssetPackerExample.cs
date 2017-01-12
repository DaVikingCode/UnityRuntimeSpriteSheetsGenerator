using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssetPackerExample : MonoBehaviour {
	
	public Image anim;

	AssetPacker assetPacker;
	
	void Start () {

		CopyPasteFoldersAndPNG(Application.dataPath + "/SpriteSheetRuntimeGenerator/Demos/StreamingAssets", Application.persistentDataPath);

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		assetPacker = GetComponent<AssetPacker>();

		assetPacker.OnProcessCompleted.AddListener(LaunchAnimations);

		assetPacker.AddItemsToRaster(files);
		assetPacker.Process();
	}

	void LaunchAnimations() {

		StartCoroutine(LoadAnimation());
	}

	IEnumerator LoadAnimation() {

		Sprite[] sprites = assetPacker.GetSprites("walking");

		int j = 0;
		while (j < sprites.Length) {

			anim.sprite = sprites[j++];

			yield return new WaitForSeconds(0.1f);

			if (j == sprites.Length)
				j = 0;
		}
	}

	void CopyPasteFoldersAndPNG(string SourcePath, string DestinationPath) {

		foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

		foreach (string newPath in Directory.GetFiles(SourcePath, "*.png", SearchOption.AllDirectories))
			File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
	}

}
