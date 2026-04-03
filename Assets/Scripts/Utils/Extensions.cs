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

		public static T RandomOne<T>(List<T> list)
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
		public static bool IsAllChinese(string input)
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
		public static string FormatNumber(long num, int decimalPlaces = 2, bool fixedDecimal = false)
		{
			string format = fixedDecimal
				? "0." + new string('0', decimalPlaces)
				: "0." + new string('#', decimalPlaces);

			if (num >= 100000000)
			{
				decimal value = num / 100000000m;
				return value.ToString(format) + "亿";
			}
			else if (num >= 10000)
			{
				decimal value = num / 10000m;
				return value.ToString(format) + "万";
			}
			else
			{
				return num.ToString();
			}
		}

		public static string GetGoodsResNameByType(GoodsType type)
		{
			string resName = string.Empty;
			switch (type)
			{
				case GoodsType.YunZhiCha:
					resName = "YunZhiCha";
					break;
				case GoodsType.YueLuCha:
					resName = "YueLuCha";
					break;
				case GoodsType.ZiXinCha:
					resName = "ZiXinCha";
					break;
				case GoodsType.YuHuiCha:
					resName = "YuHeCha";
					break;
				case GoodsType.XingWenCha:
					resName = "XingWenCha";
					break;
				case GoodsType.WuRongCha:
					resName = "WuRongCha";
					break;
				case GoodsType.LingXuCha:
					resName = "LingXuCha";
					break;
				case GoodsType.XueBanCha:
					resName = "XueBanCha";
					break;
				case GoodsType.MuLingCha:
					resName = "MuLingCha";
					break;
				case GoodsType.JingRuiCha:
					resName = "JingRuiCha";
					break;
				case GoodsType.QingYanJian:
					resName = "QingYanJian";
					break;
				case GoodsType.YinSiDao:
					resName = "YinSiDao";
					break;
				case GoodsType.TongWenDao:
					resName = "TongWenDao";
					break;
				case GoodsType.ZiWuJian:
					resName = "ZiWuJian";
					break;
				case GoodsType.YueXinJing:
					resName = "YueXinJing";
					break;
				case GoodsType.JingYunBao:
					resName = "JingYuanBao";
					break;
				case GoodsType.TongBi:
					resName = "TongBi";
					break;
			}
			return resName;
		}

		public static string GetDropItemResNameByType(DropItemType type)
		{
			string resName = string.Empty;
			switch (type)
			{
				case DropItemType.ShuangYunZhiFragment:
					resName = "ShuangYunZhi";
					break;
				case DropItemType.YueLuCaoFragment:
					resName = "YueLuCao";
					break;
				case DropItemType.ZiXinHuaFragment:
					resName = "ZiXinHua";
					break;
				case DropItemType.YuHuiHeFragment:
					resName = "YuHuiHe";
					break;
				case DropItemType.XingWenGuoFragment:
					resName = "XingWenGuo";
					break;
				case DropItemType.WuRongJunFragment:
					resName = "WuRongJun";
					break;
				case DropItemType.LingXuShengFragment:
					resName = "LingXuSheng";
					break;
				case DropItemType.XueBanHuaFragment:
					resName = "XueBanHua";
					break;
				case DropItemType.MuLingYaFragment:
					resName = "MuLingYa";
					break;
				case DropItemType.JingRuiCaoFragment:
					resName = "JingRuiCao";
					break;
				case DropItemType.TieKuangShiFragment:
					resName = "TieKuangShi";
					break;
				case DropItemType.YinKuangShiFragment:
					resName = "YinKuangShi";
					break;
				case DropItemType.TongKuangShiFragment:
					resName = "TongKuangShi";
					break;
				case DropItemType.ZiJingShiFragment:
					resName = "ZiJingShi";
					break;
				case DropItemType.YueJingShiFragment:
					resName = "YueJingShi";
					break;
				case DropItemType.JingYuanBao:
					resName = "JingYuanBao";
					break;
				case DropItemType.YingQian:
					resName = "TongBi";
					break;
			}
			return resName;
		}

		public static string GetMonsterPictureNameByType(MonsterFamily type)
		{
			string resName = string.Empty;
			switch (type)
			{
				case MonsterFamily.ShuangYunZhi:
					resName = "ShuangYunZhi";
					break;
				case MonsterFamily.YueLuCao:
					resName = "YueLuCao";
					break;

				case MonsterFamily.ZiXinHua:
					resName = "ZiXinHua";
					break;

				case MonsterFamily.YuHuiHe:
					resName = "YuHuiHe";
					break;

				case MonsterFamily.XingWenGuo:
					resName = "XingWenGuo";
					break;

				case MonsterFamily.WuRongJun:
					resName = "WuRongJun";
					break;

				case MonsterFamily.LingXuSheng:
					resName = "LingXuSheng";
					break;

				case MonsterFamily.XueBanHua:
					resName = "XueBanHua";
					break;

				case MonsterFamily.MuLingYa:
					resName = "MuLingYa";
					break;

				case MonsterFamily.JingRuiCao:
					resName = "JingRuiCao";
					break;

				case MonsterFamily.TieKuangShi:
					resName = "TieKuangShi";
					break;

				case MonsterFamily.YinKuangShi:
					resName = "YinKuangShi";
					break;

				case MonsterFamily.TongKuangShi:
					resName = "TongKuangShi";
					break;

				case MonsterFamily.ZiJingShi:
					resName = "ZiJingShi";
					break;

				case MonsterFamily.YueJingShi:
					resName = "YueJingShi";
					break;

			}
			return resName;
		}

		public static DropItemType ExchangeType(MonsterFamily monsterType)
		{
			DropItemType resultype = DropItemType.None;
			switch (monsterType)
			{
				case MonsterFamily.ShuangYunZhi:
					resultype = DropItemType.ShuangYunZhiFragment;
					break;
				case MonsterFamily.YueLuCao:
					resultype = DropItemType.YueLuCaoFragment;
					break;
				case MonsterFamily.ZiXinHua:
					resultype = DropItemType.ZiXinHuaFragment;
					break;

				case MonsterFamily.YuHuiHe:
					resultype = DropItemType.YuHuiHeFragment;
					break;
				case MonsterFamily.XingWenGuo:
					resultype = DropItemType.XingWenGuoFragment;
					break;
				case MonsterFamily.WuRongJun:
					resultype = DropItemType.WuRongJunFragment;
					break;
				case MonsterFamily.LingXuSheng:
					resultype = DropItemType.LingXuShengFragment;
					break;
				case MonsterFamily.XueBanHua:
					resultype = DropItemType.XueBanHuaFragment;
					break;
				case MonsterFamily.MuLingYa:
					resultype = DropItemType.MuLingYaFragment;
					break;
				case MonsterFamily.JingRuiCao:
					resultype = DropItemType.JingRuiCaoFragment;
					break;
				case MonsterFamily.TieKuangShi:
					resultype = DropItemType.TieKuangShiFragment;
					break;
				case MonsterFamily.YinKuangShi:
					resultype = DropItemType.YinKuangShiFragment;
					break;
				case MonsterFamily.TongKuangShi:
					resultype = DropItemType.TongKuangShiFragment;
					break;
				case MonsterFamily.ZiJingShi:
					resultype = DropItemType.ZiJingShiFragment;
					break;
				case MonsterFamily.YueJingShi:
					resultype = DropItemType.YueJingShiFragment;
					break;
			}
			return resultype;
		}

		public static GoodsType GetGoodsTypeByMonsterType(MonsterType type)
		{
			switch (type)
			{
				case MonsterType.ShuangYunZhi:
					return GoodsType.YunZhiCha;
				case MonsterType.ShuangYunZhiGolden:
					return GoodsType.YunZhiCha;
				case MonsterType.ShuangYunZhiBig:
					return GoodsType.YunZhiCha;
				case MonsterType.YueLuCao:
					return GoodsType.YueLuCha;
				case MonsterType.YueLuCaoGolden:
					return GoodsType.YueLuCha;
				case MonsterType.YueLuCaoBig:
					return GoodsType.YueLuCha;
				case MonsterType.ZiXinHua:
					return GoodsType.ZiXinCha;
				case MonsterType.ZiXinHuaGolden:
					return GoodsType.ZiXinCha;
				case MonsterType.ZiXinHuaBig:
					return GoodsType.ZiXinCha;
				case MonsterType.YuHuiHe:
					return GoodsType.YuHuiCha;
				case MonsterType.YuHuiHeGolden:
					return GoodsType.YuHuiCha;
				case MonsterType.YuHuiHeBig:
					return GoodsType.YuHuiCha;
				case MonsterType.XingWenGuo:
					return GoodsType.XingWenCha;
				case MonsterType.XingWenGuoGolden:
					return GoodsType.XingWenCha;
				case MonsterType.XingWenGuoBig:
					return GoodsType.XingWenCha;
				case MonsterType.WuRongJun:
					return GoodsType.WuRongCha;
				case MonsterType.WuRongJunBig:
					return GoodsType.WuRongCha;
				case MonsterType.WuRongJunGolden:
					return GoodsType.WuRongCha;
				case MonsterType.LingXuSheng:
					return GoodsType.LingXuCha;
				case MonsterType.LingXuShengGolden:
					return GoodsType.LingXuCha;
				case MonsterType.LingXuShengBig:
					return GoodsType.LingXuCha;
				case MonsterType.XueBanHua:
					return GoodsType.XueBanCha;
				case MonsterType.XueBanHuaGolden:
					return GoodsType.XueBanCha;
				case MonsterType.XueBanHuaBig:
					return GoodsType.XueBanCha;
				case MonsterType.MuLingYa:
					return GoodsType.MuLingCha;
				case MonsterType.MuLingYaGolden:
					return GoodsType.MuLingCha;
				case MonsterType.MuLingYaBig:
					return GoodsType.MuLingCha;
				case MonsterType.JingRuiCao:
					return GoodsType.JingRuiCha;
				case MonsterType.JingRuiCaoGolden:
					return GoodsType.JingRuiCha;
				case MonsterType.JingRuiCaoBig:
					return GoodsType.JingRuiCha;
				case MonsterType.TieKuangShi:
					return GoodsType.QingYanJian;
				case MonsterType.TieKuangShiGolden:
					return GoodsType.QingYanJian;
				case MonsterType.TieKuangShiBig:
					return GoodsType.QingYanJian;
				case MonsterType.YinKuangShi:
					return GoodsType.YinSiDao;
				case MonsterType.YinKuangShiGolden:
					return GoodsType.YinSiDao;
				case MonsterType.YinKuangShiBig:
					return GoodsType.YinSiDao;
				case MonsterType.TongKuangShi:
					return GoodsType.TongWenDao;
				case MonsterType.TongKuangShiGolden:
					return GoodsType.TongWenDao;
				case MonsterType.TongKuangShiBig:
					return GoodsType.TongWenDao;
				case MonsterType.ZiJingShi:
					return GoodsType.ZiWuJian;
				case MonsterType.ZiJingShiGolden:
					return GoodsType.ZiWuJian;
				case MonsterType.ZiJingShiBig:
					return GoodsType.ZiWuJian;
				case MonsterType.YueJingShi:
					return GoodsType.YueXinJing;
				case MonsterType.YueJingShiGolden:
					return GoodsType.YueXinJing;
				case MonsterType.YueJingShiBig:
					return GoodsType.YueXinJing;
			}
			return GoodsType.None;
		}

		public static DropItemType GetDropTypeByMonsterType(MonsterType type)
		{
			switch (type)
			{
				case MonsterType.ShuangYunZhi:
					return DropItemType.ShuangYunZhiFragment;
				case MonsterType.ShuangYunZhiGolden:
					return DropItemType.ShuangYunZhiFragment;
				case MonsterType.ShuangYunZhiBig:
					return DropItemType.ShuangYunZhiFragment;
				case MonsterType.YueLuCao:
					return DropItemType.YueLuCaoFragment;
				case MonsterType.YueLuCaoGolden:
					return DropItemType.YueLuCaoFragment;
				case MonsterType.YueLuCaoBig:
					return DropItemType.YueLuCaoFragment;
				case MonsterType.ZiXinHua:
					return DropItemType.ZiXinHuaFragment;
				case MonsterType.ZiXinHuaGolden:
					return DropItemType.ZiXinHuaFragment;
				case MonsterType.ZiXinHuaBig:
					return DropItemType.ZiXinHuaFragment;
				case MonsterType.YuHuiHe:
					return DropItemType.YuHuiHeFragment;
				case MonsterType.YuHuiHeGolden:
					return DropItemType.YuHuiHeFragment;
				case MonsterType.YuHuiHeBig:
					return DropItemType.YuHuiHeFragment;
				case MonsterType.XingWenGuo:
					return DropItemType.XingWenGuoFragment;
				case MonsterType.XingWenGuoGolden:
					return DropItemType.XingWenGuoFragment;
				case MonsterType.XingWenGuoBig:
					return DropItemType.XingWenGuoFragment;
				case MonsterType.WuRongJun:
					return DropItemType.WuRongJunFragment;
				case MonsterType.WuRongJunBig:
					return DropItemType.WuRongJunFragment;
				case MonsterType.WuRongJunGolden:
					return DropItemType.WuRongJunFragment;
				case MonsterType.LingXuSheng:
					return DropItemType.LingXuShengFragment;
				case MonsterType.LingXuShengGolden:
					return DropItemType.LingXuShengFragment;
				case MonsterType.LingXuShengBig:
					return DropItemType.LingXuShengFragment;
				case MonsterType.XueBanHua:
					return DropItemType.XueBanHuaFragment;
				case MonsterType.XueBanHuaGolden:
					return DropItemType.XueBanHuaFragment;
				case MonsterType.XueBanHuaBig:
					return DropItemType.XueBanHuaFragment;
				case MonsterType.MuLingYa:
					return DropItemType.MuLingYaFragment;
				case MonsterType.MuLingYaGolden:
					return DropItemType.MuLingYaFragment;
				case MonsterType.MuLingYaBig:
					return DropItemType.MuLingYaFragment;
				case MonsterType.JingRuiCao:
					return DropItemType.JingRuiCaoFragment;
				case MonsterType.JingRuiCaoGolden:
					return DropItemType.JingRuiCaoFragment;
				case MonsterType.JingRuiCaoBig:
					return DropItemType.JingRuiCaoFragment;
				case MonsterType.TieKuangShi:
					return DropItemType.TieKuangShiFragment;
				case MonsterType.TieKuangShiGolden:
					return DropItemType.TieKuangShiFragment;
				case MonsterType.TieKuangShiBig:
					return DropItemType.TieKuangShiFragment;
				case MonsterType.YinKuangShi:
					return DropItemType.YinKuangShiFragment;
				case MonsterType.YinKuangShiGolden:
					return DropItemType.YinKuangShiFragment;
				case MonsterType.YinKuangShiBig:
					return DropItemType.YinKuangShiFragment;
				case MonsterType.TongKuangShi:
					return DropItemType.TongKuangShiFragment;
				case MonsterType.TongKuangShiGolden:
					return DropItemType.TongKuangShiFragment;
				case MonsterType.TongKuangShiBig:
					return DropItemType.TongKuangShiFragment;
				case MonsterType.ZiJingShi:
					return DropItemType.ZiJingShiFragment;
				case MonsterType.ZiJingShiGolden:
					return DropItemType.ZiJingShiFragment;
				case MonsterType.ZiJingShiBig:
					return DropItemType.ZiJingShiFragment;
				case MonsterType.YueJingShi:
					return DropItemType.YueJingShiFragment;
				case MonsterType.YueJingShiGolden:
					return DropItemType.YueJingShiFragment;
				case MonsterType.YueJingShiBig:
					return DropItemType.YueJingShiFragment;
			}
			return DropItemType.None;
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
				case MonsterType.JingYuanBao:
					resName = "JingYuanBao";
					break;
			}
			return resName;
		}

		public static string GetSMonsterRegionNameByType(MonsterType type)
		{
			string resName = "";
			switch (type)
			{
				case MonsterType.ShuangYunZhi:
					resName = "霜云芝";
					break;
				case MonsterType.ShuangYunZhiBig:
					resName = "巨型霜云芝";
					break;
				case MonsterType.YueLuCao:
					resName = "月露草";
					break;
				case MonsterType.YueLuCaoBig:
					resName = "巨型月露草";
					break;
				case MonsterType.ZiXinHua:
					resName = "栀心花";
					break;
				case MonsterType.ZiXinHuaBig:
					resName = "巨型栀心花";
					break;
				case MonsterType.YuHuiHe:
					resName = "玉穗禾";
					break;
				case MonsterType.YuHuiHeBig:
					resName = "巨型玉穗禾";
					break;
				case MonsterType.XingWenGuo:
					resName = "星纹果";
					break;
				case MonsterType.XingWenGuoBig:
					resName = "巨型星纹果";
					break;
				case MonsterType.WuRongJun:
					resName = "雾茸菌";
					break;
				case MonsterType.WuRongJunBig:
					resName = "巨型雾茸菌";
					break;
				case MonsterType.LingXuSheng:
					resName = "灵须参";
					break;
				case MonsterType.LingXuShengBig:
					resName = "巨型灵须参";
					break;
				case MonsterType.XueBanHua:
					resName = "雪瓣花";
					break;
				case MonsterType.XueBanHuaBig:
					resName = "巨型雪瓣花";
					break;
				case MonsterType.MuLingYa:
					resName = "木灵芽";
					break;
				case MonsterType.MuLingYaBig:
					resName = "巨型木灵芽";
					break;
				case MonsterType.JingRuiCao:
					resName = "晶蕊草";
					break;
				case MonsterType.JingRuiCaoBig:
					resName = "巨型晶蕊草";
					break;
				case MonsterType.TieKuangShi:
					resName = "铁矿石";
					break;
				case MonsterType.TieKuangShiBig:
					resName = "巨型铁矿石";
					break;
				case MonsterType.YinKuangShi:
					resName = "银矿石";
					break;
				case MonsterType.YinKuangShiBig:
					resName = "巨型银矿石";
					break;
				case MonsterType.TongKuangShi:
					resName = "铜矿石";
					break;
				case MonsterType.TongKuangShiBig:
					resName = "巨型铜矿石";
					break;
				case MonsterType.ZiJingShi:
					resName = "紫金石";
					break;
				case MonsterType.ZiJingShiBig:
					resName = "巨型紫金石";
					break;
				case MonsterType.YueJingShi:
					resName = "月晶石";
					break;
				case MonsterType.YueJingShiBig:
					resName = "巨型月晶石";
					break;
				case MonsterType.JingYuanBao:
					resName = "金元宝";
					break;
			}
			return resName;

		}



		public static string GetBuilderTypeNameByType(BuildingType type)
		{
			string resName = "";
			switch (type)
			{
				case BuildingType.LingZhangTai:
					resName = "灵账台";
					break;
				case BuildingType.LingChuGe_1:
					resName = "一号灵储阁";
					break;
				case BuildingType.LingChuGe_2:
					resName = "二号灵储阁";
					break;
				case BuildingType.YunDiGe:
					resName = "云递阁";
					break;
				case BuildingType.LingChaJia_1:
					resName = "一号灵茶架";
					break;
				case BuildingType.LingChaJia_2:
					resName = "二号灵茶架";
					break;
				case BuildingType.LingChaJia_3:
					resName = "三号灵茶架";
					break;
				case BuildingType.LingChaJia_4:
					resName = "四号灵茶架";
					break;
				case BuildingType.YuShaHu_1:
					resName = "一号玉砂壶";
					break;
				case BuildingType.YuShaHu_2:
					resName = "二号玉砂壶";
					break;
				case BuildingType.YuShaHu_3:
					resName = "三号玉砂壶";
					break;
				case BuildingType.YuShaHu_4:
					resName = "四号玉砂壶";
					break;
				case BuildingType.LianQiLu_1:
					resName = "一号炼器炉";
					break;
				case BuildingType.LianQiLu_2:
					resName = "二号炼器炉";
					break;
				case BuildingType.LianQiLu_3:
					resName = "三号炼器炉";
					break;
				case BuildingType.LingQiJia_1:
					resName = "一号灵器架";	
					break;
				case BuildingType.LingQiJia_2:
					resName = "二号灵器架";	
					break;
				case BuildingType.LingQiJia_3:
					resName = "三号灵器架";
					break;
			}
			return resName;

		}

		public static string GetCustomerResNameByType(CustomerType type)
		{
			string resName = "";
			switch (type)
			{
				case CustomerType.YunZhiTangDiZi:
					resName = "YunZhiTangDiZi";
					break;
				case CustomerType.YunZhiTangZhangLao:
					resName = "YunZhiTangZhangLao";
					break;
				case CustomerType.CangYunGeDiZi:
					resName = "CangYunGeDiZi";
					break;
				case CustomerType.CangYunGeZhangLao:
					resName = "CangYunGeZhangLao";
					break;
				case CustomerType.QingLanGuDiZi:
					resName = "QingLanGuDiZi";
					break;
				case CustomerType.QingLanGuZhangLao:
					resName = "CangYunGeZhangLao";
					break;
				case CustomerType.SuCaiGeDiZi:
					resName = "SuCaiGeDiZi";
					break;
				case CustomerType.SuCaiGeZhangLao:
					resName = "SuCaiGeZhangLao";
					break;
			}
			return resName;

		}

		public static string GetTaskInfoResNameByTypeWithId(TaskType type, int id)
		{
			string resName = "";
			switch (type)
			{

				case TaskType.Harvest:
					GetHarvestStr(id, out resName);
					break;
				case TaskType.Makemoney:
					resName = "TongBi";
					break;
				case TaskType.Produce:
				case TaskType.Sell:
					GetGoodsStr(id, out resName);
					break;
				case TaskType.Construct:
				case TaskType.Upgrade:
					GetStructStr(id, out resName);
					break;
				case TaskType.Unlock:
					resName = "MapLock";
					break;
			}

			return resName;
		}

		public static void GetHarvestStr(int id, out string resName)
		{
			resName = "";
			switch (id)
			{
				case 1:
					resName = "ShuangYunZhi";
					break;
				case 2:
					resName = "YueLuCao";
					break;
				case 3:
					resName = "ZiXinHua";
					break;
				case 4:
					resName = "YuHuiHe";
					break;
				case 5:
					resName = "XingWenGuo";
					break;
				case 6:
					resName = "WuRongJun";
					break;
				case 7:
					resName = "LingXuSheng";
					break;
				case 8:
					resName = "XueBanHua";
					break;
				case 9:
					resName = "MuLingYa";
					break;
				case 10:
					resName = "JingRuiCao";
					break;
				case 11:
					resName = "TieKuangShi";
					break;
				case 12:
					resName = "YinKuangShi";
					break;
				case 13:
					resName = "TongKuangShi";
					break;
				case 14:
					resName = "ZiJingShi";
					break;
				case 15:
					resName = "YueJingShi";
					break;
			}
		}

		public static void GetGoodsStr(int id, out string resName)
		{
			resName = "";
			switch (id)
			{
				case 1:
					resName = "YunZhiCha";
					break;
				case 2:
					resName = "YueLuCha";
					break;
				case 3:
					resName = "ZiXinCha";
					break;
				case 4:
					resName = "YuHeCha";
					break;
				case 5:
					resName = "XingWenCha";
					break;
				case 6:
					resName = "WuRongCha";
					break;
				case 7:
					resName = "LingXuCha";
					break;
				case 8:
					resName = "XueBanCha";
					break;
				case 9:
					resName = "MuLingCha";
					break;
				case 10:
					resName = "JingRuiCha";
					break;
				case 11:
					resName = "QingYanJian";
					break;
				case 12:
					resName = "YinSiDao";
					break;
				case 13:
					resName = "TongWenDao";
					break;
				case 14:
					resName = "ZiWuJian";
					break;
				case 15:
					resName = "YueXinJing";
					break;
				case 16:
					resName = "JingYuanBao";
					break;
				case 17:
					resName = "YingQian";
					break;
			}
		}

		public static void GetStructStr(int id, out string resName)
		{
			resName = "";
			switch (id)
			{
				case 1:
					resName = "LingZhangTai";
					break;
				case 2:
					resName = "LingChuGe";
					break;
				case 3:
					resName = "YunDiGe";
					break;
				case 4:
					resName = "LingChaJia";
					break;
				case 5:
					resName = "YuShaHu_1";
					break;
				case 6:
					resName = "LingChaJia";
					break;
				case 7:
					resName = "YuShaHu_2";
					break;
				case 8:
					resName = "LingChaJia";
					break;
				case 9:
					resName = "YuShaHu_3";
					break;
				case 10:
					resName = "LingChaJia";
					break;
				case 11:
					resName = "YuShaHu_4";
					break;
				case 12:
					resName = "LianQiLu_1";
					break;
				case 13:
					resName = "LingQiJia";
					break;
				case 14:
					resName = "LianQiLu_2";
					break;
				case 15:
					resName = "LingQiJia";
					break;
				case 16:
					resName = "LianQiLu_3";
					break;
				case 17:
					resName = "LingQiJia";
					break;
				case 18:
					resName = "YuanBaoKuangDong";
					break;
				case 19:
					resName = "LingChuGe";
					break;
			}
		}


		public static (MonsterType, DropItemType) ExchangeFamilyType(MonsterFamily monsterFamily)
		{
			MonsterType type1 = MonsterType.None;
			DropItemType type2 = DropItemType.None;
			switch (monsterFamily)
			{
				case MonsterFamily.ShuangYunZhi:
					type1 = MonsterType.ShuangYunZhi;
					type2 = DropItemType.ShuangYunZhiFragment;
					break;
				case MonsterFamily.YueLuCao:
					type1 = MonsterType.YueLuCao;
					type2 = DropItemType.YueLuCaoFragment;
					break;
				case MonsterFamily.ZiXinHua:
					type1 = MonsterType.ZiXinHua;
					type2 = DropItemType.ZiXinHuaFragment;
					break;
				case MonsterFamily.YuHuiHe:
					type1 = MonsterType.YuHuiHe;
					type2 = DropItemType.YuHuiHeFragment;
					break;
				case MonsterFamily.XingWenGuo:
					type1 = MonsterType.XingWenGuo;
					type2 = DropItemType.XingWenGuoFragment;
					break;
				case MonsterFamily.WuRongJun:
					type1 = MonsterType.WuRongJun;
					type2 = DropItemType.WuRongJunFragment;
					break;
				case MonsterFamily.LingXuSheng:
					type1 = MonsterType.LingXuSheng;
					type2 = DropItemType.LingXuShengFragment;
					break;
				case MonsterFamily.XueBanHua:
					type1 = MonsterType.XueBanHua;
					type2 = DropItemType.XueBanHuaFragment;
					break;
				case MonsterFamily.MuLingYa:
					type1 = MonsterType.MuLingYa;
					type2 = DropItemType.MuLingYaFragment;
					break;
				case MonsterFamily.JingRuiCao:
					type1 = MonsterType.JingRuiCao;
					type2 = DropItemType.JingRuiCaoFragment;
					break;
				case MonsterFamily.TieKuangShi:
					type1 = MonsterType.TieKuangShi;
					type2 = DropItemType.TieKuangShiFragment;
					break;
				case MonsterFamily.YinKuangShi:
					type1 = MonsterType.YinKuangShi;
					type2 = DropItemType.YinKuangShiFragment;
					break;
				case MonsterFamily.TongKuangShi:
					type1 = MonsterType.TongKuangShi;
					type2 = DropItemType.TongKuangShiFragment;
					break;
				case MonsterFamily.ZiJingShi:
					type1 = MonsterType.ZiJingShi;
					type2 = DropItemType.ZiJingShiFragment;
					break;
				case MonsterFamily.YueJingShi:
					type1 = MonsterType.YueJingShi;
					type2 = DropItemType.YueJingShiFragment;
					break;
			}
			return (type1, type2);
		}



		public static string GetStructureResNameByType(BuildingType type)
		{
			string resName = "";
			switch (type)
			{
				case BuildingType.LingChaJia_1:
					resName = "LingChaJia";
					break;
				case BuildingType.YuShaHu_1:
					resName = "YuShaHu_1";
					break;
				case BuildingType.LingChaJia_2:
					resName = "LingChaJia";
					break;
				case BuildingType.YuShaHu_2:
					resName = "YuShaHu_2";
					break;
				case BuildingType.LingChaJia_3:
					resName = "LingChaJia";
					break;
				case BuildingType.YuShaHu_3:
					resName = "YuShaHu_3";
					break;
				case BuildingType.LingChaJia_4:
					resName = "LingChaJia";
					break;
				case BuildingType.YuShaHu_4:
					resName = "YuShaHu_4";
					break;
				case BuildingType.LianQiLu_1:
					resName = "LianQiLu_1";
					break;
				case BuildingType.LingQiJia_1:
					resName = "LingQiJia";
					break;
				case BuildingType.LianQiLu_2:
					resName = "LianQiLu_2";
					break;
				case BuildingType.LingQiJia_2:
					resName = "LingQiJia";
					break;
				case BuildingType.LianQiLu_3:
					resName = "LianQiLu_3";
					break;
				case BuildingType.LingQiJia_3:
					resName = "LingQiJia";
					break;
				case BuildingType.YuanBaoKuangDong:
					resName = "YuanBaoKuangDong";
					break;
				case BuildingType.LingChuGe_2:
					resName = "LingChuGe";
					break;
				case BuildingType.LingChuGe_1:
					resName = "LingChuGe";
					break;
				case BuildingType.YunDiGe:
					resName = "YunDiGe";
					break;
				case BuildingType.LingZhangTai:
					resName = "LingZhangTai";
					break;

			}
			return resName;
		}


		public static string GetStructureNameByType(BuildingType type)
		{
			string resName = "";
			switch (type)
			{
				case BuildingType.LingChaJia_1:
					resName = "一号灵茶架";
					break;
				case BuildingType.YuShaHu_1:
					resName = "一号玉沙壶";
					break;
				case BuildingType.LingChaJia_2:
					resName = "二号灵茶架";
					break;
				case BuildingType.YuShaHu_2:
					resName = "二号玉沙壶";
					break;
				case BuildingType.LingChaJia_3:
					resName = "三号灵茶架";
					break;
				case BuildingType.YuShaHu_3:
					resName = "三号玉沙壶";
					break;
				case BuildingType.LingChaJia_4:
					resName = "四号灵茶架";
					break;
				case BuildingType.YuShaHu_4:
					resName = "四号玉沙壶";
					break;
				case BuildingType.LianQiLu_1:
					resName = "一号炼器炉";
					break;
				case BuildingType.LingQiJia_1:
					resName = "一号灵器架";
					break;
				case BuildingType.LianQiLu_2:
					resName = "二号炼器炉";
					break;
				case BuildingType.LingQiJia_2:
					resName = "二号灵器架";
					break;
				case BuildingType.LianQiLu_3:
					resName = "三号炼器炉";
					break;
				case BuildingType.LingQiJia_3:
					resName = "三号灵器架";
					break;
				case BuildingType.YuanBaoKuangDong:
					resName = "元宝矿洞";
					break;
				case BuildingType.LingChuGe_2:
					resName = "二号灵储阁";
					break;
				case BuildingType.LingChuGe_1:
					resName = "一号灵储阁";
					break;
				case BuildingType.YunDiGe:
					resName = "云递阁                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        ";
					break;
				case BuildingType.LingZhangTai:
					resName = "灵账台";
					break;
			}
			return resName;
		}
	}
}