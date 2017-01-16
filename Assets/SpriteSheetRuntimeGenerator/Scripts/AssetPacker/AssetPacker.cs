using DaVikingCode.RectanglePacking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace DaVikingCode.AssetPacker {

	public class AssetPacker : MonoBehaviour {

		public UnityEvent OnProcessCompleted;
		public float pixelsPerUnit = 100.0f;

		public bool useCache = false;
		public string cacheName = "";
		public int cacheVersion = 1;
		public bool deletePreviousCacheVersion = true;

		protected Dictionary<string, Sprite> mSprites = new Dictionary<string, Sprite>();
		protected List<TextureToPack> itemsToRaster = new List<TextureToPack>();

		protected bool allow4096Textures = false;

		public void AddTextureToPack(string file, string customID = null) {

			itemsToRaster.Add(new TextureToPack(file, customID != null ? customID : Path.GetFileNameWithoutExtension(file)));
		}

		public void AddTexturesToPack(string[] files) {

			foreach (string file in files)
				AddTextureToPack(file);
		}

		public void Process(bool allow4096Textures = false) {

			this.allow4096Textures = allow4096Textures;

			if (useCache) {

				if (cacheName == "")
					throw new Exception("No cache name specified");

				string path = Application.persistentDataPath + "/AssetPacker/" + cacheName + "/" + cacheVersion + "/";

				bool cacheExist = Directory.Exists(path);

				if (!cacheExist)
					StartCoroutine(createPack(path));
				else
					StartCoroutine(loadPack(path));
				
			} else
				StartCoroutine(createPack());
			
		}

		protected IEnumerator createPack(string savePath = "") {

			List<Texture2D> textures = new List<Texture2D>();
			List<string> images = new List<string>();

			foreach (TextureToPack itemToRaster in itemsToRaster) {

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

			int numSpriteSheet = 0;
			while (mRectangles.Count > 0) {

				Texture2D mTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
				Color32[] mFillColor = mTexture.GetPixels32();
				for (int i = 0; i < mFillColor.Length; ++i)
					mFillColor[i] = Color.clear;

				RectanglePacker mPacker = new RectanglePacker(mTexture.width, mTexture.height, padding);

				for (int i = 0; i < mRectangles.Count; i++)
					mPacker.insertRectangle((int) mRectangles[i].width, (int) mRectangles[i].height, i);

				mPacker.packRectangles();

				if (mPacker.rectangleCount > 0) {

					mTexture.SetPixels32(mFillColor);
					IntegerRectangle rect = new IntegerRectangle();
					List<TextureAsset> textureAssets = new List<TextureAsset>();

					List<Rect> garbageRect = new List<Rect>();
					List<Texture2D> garabeTextures = new List<Texture2D>();
					List<string> garbageImages = new List<string>();

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

						garbageRect.Add(mRectangles[index]);
						garabeTextures.Add(textures[index]);
						garbageImages.Add(images[index]);
					}

					foreach (Rect garbage in garbageRect)
						mRectangles.Remove(garbage);

					foreach (Texture2D garbage in garabeTextures)
						textures.Remove(garbage);

					foreach (string garbage in garbageImages)
						images.Remove(garbage);

					mTexture.Apply();

					if (savePath != "") {

						if (deletePreviousCacheVersion && Directory.Exists(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/"))
							Directory.Delete(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/", true);

						Directory.CreateDirectory(savePath);

						File.WriteAllBytes(savePath + "/data" + numSpriteSheet + ".png", mTexture.EncodeToPNG());
						File.WriteAllText(savePath + "/data" + numSpriteSheet + ".json", JsonUtility.ToJson(new TextureAssets(textureAssets.ToArray())));
						++numSpriteSheet;
					}

					foreach (TextureAsset textureAsset in textureAssets)
						mSprites.Add(textureAsset.name, Sprite.Create(mTexture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
				}

			}

			OnProcessCompleted.Invoke();
		}

		protected IEnumerator loadPack(string savePath) {
			
			int numFiles = Directory.GetFiles(savePath).Length;

			for (int i = 0; i < numFiles / 2; ++i) {

				WWW loaderTexture = new WWW("file:///" + savePath + "/data" + i + ".png");
				yield return loaderTexture;

				WWW loaderJSON = new WWW("file:///" + savePath + "/data" + i + ".json");
				yield return loaderJSON;

				TextureAssets textureAssets = JsonUtility.FromJson<TextureAssets> (loaderJSON.text);

				foreach (TextureAsset textureAsset in textureAssets.assets)
					mSprites.Add(textureAsset.name, Sprite.Create(loaderTexture.texture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
			}

			yield return null;

			OnProcessCompleted.Invoke();
		}

		public void Dispose() {

			foreach (var asset in mSprites)
				Destroy(asset.Value.texture);

			mSprites.Clear();
		}

		void Destroy() {

			Dispose();
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
}
