using DaVikingCode.RectanglePacking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadTextures : MonoBehaviour {

	public Image img;
	public Image anim;

	Texture2D mTexture;
	Color32[] mFillColor;

	private RectanglePacker mPacker;

	List<Texture2D> textures = new List<Texture2D>();
	List<string> images = new List<string>();

	List<Rect> mRectangles = new List<Rect>();
	
	void Start () {

		mTexture = new Texture2D(2048, 2048, TextureFormat.ARGB32, false);

		mFillColor = mTexture.GetPixels32();

		for (int i = 0; i < mFillColor.Length; ++i)
			mFillColor[i] = Color.clear;

		img.sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), Vector2.zero);

		CopyPasteFoldersAndPNG(Application.dataPath + "/SpriteSheetRuntimeGenerator/Demos/StreamingAssets", Application.persistentDataPath);
		StartCoroutine(LoadAllTextures());
	}

	IEnumerator LoadAllTextures() {

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		foreach (string file in files) {

			WWW loader = new WWW("file:///" + file);

			yield return loader;

			textures.Add(loader.texture);
			images.Add(Path.GetFileNameWithoutExtension(file));
		}

		createRectangles();

		updateRectangles();
	}

	private void createRectangles() {

		for (int i = 0; i < textures.Count; i++)
			mRectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));
	}

	private void updateRectangles() {

		const int padding = 1;

		if (mPacker == null)
			mPacker = new RectanglePacker(mTexture.width, mTexture.height, padding);
		else
			mPacker.reset(mTexture.width, mTexture.height, padding);

		for (int i = 0; i < textures.Count; i++)
			mPacker.insertRectangle((int) mRectangles[i].width, (int) mRectangles[i].height, i);

		mPacker.packRectangles();

		if (mPacker.rectangleCount > 0) {

			mTexture.SetPixels32(mFillColor);
			IntegerRectangle rect = new IntegerRectangle();

			List<TextureAsset> textureAssets = new List<TextureAsset>();

			for (int j = 0; j < mPacker.rectangleCount; j++) {

				rect = mPacker.getRectangle(j, rect);

				int index = mPacker.getRectangleId(j);

				mTexture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());

				TextureAsset texture = new TextureAsset ();
				texture.x = rect.x;
				texture.y = rect.y;
				texture.width = rect.width;
				texture.height = rect.height;
				texture.name = images[index];

				textureAssets.Add(texture);
			}

			mTexture.Apply();

			Directory.CreateDirectory(Application.persistentDataPath + "/Test/");

			byte[] bytes = mTexture.EncodeToPNG();
			File.WriteAllBytes(Application.persistentDataPath + "/Test/data.png", bytes);

			TextureAssets assets = new TextureAssets(textureAssets.ToArray());
			File.WriteAllText(Application.persistentDataPath + "/Test/data.json", JsonUtility.ToJson(assets));

			StartCoroutine(LoadAnimation());
		}
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
