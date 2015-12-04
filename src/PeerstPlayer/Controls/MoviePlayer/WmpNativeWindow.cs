﻿using System;
using System.Windows.Forms;
using PeerstLib.Controls;
using AxWMPLib;
using PeerstPlayer.Forms.Player;
using PeerstLib.Forms;

namespace PeerstPlayer.Controls.MoviePlayer
{
	//-------------------------------------------------------------
	// 概要：WMPウィンドウのサブクラス化
	//-------------------------------------------------------------
	class WmpNativeWindow : NativeWindow
	{
		//-------------------------------------------------------------
		// 公開プロパティ
		//-------------------------------------------------------------

		// WMP
		AxWindowsMediaPlayer wmp;

		// ダブルクリックイベント
		public event AxWMPLib._WMPOCXEvents_DoubleClickEventHandler DoubleClick = delegate { };

		// ウィンドウサイズ変更用の枠サイズ
		private const int frameSize = 15;

		private IntPtr videoHandle;
		private VideoNativeWindow native;

		//-------------------------------------------------------------
		// 概要：コンストラクタ
		//-------------------------------------------------------------
		public WmpNativeWindow(AxWindowsMediaPlayer wmp)
		{
			this.wmp = wmp;

			// サブクラスウィンドウの設定
			AssignHandle(wmp.Handle);

			wmp.PlayStateChange += (sender, @event) =>
			{
				var h = Win32API.FindWindowEx(Handle, IntPtr.Zero, null, null);
				if (h != IntPtr.Zero && videoHandle != h)
				{
					native = new VideoNativeWindow(h);
					native.DoubleClick += (o, e) => DoubleClick(this, new _WMPOCXEvents_DoubleClickEvent((short)Keys.LButton, 0, e.fX, e.fY));
					videoHandle = h;
				}
			};

			// 枠なし時のサイズ変更処理
			wmp.MouseMoveEvent += (sender, e) =>
			{
				// 枠なしのときだけ処理を実行する
				if (!PlayerSettings.FrameInvisible)
				{
					return;
				}

				HitArea area = FormUtility.GetHitArea(frameSize, e.fX, e.fY, wmp.Width, wmp.Height);
				if (area == HitArea.HTNONE)
				{
					return;
				}

				switch (area)
				{
					case HitArea.HTTOP:
					case HitArea.HTBOTTOM:
						Cursor.Current = Cursors.SizeNS;
						break;
					case HitArea.HTLEFT:
					case HitArea.HTRIGHT:
						Cursor.Current = Cursors.SizeWE;
						break;
					case HitArea.HTTOPLEFT:
					case HitArea.HTBOTTOMRIGHT:
						Cursor.Current = Cursors.SizeNWSE;
						break;
					case HitArea.HTTOPRIGHT:
					case HitArea.HTBOTTOMLEFT:
						Cursor.Current = Cursors.SizeNESW;
						break;
				}
			};

			// 枠なし時のサイズ変更処理
			wmp.MouseDownEvent += (sender, e) =>
			{
				// 枠なしのときだけ処理を実行する
				if (!PlayerSettings.FrameInvisible)
				{
					return;
				}

				HitArea area = FormUtility.GetHitArea(frameSize, e.fX, e.fY, wmp.Width, wmp.Height);
				if (area != HitArea.HTNONE)
				{
					Win32API.SendMessage(wmp.Parent.Parent.Handle, (int)WindowsMessage.WM_NCLBUTTONDOWN, new IntPtr((int)area), new IntPtr(0));
				}
			};
		}


		//-------------------------------------------------------------
		// 概要：ウィンドウプロシージャ
		// 詳細：ダブルクリック押下のイベント通知
		//-------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			switch ((WindowsMessage)m.Msg)
			{
				case WindowsMessage.WM_LBUTTONDBLCLK:
					DoubleClick(this, new AxWMPLib._WMPOCXEvents_DoubleClickEvent((short)Keys.LButton, 0, (int)m.LParam & 0xFFFF, (int)m.LParam >> 16));
					break;

				case WindowsMessage.WM_MOUSEMOVE:
					// マウスカーソルの更新
					Cursor.Current = Cursors.Arrow;
					break;

				default:
					break;
			}

			base.WndProc(ref m);
		}
	}

	class VideoNativeWindow : NativeWindow
	{
		public event _WMPOCXEvents_DoubleClickEventHandler DoubleClick = delegate { };

		public VideoNativeWindow(IntPtr handle)
		{
			AssignHandle(handle);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)WindowsMessage.WM_LBUTTONDBLCLK)
			{
				DoubleClick(this, new _WMPOCXEvents_DoubleClickEvent((short)Keys.LButton, 0, (int)m.LParam & 0xFFFF, (int)m.LParam >> 16));
				return;
			}

			base.WndProc(ref m);
		}
	}
}
