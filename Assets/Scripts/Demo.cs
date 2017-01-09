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

	private Texture2D mTexture;
	private Color32[] mFillColor;

	private RectanglePacker mPacker;

	private List<Rect> mRectangles = new List<Rect>();


	void Start () {

		mTexture = new Texture2D(WIDTH, HEIGHT, TextureFormat.ARGB32, false);

		mFillColor = mTexture.GetPixels32();

		for (int i = 0; i < mFillColor.Length; ++i)
			mFillColor[i] = Color.white;

		GameObject tmp = new GameObject("RectanglePackerDemo");
		SpriteRenderer spriteRenderer = tmp.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), Vector2.zero);

		createRectangles();

		updateRectangles();
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

            int numRejected = 0;

			mTexture.SetPixels32(mFillColor);
			IntegerRectangle rect = new IntegerRectangle();
			Color32[] tmpColor;

			for (int j = 0; j < mPacker.rectangleCount; j++) {

				rect = mPacker.getRectangle(j, rect);

                int size = rect.width*rect.height;

                if (rect.x < 0 || rect.y < 0 || rect.x+rect.width > mTexture.width || rect.y+rect.height > mTexture.height)
                {
                    numRejected++;
                    continue;
                }

				tmpColor = new Color32[size];
                for (int k = 0; k < size; ++k)
                    tmpColor[k] = Color.black;

				mTexture.SetPixels32(rect.x, rect.y, rect.width, rect.height, tmpColor);

                int index = mPacker.getRectangleId(j);
				Color color = convertHexToRGBA((uint) (0xFF171703 + (((18 * ((index + 4) % 13)) << 16) + ((31 * ((index * 3) % 8)) << 8) + 63 * (((index + 1) * 3) % 5))));

                size -= 4;

				tmpColor = new Color32[size];
				for (int k = 0; k < size; ++k)
					tmpColor[k] = color;

				mTexture.SetPixels32(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2, tmpColor);

            }

			mTexture.Apply();

            if(numRejected > 0)
                Debug.LogWarning("Rejected " + numRejected + " rectangles that were either out of bounds or intersecting borders.");
		}
	}

	private Color32 convertHexToRGBA(uint color) {

		Color32 c;
		c.b = (byte)((color) & 0xFF);
		c.g = (byte)((color>>8) & 0xFF);
		c.r = (byte)((color>>16) & 0xFF);
		c.a = (byte)((color>>24) & 0xFF);
		return c;
	}

}
