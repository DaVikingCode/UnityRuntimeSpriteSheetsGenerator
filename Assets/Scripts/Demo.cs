using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

	private const int WIDTH = 480;
	private const int HEIGHT = 480;
	private const int Y_MARGIN = 40;
	private const int BOX_MARGIN = 15;

	private const int RECTANGLE_COUNT = 500;
	private const float SIZE_MULTIPLIER = 2;

	private RectanglePacker mPacker;

	private List<Rect> mRectangles = new List<Rect>();


	void Start () {

		Texture2D texture = new Texture2D(WIDTH, HEIGHT, TextureFormat.ARGB32, false);

		Color[] fillColor = texture.GetPixels();
		for (int i = 0; i < fillColor.Length; ++i)
			fillColor[i] = Color.red;

		texture.SetPixels(fillColor);

		texture.Apply();

		GameObject tmp = new GameObject();
		SpriteRenderer spriteRenderer = tmp.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
	}

	void Update() {

	}

	private void createRectangles() {

		int width;
		int height;
		for (int i = 0; i < 10; i++) {
			
			width = (int) (20 * SIZE_MULTIPLIER + Mathf.Floor(Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
			height = (int) (20 * SIZE_MULTIPLIER + Mathf.Floor(Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
			mRectangles.Add(new Rect(0, 0, width, height));
		}

		for (int j = 10; j < RECTANGLE_COUNT; j++) {

			width = (int) (3 * SIZE_MULTIPLIER + Mathf.Floor(Random.value * 8) * SIZE_MULTIPLIER);
			height = (int) (3 * SIZE_MULTIPLIER + Mathf.Floor(Random.value * 8) * SIZE_MULTIPLIER);
			mRectangles.Add(new Rect(0, 0, width, height));
		}
	}

	private void updateRectangles() {

		const int padding = 1;

		if (mPacker == null)
			mPacker = new RectanglePacker(WIDTH, HEIGHT, padding);

		else
			mPacker.reset(WIDTH, HEIGHT, padding);

		for (int i = 0; i < RECTANGLE_COUNT; i++)
			mPacker.insertRectangle((int) mRectangles[i].width, (int) mRectangles[i].height, i);

		mPacker.packRectangles();

		if (mPacker.rectangleCount > 0) {

			//mBitmapData.fillRect(mBitmapData.rect, 0xFFFFFFFF);

			/*Rect rect = new Rect();
			for (int j = 0; j < mPacker.rectangleCount; ++j) {

				rect = mPacker.getRectangle(j, rect);

			}*/
		}
	}

}
