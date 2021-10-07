using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dalamud.Configuration;
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
	}
}
