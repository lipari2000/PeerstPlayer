﻿using Shule.Peerst.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
namespace Shule.Peerst.BBS
{
	/// <summary>
	/// わいわいKakikoストラテジクラス
	/// </summary>
	class YYKakikoBbsStrategy : BbsStrategy
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="bbsUrl">掲示板アドレス情報</param>
		public YYKakikoBbsStrategy(BbsUrl bbsUrl)
		: base(bbsUrl)
		{
		}

		/// <summary>
		/// 掲示板書き込みリクエスト用データ作成
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mail"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		protected override byte[] CreateWriteRequestData(string name, string mail, string message)
		{
			Encoding encode = GetEncode();
			string param = ""; // リクエストデータ

			param += "bbs=" + HttpUtility.UrlEncode(bbsUrl.BoadNo, encode) + "&";	// 板番号
			param += "key=" + HttpUtility.UrlEncode(bbsUrl.ThreadNo, encode) + "&";	// スレ番号
			param += "FROM=" + HttpUtility.UrlEncode(name, encode) + "&";			// 名前
			param += "mail=" + HttpUtility.UrlEncode(mail, encode) + "&";			// メール
			param += "MESSAGE=" + HttpUtility.UrlEncode(message, encode) + "&";		// 本文
			param += "submit=" + HttpUtility.UrlEncode("書き込む", encode) + "&";	// 書き込む

			return Encoding.ASCII.GetBytes(param);
		}

		/// <summary>
		/// リクエストURLを取得
		/// </summary>
		protected override string GetRequestURL()
		{
			return "http://" + bbsUrl.BoadGenre + "/test/bbs.cgi";
		}

		/// <summary>
		/// スレッド読み込み
		/// </summary>
		/// <returns>スレッド情報一覧</returns>
		public override List<ThreadInfo> ReadThread(string threadNo)
		{
			List<ThreadInfo> threadData = new List<ThreadInfo>();

			// datの取得
			// http://yy67.60.kg/ff11peca/dat/1263298996.dat
			string url = "http://" + bbsUrl.BoadGenre + "/" + bbsUrl.BoadNo + "/dat/" + threadNo + ".dat";
			string html = HTTP.GetHtml(url, GetEncode());

			// TODO 指定レス(ResNum)まで飛ばす
			/*
			int find = 0;
			for (int i = 0; i < ResNum - 1; i++)
			{
				find = html.IndexOf('\n', find) + 1;
			}
			html = html.Substring(find);
			 */

			// 本文の修正
			// html = html.Replace("<a", @"<u><font color=""blue""");
			// html = html.Replace("</a>", "</font></u>");
			//html = html.Replace("<br>", "<br><DD>");
			//html = html.Replace("<br>", "<DD>");
			html = html.Replace("<a", "<tt");
			html = html.Replace("</a>", "</tt>");

			// リンクの書き換え
			// TODO リンクの書き換え html = LinkReplace(html);

			// 1レスごと区切る
			string[] ResList = html.Split('\n');

			// データごと区切る
			string[] split = new string[1];
			split[0] = "<>";
			for (int i = 0; i < ResList.Length; i++)
			{
				string[] ResData = ResList[i].Split(split, StringSplitOptions.None);
				if (ResData.Length == 5)
				{
					// 名前の太文字バグを除去
					ResData[0] = ResData[0].Replace("<b>", "").Replace("</b>", "");

					string id = "";
					string date = "";

					int index = ResData[2].LastIndexOf("ID:");
					if (index != -1)
					{
						id = ResData[2].Substring(index + 3, ResData[2].Length - (index + 3));
						date = ResData[2].Substring(0, index - 1);
					}
					else
					{
						date = ResData[2];
					}

					// データ作成
					ThreadInfo data = new ThreadInfo();
					data.Name = ResData[0]; // 名前
					data.Mail = ResData[1]; // メール
					data.Date = date; // 日付
					data.ID = id; // ID
					data.Text = ResData[3]; // 本文

					// レスデータを追加
					threadData.Add(data);
				}
			}

			return threadData;
		}

		/// <summary>
		/// 文字エンコードの取得
		/// </summary>
		protected override Encoding GetEncode()
		{
			return Encoding.GetEncoding("Shift_JIS");
		}

		/// <summary>
		/// 板URLを取得
		/// </summary>
		protected override string GetBoadUrl()
		{
			return "http://" + bbsUrl.BoadGenre + "/" + bbsUrl.BoadNo + "/";
		}
	}
}
