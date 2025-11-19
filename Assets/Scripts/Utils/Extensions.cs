using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Module.Data;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Utils
{
	public static class Extensions
	{
		#region Extentions for LINQ

		public static List<T> GetRandomElems<T>(this List<T> list, int num)
		{
			if (num > list.Count())
			{
				throw new EvaluateException("List member is not enough");
			}

			var disOrdered = list.OrderBy(e => Guid.NewGuid()).ToList();

			return disOrdered.GetRange(0, num);
		}

		public static T GetRandomOne<T>(this List<T> list) => list.GetRandomElems(1).First();

		public static T GetRandomByWeight<T>(this List<T> list, List<int> weightList) =>
			list[weightList.RandomIndexByWeight()];


		private static int RandomIndexByWeight(this List<int> list)
		{
			var totalWeight = list.Sum();
			var randomNum = Random.Range(0, totalWeight);
			for (int i = 0; i < list.Count; i++)
			{
				var threshold = list.GetRange(0, i + 1).Sum();
				if (randomNum < threshold)
				{
					return i;
				}
			}

			return -1;
		}

		public static List<T> DisOrderedList<T>(this List<T> list)
		{
			return list.OrderBy(a => Guid.NewGuid()).ToList();
		}

		public static List<T> GetExtendedList<T>(this List<T> list, params T[] values)
		{
			var newList = list.Where(e => true).ToList();
			values.ToList().ForEach(v => newList.Add(v));
			return newList;
		}

		#endregion
		public static Sprite ToSprite(this Texture2D tex)
		{
			if (tex == null) return null;
			return Sprite.Create(
				tex,
				new Rect(0, 0, tex.width, tex.height),
				new Vector2(0.5f, 0.5f)
			);
		}
		#region Random

		public static T RandomOne<T>(params T[] values)
		{
			return values[Random.Range(0, values.Length)];
		}

		public static T RandomOne<T>( List<T> list)
		{
			return list[Random.Range(0, list.Count)];
		}

		#endregion

		#region Transform

		public static void ClearChildren(this Transform transform)
		{
			List<Transform> clearList = new List<Transform>();
			foreach (Transform cTransform in transform) clearList.Add(cTransform);

			clearList.ForEach(t => GameObject.Destroy(t.gameObject));
		}

		public static void ClearChildrenImmediate(this Transform transform)
		{
			List<Transform> clearList = new List<Transform>();
			foreach (Transform cTransform in transform) clearList.Add(cTransform);

			clearList.ForEach(t => GameObject.DestroyImmediate(t.gameObject));
		}


		public static List<Transform> GetChildren(this Transform transform)
		{
			var children = new List<Transform>();
			foreach (Transform cTransform in transform)
			{
				children.Add(cTransform);
			}

			return children;
		}
		
		public static Vector2 ConvertUIPosition(RectTransform source, RectTransform target)
		{
			// 1. 源位置转屏幕坐标
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, source.position);

			// 2. 屏幕坐标转目标 RectTransform 的局部坐标
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPoint, Camera.main, out localPoint);
			return localPoint;
		}
		
		#endregion

		#region Vector 3

		public static Vector3 ReplaceX(this Vector3 vector3, float x)
		{
			return new Vector3(x, vector3.y, vector3.z);
		}

		public static Vector3 ReplaceY(this Vector3 vector3, float y)
		{
			return new Vector3(vector3.x, y, vector3.z);
		}

		public static Vector3 ReplaceZ(this Vector3 vector3, float z)
		{
			return new Vector3(vector3.x, vector3.y, z);
		}

		#endregion

		public static void TextLazyUpdate(this TextMeshProUGUI tmp, string content)
		{
			if (content != tmp.text) tmp.SetText(content);
		}

		public static IEnumerator ExecuteDelay(Action action, float delay)
		{
			yield return new WaitForSeconds(delay);
			action?.Invoke();
		}

		private static readonly string Format = "0." + new string('#', 339);

		public static string ToStringFix(this float num)
		{
			return num.ToString(Format, CultureInfo.InvariantCulture);
		}
		
		/// <summary>
		/// 获取当前日期属于当年的第几周
		/// </summary>
		public static int GetCurrentWeekNumber()
		{
			// 当前时间
			DateTime now = DateTime.Now;

			// 当前区域文化（决定每周起始日、周规则）
			CultureInfo cul = CultureInfo.CurrentCulture;

			// 计算周数：规则=FirstFourDayWeek, 周起始日=Monday
			int weekNum = cul.Calendar.GetWeekOfYear(
				now,
				CalendarWeekRule.FirstFourDayWeek,
				DayOfWeek.Monday);

			return weekNum;
		}
		
	   public static List<int> GetRandomNumbers(int min, int max, int count)
		{
			List<int> numbers = new List<int>();
			for (int i = min; i <= max; i++)
			{
				numbers.Add(i);
			}

			// 打乱顺序
			for (int i = numbers.Count - 1; i > 0; i--)
			{
				int j = UnityEngine.Random.Range(0, i + 1);
				(numbers[i], numbers[j]) = (numbers[j], numbers[i]);
			}

			// 取前 count 个
			return numbers.GetRange(0, count);
		}
		public  static bool IsAllChinese(string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;

			foreach (char c in input)
			{
				// 中文 Unicode 范围：\u4e00 - \u9fa5
				if (c < 0x4e00 || c > 0x9fa5)
					return false;
			}
			return true;
		}
		public static string FormatNumber(long num)
		{
			if (num >= 100000000) // 一亿
			{
				float value = num / 100000000f;
				return value.ToString("0.#") + "亿";
			}
			else if (num >= 10000) // 一万
			{
				float value = num / 10000f;
				return value.ToString("0.#") + "万";
			}
			else
			{
				return num.ToString();
			}
		}


		public static string GetMonsterResNameByType(MonsterType type)
		{
			string resName = "";
			switch (type)
			{
				case MonsterType.ShuangYunZhi: 
                     resName = "ShuangYunZhi";
                        break;
                    case MonsterType.ShuangYunZhiGolden:
					resName = "ShuangYunZhiGolden";
                        break;
                    case MonsterType.ShuangYunZhiBig:
					resName = "ShuangYunZhiBig";
                        break;
                    case MonsterType.YueLuCao:
					resName = "YueLuCao";
                        break;
                    case MonsterType.YueLuCaoGolden:
					resName = "YueLuCaoGolden";
                        break;
                    case MonsterType.YueLuCaoBig:
					resName = "YueLuCaoBig";
                        break;
                    case MonsterType.ZiXinHua:
					resName = "ZiXinHua";
                        break;
                    case MonsterType.ZiXinHuaGolden:
					resName = "ZiXinHuaGolden";
                        break;
                    case MonsterType.ZiXinHuaBig:
					resName = "ZiXinHuaBig";
                        break;
                    case MonsterType.YuHuiHe:
					resName = "YuHuiHe";
                        break;
                    case MonsterType.YuHuiHeGolden:
					resName = "YuHuiHeGolden";
                        break;
                    case MonsterType.YuHuiHeBig:
					resName = "YuHuiHeBig";
                        break;
                    case MonsterType.XingWenGuo:
					resName = "XingWenGuo";
                        break;
                    case MonsterType.XingWenGuoGolden:
					resName = "XingWenGuoGolden";
                        break;
                    case MonsterType.XingWenGuoBig:
					resName = "XingWenGuoBig";
                        break;
                    case MonsterType.WuRongJun:
					resName = "WuRongJun";
                        break;
                    case MonsterType.WuRongJunBig:
					resName = "WuRongJunBig";
                        break;
                    case MonsterType.WuRongJunGolden:
					resName = "WuRongJunGolden";
                        break;
                    case MonsterType.LingXuSheng:
					resName = "LingXuSheng";
                        break;
                    case MonsterType.LingXuShengGolden:
					resName = "LingXuShengGolden";
                        break;
                    case MonsterType.LingXuShengBig:
					resName = "LingXuShengBig";
                        break;
                    case MonsterType.XueBanHua:
					resName = "XueBanHua";
                        break;
                    case MonsterType.XueBanHuaGolden:
					resName = "XueBanHuaGolden";
                        break;
                    case MonsterType.XueBanHuaBig:
					resName = "XueBanHuaBig";
                        break;
                    case MonsterType.MuLingYa:
					resName = "MuLingYa";
                        break;
                    case MonsterType.MuLingYaGolden:
					resName = "MuLingYaGolden";
                        break;
                    case MonsterType.MuLingYaBig:
					resName = "MuLingYaBig";
                        break;
                    case MonsterType.JingRuiCao:
					resName = "JingRuiCao";
                        break;
                    case MonsterType.JingRuiCaoGolden:
					resName = "JingRuiCaoGolden";
                        break;
                    case MonsterType.JingRuiCaoBig:
					resName = "JingRuiCaoBig";
                        break;
                    case MonsterType.TieKuangShi:
					resName = "TieKuangShi";
                        break;
                    case MonsterType.TieKuangShiGolden:
					resName = "TieKuangShiGolden";
                        break;
                    case MonsterType.TieKuangShiBig:
					resName = "TieKuangShiBig";
                        break;
                    case MonsterType.YinKuangShi:
					resName = "YinKuangShi";
                        break;
                    case MonsterType.YinKuangShiGolden:
					resName = "YinKuangShiGolden";
                        break;
                    case MonsterType.YinKuangShiBig:
					resName = "YinKuangShiBig";
                        break;
                    case MonsterType.TongKuangShi:
					resName = "TongKuangShi";
                        break;
                    case MonsterType.TongKuangShiGolden:
					resName = "TongKuangShiGolden";
                        break;
                    case MonsterType.TongKuangShiBig:
					resName = "TongKuangShiBig";
                        break;
                    case MonsterType.ZiJingShi:
					resName = "ZiJingShi";
                        break;
                    case MonsterType.ZiJingShiGolden:
					resName = "ZiJingShiGolden";
                        break;
                    case MonsterType.ZiJingShiBig:
					resName = "ZiJingShiBig";
                        break;
                    case MonsterType.YueJingShi:
					resName = "YueJingShi";
                        break;
                    case MonsterType.YueJingShiGolden:
					resName = "YueJingShiGolden";
                        break;
                    case MonsterType.YueJingShiBig:
					resName = "YueJingShiBig";
                        break;
			}





			return resName;
		}
	}
}