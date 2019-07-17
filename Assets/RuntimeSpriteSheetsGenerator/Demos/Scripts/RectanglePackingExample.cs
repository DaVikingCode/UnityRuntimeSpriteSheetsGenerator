/**
 * Rectangle Packer demo
 *
 * Copyright 2012 Ville Koskela. All rights reserved.
 * Ported to Unity by Da Viking Code.
 *
 * Email: ville@villekoskela.org
 * Blog: http://villekoskela.org
 * Twitter: @villekoskelaorg
 *
 * You may redistribute, use and/or modify this source code freely
 * but this copyright statement must not be removed from the source files.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. *
 *
 */
using DaVikingCode.RectanglePacking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectanglePackingExample : MonoBehaviour {

	private const int RECTANGLE_COUNT = 500;
	private const float SIZE_MULTIPLIER = 2;

	private Texture2D mTexture;
	private Color32[] mFillColor;

	private RectanglePacker mPacker;

	private List<Rect> mRectangles = new List<Rect>();

	public Image img;
	public Slider sliderWidth;
	public Slider sliderHeight;
	public Text packingTimeText;

	void Start () {

		mTexture = new Texture2D((int) sliderWidth.value, (int) sliderHeight.value, TextureFormat.ARGB32, false);

		mFillColor = mTexture.GetPixels32();

		for (int i = 0; i < mFillColor.Length; ++i)
			mFillColor[i] = Color.clear;

		img.sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), Vector2.zero);

		createRectangles();

		updateRectangles();

		sliderWidth.onValueChanged.AddListener(updatePackingBox);
		sliderHeight.onValueChanged.AddListener(updatePackingBox);
	}

	void updatePackingBox(float value) {

		updateRectangles();
	}

	private void createRectangles() {

		int width;
		int height;
		for (int i = 0; i < 10; i++) {
			
			width = (int) (20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
			height = (int) (20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
			mRectangles.Add(new Rect(0, 0, width, height));
		}

		for (int j = 10; j < RECTANGLE_COUNT; j++) {

			width = (int) (3 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER);
			height = (int) (3 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER);
			mRectangles.Add(new Rect(0, 0, width, height));
		}
	}

	private void updateRectangles() {

		DateTime start = DateTime.Now;
		const int padding = 1;

		if (mPacker == null)
			mPacker = new RectanglePacker((int) sliderWidth.value, (int)sliderHeight.value, padding);
		else
			mPacker.reset((int) sliderWidth.value, (int) sliderHeight.value, padding);

		for (int i = 0; i < RECTANGLE_COUNT; i++)
			mPacker.insertRectangle((int) mRectangles[i].width, (int) mRectangles[i].height, i);

		mPacker.packRectangles();

		DateTime end = DateTime.Now;

		if (mPacker.rectangleCount > 0) {
			
			packingTimeText.text = mPacker.rectangleCount + " rectangles packed in " + (end - start).Milliseconds + "ms";

			mTexture.SetPixels32(mFillColor);
			IntegerRectangle rect = new IntegerRectangle();
			Color32[] tmpColor;

			for (int j = 0; j < mPacker.rectangleCount; j++) {

				rect = mPacker.getRectangle(j, rect);

                int size = rect.width * rect.height;
				
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
		}
	}

	private Color32 convertHexToRGBA(uint color) {

        return new Color32(
            (byte)((color >> 16) & 0xFF),
            (byte)((color >> 8) & 0xFF),
            (byte)((color) & 0xFF),
            (byte)((color >> 24) & 0xFF)
            );
	}

}
