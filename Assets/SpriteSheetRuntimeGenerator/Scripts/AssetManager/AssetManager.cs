using DaVikingCode.RectanglePacking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class AssetManager : MonoBehaviour {

	public UnityEvent OnProcessCompleted;

	protected List<ItemToRaster> itemsToRaster = new List<ItemToRaster>();

	protected List<Texture2D> textures = new List<Texture2D>();
	protected List<string> images = new List<string>();

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

		mPacker = new RectanglePacker(mTexture.width, mTexture.height, padding);
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

				TextureAsset texture = new TextureAsset();
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

			OnProcessCompleted.Invoke();
		}

	}
}
