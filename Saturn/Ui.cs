using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using ImGuiNET;
using imnodesNET;
using Saturn.Animations;
using Saturn.Animations.EasingFunctions;
using Saturn.Infrastructures;
using Saturn.Infrastructures.Utils;
using static ImGuiNET.ImGui;
using im = ImGuiNET.ImGui;

namespace Saturn
{
	class Ui : IDisposable
	{
		public static Ui Instance { get; } = new Ui();

		private Ui()
		{
			api.PluginInterface.UiBuilder.DisableAutomaticUiHide = true;
			api.PluginInterface.UiBuilder.Draw += UiBuilder_Draw;
		}

		public void Dispose()
		{
			api.PluginInterface.UiBuilder.Draw -= UiBuilder_Draw;
		}

		Config c => Config.Instance;
		private Vector3 upVector;

		private float speed = 1;
		private float radian = (float)(Math.PI * 2);
		private Vector3 axis = Vector3.UnitY;
		private Path3F path = new Path3F() { SmoothEnds = true , PreLoop = CurveLoopType.Constant, PostLoop = CurveLoopType.Constant};

		private void PathWindow()
		{
			if (Begin("DigitalRune"))
			{
				var instanceEye = MainCameraHook.Instance.Eye;
				if (Button($"ADD {instanceEye}###addpoint") || IsKeyPressed((int)VirtualKey.C, true))
				{
					path.Add(new PathKey3F { Parameter = path.Count, Interpolation = SplineInterpolation.CatmullRom, Point = instanceEye.ToVector3F() });
				}
				if (Button($"Parameterize"))
				{
					path.ParameterizeByLength(10, 0.01f);
				}
				if (Button($"clear"))
				{
					path.Clear();
				}

				foreach (var p in path)
				{
					DrawVector(p.Point.ToVector3(), ImGuiColors.ParsedBlue);
				}
			}

			End();
		}

		private unsafe void UiBuilder_Draw()
		{
			PathWindow();
			//im.SetNextWindowPos(new Vector2(200, 200), ImGuiCond.Appearing);
			//if (im.Begin("arc"))
			//{
			//	im.SliderFloat("speed", ref speed, 0.1f, 10, speed.ToString(), ImGuiSliderFlags.Logarithmic | ImGuiSliderFlags.NoRoundToFormat);
			//	im.SliderAngle("angle", ref radian, -360, 360);
			//	im.SliderFloat3("axis", ref axis, -1, 1);
			//	//if (api.TargetManager.Target is not null)
			//	{
			//		var valueInternal = QuickAnimation(speed);
			//		im.TextUnformatted($"{valueInternal}");
			//		var normalize = Vector3.Normalize(axis);
			//		for (int i = 0; i < 16; i++)
			//		{
			//			var j = i / 16f;
			//			var ret = Arc.CalculateValue(normalize, j + valueInternal);
			//			drawVector(ret, api.ClientState.LocalPlayer?.Position ?? Vector3.Zero, ImGuiColors.DalamudRed);
			//		}
			//		drawVector(Arc.GetVerticalDir(normalize), api.ClientState.LocalPlayer?.Position ?? Vector3.Zero, ImGuiColors.HealerGreen);

			//		var path3F = new Path3F();
			//		path3F.Add(new PathKey3F(){Parameter = 0, Interpolation = SplineInterpolation.CatmullRom, Point = new Vector3F()});
			//		new DigitalRune.Animation.Path3FAnimation(path3F).CreateInstance().Animation.
			//	}
			//}





			if (Begin(nameof(Saturn)))
			{
				if (Button("record current cam position"))
				{
					c.CamPosList.Add((MainCameraHook.Instance.Eye, MainCameraHook.Instance.Target));
				}

				SameLine();

				if (Button("remove last"))
				{
					try
					{
						c.CamPosList.RemoveAt(c.CamPosList.Count - 1);
					}
					catch (Exception e)
					{
						//PluginLog.Warning(e.ToString());
					}
				}

				if (Button("add control"))
				{
					//upVector = MainCameraHook.Instance.Unk;
					try
					{
						easingEye = AnimationTypes.GetEasing(c.AnimationTypeEye, c.animationTimeMs);
						easingEye.Point1 = MainCameraHook.Instance.Eye;
						easingEye.Point2 = c.CamPosList[^1].eye;


						easingTarget = AnimationTypes.GetEasing(c.AnimationTypeTarget, c.animationTimeMs);
						easingTarget.Point1 = MainCameraHook.Instance.Target;
						easingTarget.Point2 = c.CamPosList[^1].target;

						if (c.animReverse)
						{
							(easingEye.Point2, easingEye.Point1) = (easingEye.Point1, easingEye.Point2);
							(easingTarget.Point2, easingTarget.Point1) = (easingTarget.Point1, easingTarget.Point2);
						}
					}
					catch (Exception e)
					{
						PluginLog.Error(e.ToString());
					}

					MainCameraHook.Instance.DoCamControl += Instance_DoCamControl;
				}

				SameLine();
				if (Button("clear control")) MainCameraHook.Instance.ClearControls();
				SameLine();
				if (Button("reset control")) upVector = Vector3.UnitY;
				SameLine();
				Checkbox("Loopback", ref c.LoopBack);

				DragInt("Speed in MS", ref c.animationTimeMs, 10, 0, 60000);
				SliderFloat3("upVector", ref upVector, -1, 1);
				if (IsItemClicked(ImGuiMouseButton.Right))
				{
					upVector = Vector3.UnitY;
				}

				imguiUtil.EnumCombo("EYE ANIM", ref c.AnimationTypeEye, ImGuiComboFlags.HeightLarge);
				imguiUtil.EnumCombo("TARGET ANIM", ref c.AnimationTypeTarget, ImGuiComboFlags.HeightLarge);
				Checkbox("REVERSE", ref c.animReverse);

				var label = MainCameraHook.Instance.MatrixPtr.ToInt64().ToString("X");
				CopyButton(label, 1111);
				TextUnformatted($"U:{MainCameraHook.Instance.Unk}");
				TextUnformatted($"E:{MainCameraHook.Instance.Eye}");
				TextUnformatted($"T:{MainCameraHook.Instance.Target}");
				//im.TextUnformatted($"{MainCameraHook.Instance.ViewMatrix}");
				Separator();


				//im.SliderFloat(nameof(width), ref width, 0, 3000);
				//im.SliderFloat(nameof(height), ref height, 0, 3000);
				//im.SliderFloat(nameof(fieldOfView), ref fieldOfView, 0, (float)Math.PI);
				//im.SliderFloat(nameof(aspectRatio), ref aspectRatio, -10, 10);
				//im.SliderFloat(nameof(nearPlaneDistance), ref nearPlaneDistance, 0, 10);
				//im.SliderFloat(nameof(farPlaneDistance), ref farPlaneDistance, 0, 100000);

				//try
				//{
				//	var view = Matrix4x4.CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance);
				//	//var view = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
				//	//view = Matrix4x4.Identity;
				//	var floats = (float*)&view;
				//	if (im.BeginTable("matrixtable", 4))
				//	{
				//		for (int i = 0; i < 4; i++)
				//		{
				//			im.TableNextRow();
				//			for (int j = 0; j < 4; j++)
				//			{
				//				im.TableNextColumn();
				//				im.TextUnformatted(floats[i * 4 + j].ToString());
				//			}
				//		}
				//		im.EndTable();
				//	}
				//}
				//catch (Exception e)
				//{
				//	im.TextUnformatted(e.ToString());
				//}


				foreach (var pos in c.CamPosList)
				{
					TextUnformatted($"E:{pos.eye}");
					SameLine(300);
					TextUnformatted($"T:{pos.target}");
					if (!api.GameGui.GameUiHidden)
					{
						if (api.GameGui.WorldToScreen(pos.target, out var screenpos))
							GetBackgroundDrawList(GetMainViewport()).AddCircleFilled(screenpos, 3,
								ColorConvertFloat4ToU32(ImGuiColors.DPSRed));
						if (api.GameGui.WorldToScreen(pos.eye, out var screeneye))
							GetBackgroundDrawList(GetMainViewport()).AddCircleFilled(screeneye, 3,
								ColorConvertFloat4ToU32(ImGuiColors.HealerGreen));
					}
				}
			}

			End();

			//if (im.Begin("Cameras"))
			//{
			//	var cameraManager = Offsets.CameraManager;
			//	var worldCamera = (*(IntPtr*)cameraManager);
			//	var idleCamera = (*(IntPtr*)(cameraManager + 0x8));
			//	var menuCamera = (*(IntPtr*)(cameraManager + 0x10));
			//	var spectatorCamera = (*(IntPtr*)(cameraManager + 0x18));
			//	CopyButton(cameraManager.ToInt64().ToString("X"), 1);
			//	CopyButton(worldCamera.ToInt64().ToString("X"), 2);
			//	CopyButton(idleCamera.ToInt64().ToString("X"), 3);
			//	CopyButton(menuCamera.ToInt64().ToString("X"), 4);
			//	CopyButton(spectatorCamera.ToInt64().ToString("X"), 5);

			//	CopyButton(((delegate*<IntPtr>)Offsets.GetMatrixSingleton)().ToInt64().ToString("X"), 6);
			//}

			End();
		}



		private static void Draw3Vector(Vector3 normalize, Vector3 @base)
		{
			DrawVector(normalize, @base, ImGuiColors.DalamudRed);
			DrawVector(Arc.GetVerticalDir(normalize), @base, ImGuiColors.HealerGreen);
			DrawVector(Vector3.Cross(normalize, Arc.GetVerticalDir(normalize)), @base,
				ImGuiColors.TankBlue);
		}

		private static void DrawVector(Vector3 vector, Vector3 @base, Vector4 color)
		{
			if (api.GameGui.WorldToScreen(vector + @base, out var screenPos))
			{
				GetForegroundDrawList(GetMainViewport()).AddCircleFilled(screenPos, 4, ColorConvertFloat4ToU32(color));
				if (api.GameGui.WorldToScreen(@base, out var screenPos2))
				{
					GetForegroundDrawList(GetMainViewport())
						.AddLine(screenPos, screenPos2, ColorConvertFloat4ToU32(color), 1.5f);
				}
			}
		}

		private static void DrawVector(Vector3 vector, Vector4 color)
		{
			if (api.GameGui.WorldToScreen(vector, out var screenPos))
			{
				GetForegroundDrawList(GetMainViewport()).AddCircleFilled(screenPos, 4, ColorConvertFloat4ToU32(color));
			}
		}

		private static float QuickAnimation(double speed = 1)
		{
			var totalSeconds = DateTime.Now.TimeOfDay.TotalSeconds * speed;
			return (float)(totalSeconds - (int)totalSeconds);
		}

		private static void CopyButton(string label, int id = 0)
		{
			if (Button(label + "##" + id))
				SetClipboardText(label);
		}

		private EasingV3 easingEye;
		private EasingV3 easingTarget;
		private float fieldOfView;
		private float aspectRatio;
		private float nearPlaneDistance;
		private float farPlaneDistance;
		private float width;
		private float height;

		private unsafe void Instance_DoCamControl(Matrix4x4* matrix, System.Numerics.Vector3* eye, System.Numerics.Vector3* target, System.Numerics.Vector3* unk)
		{
			var eye1 = GetEaseVector(easingEye);
			var target1 = GetEaseVector(easingTarget);

			*matrix = Matrix4x4.CreateLookAt(eye1, target1, upVector);
		}

		private unsafe Vector3 GetEaseVector<T>(T easing) where T : EasingV3
		{
			if (!easing.IsRunning)
			{
				easing.Start();
			}

			if (easing.IsDone)
			{
				if (c.LoopBack)
				{
					(easing.Point1, easing.Point2) = (easing.Point2, easing.Point1);
					easing.Restart();
				}
				else
				{
					easing.Stop();
				}
			}


			easing.Update();

			return easing.EasedPoint;
		}
	}

	public static class imguiUtil
	{
		public static bool EnumCombo<TEnum>(string label, ref TEnum @enum, ImGuiComboFlags flags = ImGuiComboFlags.None) where TEnum : struct, Enum
		{
			bool ret = false;
			if (BeginCombo(label, @enum.ToString(), flags))
			{
				var text = @enum.ToString();
				var strings = Enum.GetNames<TEnum>();
				for (var i = 0; i < strings.Length; i++)
				{
					try
					{
						PushID(i);
						if (Selectable(strings[i], strings[i] == text))
						{
							ret = true;
							@enum = Enum.GetValues<TEnum>()[i];
						}
						PopID();
					}
					catch (Exception e)
					{
						PluginLog.Error(e.ToString());
					}
				}
				EndCombo();
			}

			return ret;
		}
	}
}
