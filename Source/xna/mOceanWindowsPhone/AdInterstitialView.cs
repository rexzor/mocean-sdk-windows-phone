/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace mOceanWindowsPhone
{
	public class AdInterstitialView : AdView
	{
		public enum ButtonPosition
		{
			Top = 0,
			Bottom = 1,
			Left = 2,
			Right = 3,
			Center = 4
		};

		private const int DEFAULT_CLOSE_BUTTON_WIDTH = 173;
		private const int DEFAULT_CLOSE_BUTTON_HEIGHT = 173;

		#region "Variables"
		private ButtonPosition closeButtonPosition = ButtonPosition.Left;
		private uint showCloseButtonTime = 0;
		private uint autoCloseInterstitialTime = System.UInt32.MaxValue;
		private Color closeButtonBackgroundColor = Color.FromNonPremultiplied(0, 0, 0, 0);
		private Texture2D closeButtonImage = null;
		private Texture2D closeButtonSelectedImage = null;
		private bool isShowPhoneStatusBar = false;
		private System.Threading.Timer closeBtnTimer = null;
		private System.Threading.Timer adTimer = null;

		private CustomButton closeButton = null;
		#endregion

		#region "Public methods"
		public AdInterstitialView(int site, int zone, Game game) : base(site, zone, game)
		{
			closeButton = new CustomButton(spriteBatch);
			closeButton.Width = DEFAULT_CLOSE_BUTTON_WIDTH;
			closeButton.Height = DEFAULT_CLOSE_BUTTON_HEIGHT;

			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeBtnTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseBtnTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
		}

		public override void Update(GameTime gameTime)
		{
			if (adState == AD_STATE.NULL)
			{
				Thread initCloseBtnThread = new Thread(new ThreadStart(InitCloseButton));
				initCloseBtnThread.Start();
			}

			bool closeButtonPressed = false;
			if (adState == AD_STATE.READY || adState == AD_STATE.SHOW || adState == AD_STATE.UPDATE)
			{
				touchCollection = Microsoft.Xna.Framework.Input.Touch.TouchPanel.GetState();
				closeButton.Update(gameTime);
				closeButtonPressed = closeButton.InputProcess(touchCollection);
				if (closeButtonPressed)
				{
					adState = AD_STATE.CLOSED;
				}
			}

			if (!closeButtonPressed)
			{
				UpdateAd();
				UpdateAdCampaignManager(gameTime);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (game != null)
			{
				adRect.X = (game.GraphicsDevice.Viewport.Width - adRect.Width) / 2;
				adRect.Y = (game.GraphicsDevice.Viewport.Height - adRect.Height) / 2;
			}

			base.Draw(gameTime);

			switch (adState)
			{
				case AD_STATE.SHOW:
				case AD_STATE.UPDATE:
				case AD_STATE.PRESSED:
					InitCloseButtonPosition();
					closeButton.Draw(gameTime);
					break;
				default:
					break;
			}
		}

		public void Close()
		{
			adState = AD_STATE.CLOSED;
			closeButton.Hide();
			adTimer.Dispose();
			closeBtnTimer.Dispose();
		}
		#endregion

		#region "Properties"
		public ButtonPosition CloseButtonPosition
		{
			get { return closeButtonPosition; }
			set { closeButtonPosition = value; }
		}
		public uint ShowCloseButtonTime
		{
			get { return showCloseButtonTime / 1000; }
			set { showCloseButtonTime = value * 1000; }
		}
		public uint AutoCloseInterstitialTime
		{
			get { return autoCloseInterstitialTime / 1000; }
			set
			{
				if (value == 0)
				{
					autoCloseInterstitialTime = System.UInt32.MaxValue;
				}
				else
				{
					autoCloseInterstitialTime = value * 1000;
				}
			}
		}
		public Color CloseButtonTextColor
		{
			get { return closeButton.TextColor; }
			set { closeButton.TextColor = value; }
		}
		public Color CloseButtonBackgroundColor
		{
			get { return closeButtonBackgroundColor; }
			set { closeButtonBackgroundColor = value; }
		}
		public bool IsShowPhoneStatusBar
		{
			get { return isShowPhoneStatusBar; }
			set { isShowPhoneStatusBar = value; }
		}
		public Texture2D CloseButtonImage
		{
			get { return closeButtonImage; }
			set { closeButtonImage = value; }
		}
		public Texture2D CloseButtonSelectedImage
		{
			get { return closeButtonSelectedImage; }
			set { closeButtonSelectedImage = value; }
		}
		public string CloseButtonText
		{
			get { return closeButton.Text; }
			set { closeButton.Text = value; }
		}
		public byte CloseButtonTransparency
		{
			get { return closeButton.Transparency; }
			set { closeButton.Transparency = value; }
		}
		public int CloseButtonWidth
		{
			get { return closeButton.Width; }
			set { closeButton.Width = value; }
		}
		public int CloseButtonHeight
		{
			get { return closeButton.Height; }
			set { closeButton.Height = value; }
		}

// 		public SpriteFont CloseButtonFont
// 		{
// 			get { return closeButton.TextFont; }
// 			set { closeButton.TextFont = value; }
// 		}
		#endregion

		#region "Private methods"
		private void InitShowTimers()
		{
			adTimer.Change(autoCloseInterstitialTime, System.UInt32.MaxValue);
			closeBtnTimer.Change(showCloseButtonTime, System.UInt32.MaxValue);
		}

		private void InitCloseButton()
		{
			InitShowTimers();

			if (closeButtonImage != null)
			{
				closeButton.Image = closeButtonImage;
			}
			else
			{
				closeButton.Image = GetColorTexture(closeButtonBackgroundColor);
			}

			if (closeButtonSelectedImage != null)
			{
				closeButton.ImagePressed = closeButtonSelectedImage;
			}
			else
			{
				closeButton.ImagePressed = GetColorTexture(closeButtonBackgroundColor);
			}

			closeButton.Click += new EventHandler(CloseButton_Click);
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private Texture2D GetColorTexture(Color color)
		{
			Texture2D resultTex = null;

			Color[] colorData = new Color[1];
			colorData[0] = color;

			resultTex = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			resultTex.SetData(colorData);

			return resultTex;
		}

		private void ProcessAdTimerEvent(object sender)
		{
			Close();
		}

		private void ProcessCloseBtnTimerEvent(object sender)
		{
			ShowCloseButton();
		}

		private void ShowCloseButton()
		{
			closeBtnTimer.Dispose();
			closeButton.Show();
		}

		private void InitCloseButtonPosition()
		{
			int winWidth = game.GraphicsDevice.Viewport.Width;
			int winHeight = game.GraphicsDevice.Viewport.Height;

			switch (closeButtonPosition)
			{
				case ButtonPosition.Top:
					closeButton.Position = new Point((winWidth - closeButton.Width) / 2, 0);
					break;
				case ButtonPosition.Bottom:
					closeButton.Position = new Point((winWidth - closeButton.Width) / 2, winHeight - closeButton.Height);
					break;
				case ButtonPosition.Right:
					closeButton.Position = new Point(winWidth - closeButton.Width, (winHeight - closeButton.Height) / 2);
					break;
				case ButtonPosition.Left:
					closeButton.Position = new Point(0, (winHeight - closeButton.Height) / 2);
					break;
				case ButtonPosition.Center:
					closeButton.Position = new Point((winWidth - closeButton.Width) / 2, (winHeight - closeButton.Height) / 2);
					break;
				default:
					closeButton.Position = new Point(winWidth - closeButton.Width, 0);
					break;
			}
		}
		#endregion
	}
}
