using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dalamud.Configuration;
using Dalamud.Logging;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Interpolation;
using Newtonsoft.Json;
using Saturn.Animations;
using Saturn.Infrastructures;

namespace Saturn
{
	class Config : IPluginConfiguration
	{
		#region misc

		public int Version { get; set; }
		[JsonIgnore] public static Config Instance { get; } = (Config)api.PluginInterface.GetPluginConfig() ?? new Config();
		public static void Save() => api.PluginInterface.SavePluginConfig(Instance);
		private Config() { }

		#endregion

		public List<(Vector3 eye, Vector3 target)> CamPosList = new();
		public int animationTimeMs = 3000;
		public AnimationTypes.AnimationType AnimationTypeEye = AnimationTypes.AnimationType.Linear;
		public AnimationTypes.AnimationType AnimationTypeTarget = AnimationTypes.AnimationType.Linear;
		public bool LoopBack;
		public bool animReverse;
		//private Path3F eyePath = new Path3F() { SmoothEnds = true, PreLoop = CurveLoopType.Constant, PostLoop = CurveLoopType.Constant };
		//private Path3F targetPath = new Path3F() { SmoothEnds = true, PreLoop = CurveLoopType.Constant, PostLoop = CurveLoopType.Constant };

		//public List<(Path3F eye, Path3F target)> paths;
	}

	static	class CamPath
	{
		public static unsafe string XmlSerializeString<T>(this T obj)
		{
			using (var s = new MemoryStream())
			{
				new XmlSerializer(typeof(T)).Serialize(s, obj);
				var b = s.ToArray();
				fixed (void* pb = &b[0])
				{
					return new string((sbyte*)pb, 0, b.Length);
				}
			}
		}

		//[JsonIgnore]
		//private unsafe Path3F EyePath
		//{
		//	get
		//	{

		//		var path3F = new Path3F() { SmoothEnds = true, PostLoop = CurveLoopType.Constant, PreLoop = CurveLoopType.Constant };
		//		path3F.AddRange(_eyePath);
		//		return path3F;
		//	}
		//}

		//[JsonIgnore]
		//private Path3F TargetPath
		//{
		//	get
		//	{
		//		var path3F = new Path3F() { SmoothEnds = true, PostLoop = CurveLoopType.Constant, PreLoop = CurveLoopType.Constant };
		//		path3F.AddRange(_targetPath);
		//		return path3F;
		//	}
		//}

		//private PathKey3F[] _eyePath;
		//private PathKey3F[] _targetPath;
		//void Save()
		//{
		//	_eyePath = EyePath.ToArray();
		//	_targetPath = TargetPath.ToArray();
		//}
	}
}
