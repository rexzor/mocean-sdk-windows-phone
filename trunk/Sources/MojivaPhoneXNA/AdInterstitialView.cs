/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using Microsoft.Xna.Framework;
using System.Windows;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Input.Touch;

namespace MojivaPhone
{
	public class AdInterstitialView : AdView
	{
		public enum AdInterstitialCloseButtonPosition
		{
			AdInterstitialCloseButtonPositionCenter = 0,
			AdInterstitialCloseButtonPositionTop = 1,
			AdInterstitialCloseButtonPositionBottom = 2,
			AdInterstitialCloseButtonPositionLeft = 4,
			AdInterstitialCloseButtonPositionRight = 8,
		};

		private const int DEFAULT_CLOSE_BUTTON_WIDTH = 173;
		private const int DEFAULT_CLOSE_BUTTON_HEIGHT = 173;
		private const string DEFAULT_CLOSE_BUTTON_CONTENT = "";

		#region "Variables"
		private AdInterstitialCloseButtonPosition closeButtonPosition = AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight;
		private Microsoft.Xna.Framework.Point closeButtonPositionPoint = Microsoft.Xna.Framework.Point.Zero;
		private uint showCloseButtonTime = 0;
		private uint autoCloseInterstitialTime = System.UInt32.MaxValue;
		private Color closeButtonTextColor = Color.FromNonPremultiplied(0, 0, 0, 0);
		private Color closeButtonBackgroundColor = Color.FromNonPremultiplied(0, 0, 0, 0);
		private string closeButtonImage = null;
		private string closeButtonText = DEFAULT_CLOSE_BUTTON_CONTENT;
		private int closeButtonTransparency = 255;
		private Size closeButtonSize = new Size((double)DEFAULT_CLOSE_BUTTON_WIDTH, (double)DEFAULT_CLOSE_BUTTON_HEIGHT);
		private bool isShowPhoneStatusBar = false;
		private System.Threading.Timer closeBtnTimer = null;
		private System.Threading.Timer adTimer = null;
		private Texture2D closeBtnImgTexture = null;
		private bool needShowCloseBtn = false;
		private SpriteFont closeBtnFont = null;
		#endregion

		#region "Properties"
		public AdInterstitialCloseButtonPosition CloseButtonPosition
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
				autoCloseInterstitialTime = value * 1000;
				if (autoCloseInterstitialTime == 0)
				{
					autoCloseInterstitialTime = System.UInt32.MaxValue;
				}
			}
		}
		public Color CloseButtonTextColor
		{
			get { return closeButtonTextColor; }
			set { closeButtonTextColor = value; }
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
		public string CloseButtonImage
		{
			set
			{
				closeButtonImage = value;
			}
		}
		public string CloseButtonSelectedImage
		{
			set { }
		}
		public string CloseButtonText
		{
			get { return closeButtonText; }
			set { closeButtonText = value; }
		}
		public int CloseButtonTransparency
		{
			get { return closeButtonTransparency; }
			set { closeButtonTransparency = value; }
		}
		public Size CloseButtonSize
		{
			get
			{
				return closeButtonSize;
			}
			set
			{
				closeButtonSize = value;
			}
		}
		public SpriteFont CloseBtnFont
		{
			get { return closeBtnFont; }
			set { closeBtnFont = value; }
		}

		#endregion

		#region "Public methods"
		public AdInterstitialView(int site, int zone) : base(site, zone)
		{
			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeBtnTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseBtnTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
		}
		public AdInterstitialView(int site, int zone, Game game) : base(site, zone, game)
		{
			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeBtnTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseBtnTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
		}

		public override void Run()
		{
			base.Run();

			Thread initCloseBtnThread = new Thread(new ThreadStart(InitAdProperties));
			initCloseBtnThread.Start();

			InitShowTimers();
		}

		public override void Update(GameTime gameTime)
		{
			if (needShowCloseBtn)
			{
				InputProcess();
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			spriteBatch.Begin();
			if (needShowCloseBtn)
			{
				Vector2 pos = new Vector2(closeButtonPositionPoint.X, closeButtonPositionPoint.Y);

				if (closeBtnImgTexture != null)
				{
					spriteBatch.Draw(closeBtnImgTexture, new Rectangle(closeButtonPositionPoint.X, closeButtonPositionPoint.Y, (int)closeButtonSize.Width, (int)closeButtonSize.Height), Color.FromNonPremultiplied(255, 255, 255, closeButtonTransparency));
				}

				if (!String.IsNullOrEmpty(closeButtonText) && closeBtnFont != null)
				{
					Vector2 textMeasure = closeBtnFont.MeasureString(closeButtonText);
					Vector2 textPos = new Vector2((float)(closeButtonPositionPoint.X + (closeButtonSize.Width - textMeasure.X) / 2), (float)(closeButtonPositionPoint.Y + (closeButtonSize.Height - textMeasure.Y) / 2));

					closeButtonTextColor.A = (byte)closeButtonTransparency;
					spriteBatch.DrawString(closeBtnFont, closeButtonText, textPos, closeButtonTextColor);
				}
			}
			spriteBatch.End();
		}

		public void Close()
		{
			adState = AD_STATE.NULL;
			needShowCloseBtn = false;
			adTimer.Dispose();
			closeBtnTimer.Dispose();
		}
		#endregion

		#region "Private methods"
		private void InitAdProperties()
		{
			InitCloseButton();
		}

		private void InitShowTimers()
		{
			adTimer.Change(autoCloseInterstitialTime, System.UInt32.MaxValue);
			closeBtnTimer.Change(showCloseButtonTime, System.UInt32.MaxValue);
		}

		private void InitCloseButton()
		{
			TrySetCloseBtnImage();
 			InitCloseBtnPosition();
		}

		private void TrySetCloseBtnImage()
		{
			if (!String.IsNullOrEmpty(closeButtonImage))
			{
				try
				{
					byte[] imgBytes = DataRequest.ReadByteData(closeButtonImage);
					MemoryStream imgMemStream = new MemoryStream();
					imgMemStream.Write(imgBytes, 0, imgBytes.Length);
					closeBtnImgTexture = Texture2D.FromStream(spriteBatch.GraphicsDevice, imgMemStream);
				}
				catch (System.Exception /*ex*/)
				{}
			}
			else
			{
				Color[] color = new Color[1];
				color[0] = closeButtonBackgroundColor;

				closeBtnImgTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
				closeBtnImgTexture.SetData(color);
			}
		}

		private void ProcessAdTimerEvent(object sender)
		{
			Close();
		}

		private void ProcessCloseBtnTimerEvent(object sender)
		{
			ShowCloseBtn();
		}

		private void ShowCloseBtn()
		{
			closeBtnTimer.Dispose();
			needShowCloseBtn = true;
		}

		private void InitCloseBtnPosition()
		{
			int winWidth = game.GraphicsDevice.Viewport.Width;
			int winHeight = game.GraphicsDevice.Viewport.Height;

			switch (closeButtonPosition)
			{
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeButtonPositionPoint.X = 0;
					closeButtonPositionPoint.Y = 0;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeButtonPositionPoint.X = winWidth - (int)closeButtonSize.Width;
					closeButtonPositionPoint.Y = 0;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeButtonPositionPoint.X = 0;
					closeButtonPositionPoint.Y = winHeight - (int)closeButtonSize.Height;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeButtonPositionPoint.X = winWidth - (int)closeButtonSize.Width;
					closeButtonPositionPoint.Y = winHeight - (int)closeButtonSize.Height;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop:
					closeButtonPositionPoint.X = (winWidth - (int)closeButtonSize.Width) / 2;
					closeButtonPositionPoint.Y = 0;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom:
					closeButtonPositionPoint.X = (winWidth - (int)closeButtonSize.Width) / 2;
					closeButtonPositionPoint.Y = winHeight - (int)closeButtonSize.Height;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeButtonPositionPoint.X = winWidth - (int)closeButtonSize.Width;
					closeButtonPositionPoint.Y = (winHeight - (int)closeButtonSize.Height) / 2;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeButtonPositionPoint.X = 0;
					closeButtonPositionPoint.Y = (winHeight - (int)closeButtonSize.Height) / 2;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionCenter:
					closeButtonPositionPoint.X = (winWidth - (int)closeButtonSize.Width) / 2;
					closeButtonPositionPoint.Y = (winHeight - (int)closeButtonSize.Height) / 2;
					break;
				default:
					closeButtonPositionPoint.X = winWidth - (int)closeButtonSize.Width;
					closeButtonPositionPoint.Y = 0;
					break;
			}
		}

		private void InputProcess()
		{
			TouchCollection touchCollection = TouchPanel.GetState();
			foreach (TouchLocation touchLocation in touchCollection)
			{
				if (touchLocation.State != TouchLocationState.Invalid)
				{
					if (touchLocation.Position.X >= closeButtonPositionPoint.X && touchLocation.Position.X <= closeButtonPositionPoint.X + closeButtonSize.Width &&
						touchLocation.Position.Y >= closeButtonPositionPoint.Y && touchLocation.Position.Y <= closeButtonPositionPoint.Y + closeButtonSize.Height)
					{
						Close();
						return;
					}
				}
			}
		}
		#endregion
	}
}
