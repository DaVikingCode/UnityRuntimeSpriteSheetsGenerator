using DaVikingCode.RectanglePacking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class AssetManager : MonoBehaviour {

	public UnityEvent OnProcessCompleted;

	protected Dictionary<string, Sprite> mSprites = new Dictionary<string, Sprite>();
	protected List<ItemToRaster> itemsToRaster = new List<ItemToRaster>();

	protected RectanglePacker mPacker;
	protected Texture2D mTexture;

	protected bool allow4096Textures = false;

	public void AddItemToRaster(string file, string customID = null) {

		itemsToRaster.Add(new ItemToRaster(file, customID != null ? customID : Path.GetFileNameWithoutExtension(file)));
	}

	public void AddItemsToRaster(string[] files) {

		foreach (string file in files)
			AddItemToRaster(file);
	}

	public void Process(bool allow4096Textures = false) {

		this.allow4096Textures = allow4096Textures;

		StartCoroutine(process());
	}

	protected IEnumerator process() {

		List<Texture2D> textures = new List<Texture2D>();
		List<string> images = new List<string>();

		foreach (ItemToRaster itemToRaster in itemsToRaster) {

			WWW loader = new WWW("file:///" + itemToRaster.file);

			yield return loader;

			textures.Add(loader.texture);
			images.Add(itemToRaster.id);
		}

		List<Rect> mRectangles = new List<Rect>();
		for (int i = 0; i < textures.Count; i++)
			mRectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));

		const int padding = 1;

		int textureSize = allow4096Textures ? 4096 : 2048;

		mTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

		Color32[] mFillColor = mTexture.GetPixels32();
		for (int i = 0; i < mFillColor.Length; ++i)
			mFillColor[i] = Color.clear;

		int numSpriteSheet = 0;
		while (mRectangles.Count > 0) {

			mPacker = new RectanglePacker(mTexture.width, mTexture.height, padding);
			for (int i = 0; i < mRectangles.Count; i++)
				mPacker.insertRectangle((int) mRectangles[i].width, (int) mRectangles[i].height, i);

			mPacker.packRectangles();

			if (mPacker.rectangleCount > 0) {

				mTexture.SetPixels32(mFillColor);
				IntegerRectangle rect = new IntegerRectangle();
				List<TextureAsset> textureAssets = new List<TextureAsset>();

				List<Rect> rectGarbages = new List<Rect>();

				for (int j = 0; j < mPacker.rectangleCount; j++) {

					rect = mPacker.getRectangle(j, rect);

					int index = mPacker.getRectangleId(j);

					mTexture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());

					TextureAsset texture = new TextureAsset();
					texture.x = rect.x;
					texture.y = rect.y;
					texture.width = rect.width;
					texture.height = rect.height;
					texture.name = images[index];

					textureAssets.Add(texture);

					rectGarbages.Add(mRectangles[index]);
				}

				foreach (Rect rectGarbage in rectGarbages) {

					int indexToDestroy = mRectangles.IndexOf(rectGarbage);

					mRectangles.RemoveAt(indexToDestroy);
					textures.RemoveAt(indexToDestroy);
					images.RemoveAt(indexToDestroy);
				}

				mTexture.Apply();

				Directory.CreateDirectory(Application.persistentDataPath + "/Test/");

				File.WriteAllBytes(Application.persistentDataPath + "/Test/data" + numSpriteSheet + ".png", mTexture.EncodeToPNG());
				File.WriteAllText(Application.persistentDataPath + "/Test/data" + numSpriteSheet + ".json", JsonUtility.ToJson(new TextureAssets(textureAssets.ToArray())));
				++numSpriteSheet;

				/*WWW loaderTexture = new WWW("file:///" + Application.persistentDataPath + "/Test/data.png");
				yield return loaderTexture;

				WWW loaderJSON = new WWW("file:///" + Application.persistentDataPath + "/Test/data.json");
				yield return loaderJSON;

				TextureAssets textureAssets = JsonUtility.FromJson<TextureAssets>(loaderJSON.text);*/

				foreach (TextureAsset textureAsset in textureAssets)
					mSprites.Add(textureAsset.name, Sprite.Create(mTexture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero));

			}

		}

		OnProcessCompleted.Invoke();
	}

	public Sprite GetSprite(string id) {

		Sprite sprite = null;

		mSprites.TryGetValue (id, out sprite);

		return sprite;
	}

	public Sprite[] GetSprites(string prefix) {

		List<string> spriteNames = new List<string>();
		foreach (var asset in mSprites)
			spriteNames.Add(asset.Key);

		spriteNames.Sort(StringComparer.Ordinal);

		List<Sprite> sprites = new List<Sprite>();
		Sprite sprite;
		for (int i = 0; i < spriteNames.Count; ++i) {

			mSprites.TryGetValue(spriteNames[i], out sprite);

			sprites.Add(sprite);
		}

		return sprites.ToArray();
	}
}
