using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadTextures : MonoBehaviour {
	
	public Image anim;
	
	void Start () {

		CopyPasteFoldersAndPNG(Application.dataPath + "/SpriteSheetRuntimeGenerator/Demos/StreamingAssets", Application.persistentDataPath);

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		AssetManager assetManager = GetComponent<AssetManager>();

		assetManager.OnProcessCompleted.AddListener(LaunchAnimations);

		assetManager.AddItemsToRaster(files);
		assetManager.Process();
	}

	void LaunchAnimations() {

		StartCoroutine(LoadAnimation ());
	}

	IEnumerator LoadAnimation() {

		WWW loaderTexture = new WWW("file:///" + Application.persistentDataPath + "/Test/data.png");
		yield return loaderTexture;

		WWW loaderJSON = new WWW("file:///" + Application.persistentDataPath + "/Test/data.json");
		yield return loaderJSON;

		TextureAssets textureAssets = JsonUtility.FromJson<TextureAssets>(loaderJSON.text);

		Dictionary<string, TextureAsset> assets = new Dictionary<string, TextureAsset>();
		foreach (TextureAsset textureAsset in textureAssets.assets)
			assets.Add(textureAsset.name, textureAsset);

		TextureAsset asset = null;
		assets.TryGetValue("walking0004", out asset);

		Color[] colors = loaderTexture.texture.GetPixels(asset.x, asset.y, asset.width, asset.height);

		Texture2D texture = new Texture2D(asset.width, asset.height, TextureFormat.ARGB32, false);

		anim.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

		anim.sprite.texture.SetPixels(colors);

	}

	void CopyPasteFoldersAndPNG(string SourcePath, string DestinationPath) {

		foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

		foreach (string newPath in Directory.GetFiles(SourcePath, "*.png", SearchOption.AllDirectories))
			File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
	}

}
