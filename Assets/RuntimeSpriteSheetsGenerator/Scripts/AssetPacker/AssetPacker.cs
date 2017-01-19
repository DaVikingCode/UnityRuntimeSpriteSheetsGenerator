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

			if (savePath != "") {

				if (deletePreviousCacheVersion && Directory.Exists(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/"))
					foreach (string dirPath in Directory.GetDirectories(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/", "*", SearchOption.AllDirectories))
						Directory.Delete(dirPath, true);

				Directory.CreateDirectory(savePath);
			}

			List<Texture2D> textures = new List<Texture2D>();
			List<string> images = new List<string>();

			foreach (TextureToPack itemToRaster in itemsToRaster) {

				WWW loader = new WWW("file:///" + itemToRaster.file);

				yield return loader;

				textures.Add(loader.texture);
				images.Add(itemToRaster.id);
			}

			List<Rect> rectangles = new List<Rect>();
			for (int i = 0; i < textures.Count; i++)
				rectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));

			const int padding = 1;

			int textureSize = allow4096Textures ? 4096 : 2048;

			int numSpriteSheet = 0;
			while (rectangles.Count > 0) {

				Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
				Color32[] fillColor = texture.GetPixels32();
				for (int i = 0; i < fillColor.Length; ++i)
					fillColor[i] = Color.clear;

				RectanglePacker packer = new RectanglePacker(texture.width, texture.height, padding);

				for (int i = 0; i < rectangles.Count; i++)
					packer.insertRectangle((int) rectangles[i].width, (int) rectangles[i].height, i);

				packer.packRectangles();

				if (packer.rectangleCount > 0) {

					texture.SetPixels32(fillColor);
					IntegerRectangle rect = new IntegerRectangle();
					List<TextureAsset> textureAssets = new List<TextureAsset>();

					List<Rect> garbageRect = new List<Rect>();
					List<Texture2D> garabeTextures = new List<Texture2D>();
					List<string> garbageImages = new List<string>();

					for (int j = 0; j < packer.rectangleCount; j++) {

						rect = packer.getRectangle(j, rect);

						int index = packer.getRectangleId(j);

						texture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());

						TextureAsset textureAsset = new TextureAsset();
						textureAsset.x = rect.x;
						textureAsset.y = rect.y;
						textureAsset.width = rect.width;
						textureAsset.height = rect.height;
						textureAsset.name = images[index];

						textureAssets.Add(textureAsset);

						garbageRect.Add(rectangles[index]);
						garabeTextures.Add(textures[index]);
						garbageImages.Add(images[index]);
					}

					foreach (Rect garbage in garbageRect)
						rectangles.Remove(garbage);

					foreach (Texture2D garbage in garabeTextures)
						textures.Remove(garbage);

					foreach (string garbage in garbageImages)
						images.Remove(garbage);

					texture.Apply();

					if (savePath != "") {

						File.WriteAllBytes(savePath + "/data" + numSpriteSheet + ".png", texture.EncodeToPNG());
						File.WriteAllText(savePath + "/data" + numSpriteSheet + ".json", JsonUtility.ToJson(new TextureAssets(textureAssets.ToArray())));
						++numSpriteSheet;
					}

					foreach (TextureAsset textureAsset in textureAssets)
						mSprites.Add(textureAsset.name, Sprite.Create(texture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
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
				if (asset.Key.StartsWith(prefix))
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
