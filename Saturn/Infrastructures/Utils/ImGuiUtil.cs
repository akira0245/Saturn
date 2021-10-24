using System;
using Dalamud.Logging;
using ImGuiNET;

namespace Saturn
{
	public static class ImGuiUtil
	{
		public static bool EnumCombo<TEnum>(string label, ref TEnum @enum, ImGuiComboFlags flags = ImGuiComboFlags.None) where TEnum : struct, Enum
		{
			bool ret = false;
			if (ImGui.BeginCombo(label, @enum.ToString(), flags))
			{
				var text = @enum.ToString();
				var strings = Enum.GetNames<TEnum>();
				for (var i = 0; i < strings.Length; i++)
				{
					try
					{
						ImGui.PushID(i);
						if (ImGui.Selectable(strings[i], strings[i] == text))
						{
							ret = true;
							@enum = Enum.GetValues<TEnum>()[i];
						}
						ImGui.PopID();
					}
					catch (Exception e)
					{
						PluginLog.Error(e.ToString());
					}
				}
				ImGui.EndCombo();
			}

			return ret;
		}
	}
}