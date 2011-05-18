/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace mOceanWindowsPhone
{
	internal class CustomButton
	{
		protected enum STATE
		{
			NULL,
			READY,
			PRESSED,
			HIDDEN
		}
		protected STATE state = STATE.NULL;

		protected Texture2D image = null;
		protected Texture2D imagePressed = null;
		protected Rectangle buttonRect = Rectangle.Empty;
		protected byte transparency = Byte.MaxValue;
		protected string text = null;
		protected Color textColor = Color.Transparent;
		protected SpriteFont textFont = null;
		protected SpriteBatch spriteBatch = null;

		public CustomButton(SpriteBatch spriteBatch)
		{
			this.spriteBatch = spriteBatch;
			state = STATE.HIDDEN;
		}

		public virtual void Update(GameTime gameTime)
		{
		}

		public virtual void Draw(GameTime gameTime)
		{
			if (spriteBatch != null)
			{
				spriteBatch.Begin();

				if (state == STATE.READY || state == STATE.PRESSED)
				{
					if (state == STATE.READY)
					{
						if (image != null)
						{
							spriteBatch.Draw(image, buttonRect, Color.FromNonPremultiplied(255, 255, 255, transparency));
						}
					}
					else if (state == STATE.PRESSED)
					{
						if (imagePressed != null)
						{
							spriteBatch.Draw(imagePressed, buttonRect, Color.FromNonPremultiplied(255, 255, 255, transparency));
						}
					}

					if (!String.IsNullOrEmpty(text) && textFont != null)
					{
						Vector2 textMeasure = textFont.MeasureString(text);
						Vector2 textPos = new Vector2((float)(buttonRect.X + (buttonRect.Width - textMeasure.X) / 2), (float)(buttonRect.Y + (buttonRect.Height - textMeasure.Y) / 2));

						textColor.A = transparency;
						spriteBatch.DrawString(textFont, text, textPos, textColor);
					}
				}

				spriteBatch.End();
			}
		}

		public bool InputProcess(TouchCollection touchCollection)
		{
			if (state == STATE.READY || state == STATE.PRESSED)
			{
				foreach (TouchLocation touchLocation in touchCollection)
				{
					if (touchLocation.State == TouchLocationState.Pressed && buttonRect.Contains((int)touchLocation.Position.X, (int)touchLocation.Position.Y))
					{
						state = STATE.PRESSED;
						OnClick();
						return true;
					}
					else
					{
						TouchLocation prevLocation;
						if (touchLocation.TryGetPreviousLocation(out prevLocation))
						{
							if (prevLocation.State == TouchLocationState.Pressed && buttonRect.Contains((int)prevLocation.Position.X, (int)prevLocation.Position.Y))
							{
								state = STATE.PRESSED;
								OnClick();
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public void Show()
		{
			state = STATE.READY;
		}

		public void Hide()
		{
			state = STATE.HIDDEN;
		}

		#region "Events"
		private event EventHandler ClickEvent;
		public event EventHandler Click
		{
			add { ClickEvent += value; }
			remove { ClickEvent -= value; }
		}
		#endregion

		#region "Properties"
		public Point Position
		{
			get { return new Point(buttonRect.X, buttonRect.Y); }
			set { buttonRect.X = value.X; buttonRect.Y = value.Y; }
		}
		public int Width
		{
			get { return buttonRect.Width; }
			set { buttonRect.Width = value; }
		}
		public int Height
		{
			get { return buttonRect.Height; }
			set { buttonRect.Height = value; }
		}
		public Texture2D Image
		{
			get { return image; }
			set { image = value; }
		}
		public Texture2D ImagePressed
		{
			get { return imagePressed; }
			set { imagePressed = value; }
		}
		public byte Transparency
		{
			get { return transparency; }
			set { transparency = value; }
		}
		public string Text
		{
			get { return text; }
			set { text = value; }
		}
		public Color TextColor
		{
			get { return textColor; }
			set { textColor = value; }
		}
		public SpriteFont TextFont
		{
			get { return textFont; }
			set { textFont = value; }
		}
		#endregion

		#region "Private methods"
		private void OnClick()
		{
			if (ClickEvent != null)
			{
				ClickEvent(this, EventArgs.Empty);
			}
		}
		#endregion
	}
}
