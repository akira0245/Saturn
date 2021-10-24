using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Logging;
using DigitalRune.Collections;
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
	unsafe class Ui : IDisposable
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

		private float speed = 0.1f;
		private float radian = (float)(Math.PI * 2);
		private Vector3 axis = Vector3.UnitY;

		private bool repeat = false;

		private float prog = 0;

		private Path3F eyePath = new Path3F() { SmoothEnds = true, PreLoop = CurveLoopType.Constant, PostLoop = CurveLoopType.Oscillate };
		private Path3F targetPath = new Path3F() { SmoothEnds = true, PreLoop = CurveLoopType.Constant, PostLoop = CurveLoopType.Oscillate };

		private bool _freecaming;

		public unsafe bool freecaming
		{
			get => _freecaming;
			set
			{
				if (_freecaming != value)
				{
					if (value)
					{
						ReceiveControl = true;
						freeEye = ViewMatrixHook.Instance.Eye;
						freeTarget = ViewMatrixHook.Instance.Target;
						ViewMatrixHook.Instance.DoCamControl += (matrix, eye, target, unk) =>
						{
							currentCamDirection = Vector3.Normalize(*target - *eye);
							*eye = freeEye;
							*target = freeEye + currentCamDirection * 1000;
							*unk = Vector3.UnitY;
						};
					}
					else
					{
						ViewMatrixHook.Instance.ClearControls();
					}
				}

				_freecaming = value;
			}
		}

		private Vector3 freeEye;
		private Vector3 freeTarget;
		private Vector3 currentCamDirection;

		private List<bool> ViewingPath = new List<bool>();

		private void PathWindow()
		{

			if (Begin("Camera Path"))
			{
				if (repeat)
				{
					repeat = api.KeyState[VirtualKey.C];
				}

				TextUnformatted("Path:");
				if (Button("Add key point (KEY C)") || api.KeyState[VirtualKey.C] && !repeat)
				{
					AddCurrentCamPositionToPath();
					repeat = true;
				}
				SameLine();
				if (Button($"Clear (KEY V)") || api.KeyState[VirtualKey.V])
				{
					CameraPathClear();
				}
				SameLine();
				if (Button($"Parameterize"))
				{
					Task.Run(() =>
					{
						var startNew = Stopwatch.StartNew();
						eyePath.ParameterizeByLength(3000, 0.01f);
						PluginLog.Warning($"eyePath parameterize complete in {startNew.Elapsed}");
					});
					Task.Run(() =>
					{
						var startNew = Stopwatch.StartNew();
						targetPath.ParameterizeByLength(3000, 0.01f);
						PluginLog.Warning($"targetPath parameterize complete in {startNew.Elapsed}");
					});
				}
				Separator();
				TextUnformatted("Spline Interpolation:");
				if (Button($"CatmullRom (default)"))
				{
					foreach (var pathKey3F in eyePath) pathKey3F.Interpolation = SplineInterpolation.CatmullRom;
					foreach (var pathKey3F in targetPath) pathKey3F.Interpolation = SplineInterpolation.CatmullRom;
				}
				SameLine();
				if (Button($"BSpline"))
				{
					foreach (var pathKey3F in eyePath) pathKey3F.Interpolation = SplineInterpolation.BSpline;
					foreach (var pathKey3F in targetPath) pathKey3F.Interpolation = SplineInterpolation.BSpline;
				}
				SameLine();
				if (Button($"Hermite"))
				{
					foreach (var pathKey3F in eyePath) pathKey3F.Interpolation = SplineInterpolation.Hermite;
					foreach (var pathKey3F in targetPath) pathKey3F.Interpolation = SplineInterpolation.Hermite;
				}
				SameLine();
				if (Button($"Linear"))
				{
					foreach (var pathKey3F in eyePath) pathKey3F.Interpolation = SplineInterpolation.Linear;
					foreach (var pathKey3F in targetPath) pathKey3F.Interpolation = SplineInterpolation.Linear;
				}
				Separator();

				SliderFloat("speed", ref speed, 0.01f, 10, speed.ToString(), ImGuiSliderFlags.Logarithmic | ImGuiSliderFlags.NoRoundToFormat);

				TextUnformatted($"EyePath:\nCount: {eyePath.Count}\tLength: {eyePath.LastOrDefault()?.Parameter}");
				SameLine(GetWindowContentRegionWidth() / 2);
				TextUnformatted($"TargetPath:\nCount: {targetPath.Count}\tLength: {targetPath.LastOrDefault()?.Parameter}");

				if (BeginTable("PATHTABLE", 5))
				{
					for (int i = 0; i < eyePath.Count; i++)
					{
						var e = eyePath[i];
						var t = targetPath[i];
						PushID($"{i}path");
						TableNextRow();
						TableNextColumn();
						if (Selectable($"{i}", false, ImGuiSelectableFlags.SpanAllColumns))
						{
							ViewMatrixHook.Instance.DoCamControl += setCamPos;

							void setCamPos(Matrix4x4* matrix, Vector3* eye, Vector3* target, Vector3* unk)
							{
								*eye = e.Point.ToVector3();
								*target = t.Point.ToVector3();
								*unk = Vector3.UnitY;
							}

						}
						else if (IsWindowFocused() && IsItemHovered())
						{
							ViewMatrixHook.Instance.DoCamControl += setCamPos;

							void setCamPos(Matrix4x4* matrix, Vector3* eye, Vector3* target, Vector3* unk)
							{
								*eye = e.Point.ToVector3();
								*target = t.Point.ToVector3();
								*unk = Vector3.UnitY;
								ViewMatrixHook.Instance.DoCamControl -= setCamPos;
							}

						}


						TableNextColumn();
						TextUnformatted(e.Parameter.ToString("F4"));
						TableNextColumn();
						TextUnformatted(t.Parameter.ToString("F4"));
						TableNextColumn();

						PopID();
					}





					EndTable();
				}

				//DrawVector(axis, api.ClientState.LocalPlayer.Position, ImGuiColors.DalamudYellow);
				//DrawVector(Vector3.Normalize(Vector3.Cross(axis, Vector3.UnitX)), api.ClientState.LocalPlayer.Position, ImGuiColors.DalamudRed);
				//DrawVector(Vector3.Normalize(Vector3.Cross(axis, Vector3.UnitY)), api.ClientState.LocalPlayer.Position, ImGuiColors.HealerGreen);
				//DrawVector(Vector3.Normalize(Vector3.Cross(axis, Vector3.UnitZ)),api.ClientState.LocalPlayer.Position, ImGuiColors.TankBlue);


				DrawPath(eyePath, ImGuiColors.TankBlue, ImGuiColors.DalamudViolet, ImGuiColors.DalamudRed, out _currentEye);
				DrawPath(targetPath, ImGuiColors.ParsedPurple, ImGuiColors.DalamudViolet, ImGuiColors.ParsedGreen, out _currentTarget);

				//if (!api.GameGui.GameUiHidden)
				//{
				//	try
				//	{
				//		api.GameGui.WorldToScreen(_currentEye.Value, out var eye);
				//		api.GameGui.WorldToScreen(_currentTarget.Value, out var target);
				//		BDL.AddLine(eye, target, GetColorU32(ImGuiColors.DalamudYellow));
				//	}
				//	catch (Exception e)
				//	{

				//	}
				//}
			}
			//ImGui.GetIO().WantCaptureKeyboard = false;
			//ImGui.CaptureKeyboardFromApp(false);
			End();
		}

		public void CameraPathClear()
		{
			eyePath.Clear();
			targetPath.Clear();
		}

		public void AddCurrentCamPositionToPath()
		{
			eyePath.Add(new PathKey3F
			{
				Parameter = eyePath.Count,
				Interpolation = SplineInterpolation.CatmullRom,
				Point = freecaming ? freeEye.ToVector3F() : ViewMatrixHook.Instance.Eye.ToVector3F(),
			});
			targetPath.Add(new PathKey3F
			{
				Parameter = targetPath.Count,
				Interpolation = SplineInterpolation.CatmullRom,
				Point = freecaming
					? (freeEye + currentCamDirection * 500).ToVector3F()
					: ViewMatrixHook.Instance.Target.ToVector3F(),
			});
		}

		private bool ReceiveControl = true;
		private unsafe void freecamControl()
		{
			if (ImGui.Begin("Freecam"))
			{

				if (Button(freecaming ? "EXIT FREECAM" : "FREECAM MODE")) freecaming ^= true;
				if (freecaming)
				{
					SameLine();
					if (Button(ReceiveControl ? "DISABLE CONTROL" : "ENABLE CONTROL"))
						ReceiveControl ^= true;
				}

				PushFont(UiBuilder.MonoFont);

				TextUnformatted(freecaming ? "FREECAMING" : "");
				TextUnformatted(freecaming && ReceiveControl ? "CONTROLLING" : "");

				TextUnformatted("W A S D    Move");
				TextUnformatted("E          Up");
				TextUnformatted("Q          Down");
				TextUnformatted("SHIFT      Speed+");
				TextUnformatted("CONTROL    Speed-");
				TextUnformatted("ESCAPE     Exit");
				PopFont();
				if (freecaming)
				{

					if (ReceiveControl)
					{
						ImGui.GetIO().WantTextInput = true;

						if (IsKeyReleased((int)VirtualKey.C))
						{
							AddCurrentCamPositionToPath();
						}

						var delta = IsKeyDown((int)VirtualKey.SHIFT) ? currentCamDirection :
							IsKeyDown((int)VirtualKey.MENU) ? currentCamDirection * 0.04f : currentCamDirection * 0.2f;
						var deltaY = IsKeyDown((int)VirtualKey.SHIFT) ? Vector3.UnitY :
							IsKeyDown((int)VirtualKey.MENU) ? Vector3.UnitY * 0.04f : Vector3.UnitY * 0.2f;


						delta *= (float)(api.Framework.UpdateDelta.TotalMilliseconds / 16.6666666666667D);
						deltaY *= (float)(api.Framework.UpdateDelta.TotalMilliseconds / 16.6666666666667D);

						if (IsKeyDown((int)VirtualKey.W)) freeEye += delta;
						if (IsKeyDown((int)VirtualKey.S)) freeEye -= delta;
						if (IsKeyDown((int)VirtualKey.A)) freeEye -= Vector3.Cross(delta, Vector3.UnitY);
						if (IsKeyDown((int)VirtualKey.D)) freeEye += Vector3.Cross(delta, Vector3.UnitY);
						if (IsKeyDown((int)VirtualKey.R)) freeEye = api.ClientState.LocalPlayer?.Position ?? Vector3.Zero;
						//if (IsKeyDown((int)VirtualKey.E)) freeEye += Vector3.Normalize(Vector3.Cross(delta, Vector3.Cross(delta, Vector3.UnitY)));
						//if (IsKeyDown((int)VirtualKey.Q)) freeEye -= Vector3.Normalize(Vector3.Cross(delta, Vector3.Cross(delta, Vector3.UnitY)));
						if (IsKeyDown((int)VirtualKey.SPACE)) freeEye += deltaY;
						if (IsKeyDown((int)VirtualKey.CONTROL)) freeEye -= deltaY;

						if (IsKeyDown((int)VirtualKey.ESCAPE))
						{
							freecaming = false;
						}
					}
				}
			}


			End();
		}


		private ImDrawListPtr BDL => ImGui.GetBackgroundDrawList(ImGui.GetMainViewport());
		private ImDrawListPtr FDL => ImGui.GetForegroundDrawList(ImGui.GetMainViewport());
		private float correction = 0f;
		private void DrawPath(Path3F path3F, Vector4 tankBlue, Vector4 dalamudViolet, Vector4 dalamudRed, out Vector3? currentValue)
		{
			currentValue = null;
			if (path3F.Any())
			{
				var length = path3F.Last().Parameter;
				var vector3F = path3F.GetPoint((QuickAnimation(speed)) * length);
				if (!api.GameGui.GameUiHidden)
				{
					foreach (var p in path3F)
					{
						DrawVector(p.Point.ToVector3(), tankBlue, 3.5f);
					}

					var l = (int)length;
					for (int i = 0; i < l; i++)
					{
						var point = path3F.GetPoint(length * i / l);
						DrawVector(point.ToVector3(), dalamudViolet, 2);
					}

					DrawVector(vector3F.ToVector3(), dalamudRed, 5);
				}

				currentValue = vector3F.ToVector3();
			}
		}

		public bool visible = true;
		private unsafe void UiBuilder_Draw()
		{
			if (!visible) return;

			DoF();
			freecamControl();
			PathWindow();


			if (Begin("Camera anim"))
			{
				//if (Button("record current cam position"))
				//{
				//	c.CamPosList.Add((MainCameraHook.Instance.Eye, MainCameraHook.Instance.Target));
				//}

				//SameLine();

				//if (Button("remove last"))
				//{
				//	try
				//	{
				//		c.CamPosList.RemoveAt(c.CamPosList.Count - 1);
				//	}
				//	catch (Exception e)
				//	{
				//		//PluginLog.Warning(e.ToString());
				//	}
				//}

				if (Button("begin control"))
				{
					////upVector = MainCameraHook.Instance.Unk;
					//try
					//{
					//	easingEye = AnimationTypes.GetEasing(c.AnimationTypeEye, c.animationTimeMs);
					//	easingEye.Point1 = MainCameraHook.Instance.Eye;
					//	easingEye.Point2 = c.CamPosList[^1].eye;


					//	easingTarget = AnimationTypes.GetEasing(c.AnimationTypeTarget, c.animationTimeMs);
					//	easingTarget.Point1 = MainCameraHook.Instance.Target;
					//	easingTarget.Point2 = c.CamPosList[^1].target;

					//	if (c.animReverse)
					//	{
					//		(easingEye.Point2, easingEye.Point1) = (easingEye.Point1, easingEye.Point2);
					//		(easingTarget.Point2, easingTarget.Point1) = (easingTarget.Point1, easingTarget.Point2);
					//	}
					//}
					//catch (Exception e)
					//{
					//	PluginLog.Error(e.ToString());
					//}
					CameraBeginControl();
				}

				SameLine();
				if (Button("clear control")) ViewMatrixHook.Instance.ClearControls();
				//SameLine();
				//if (Button("reset control")) upVector = Vector3.UnitY;
				//SameLine();
				//Checkbox("Loopback", ref c.LoopBack);

				//DragInt("Speed in MS", ref c.animationTimeMs, 10, 0, 60000);
				//SliderFloat3("upVector", ref upVector, -1, 1);
				//if (IsItemClicked(ImGuiMouseButton.Right))
				//{
				//	upVector = Vector3.UnitY;
				//}

				//imguiUtil.EnumCombo("EYE ANIM", ref c.AnimationTypeEye, ImGuiComboFlags.HeightLarge);
				//imguiUtil.EnumCombo("TARGET ANIM", ref c.AnimationTypeTarget, ImGuiComboFlags.HeightLarge);
				//Checkbox("REVERSE", ref c.animReverse);

				//var label = MainCameraHook.Instance.MatrixPtr.ToInt64().ToString("X");
				//CopyButton(label, 1111);

				TextUnformatted($"U:{ViewMatrixHook.Instance.Unk}");
				TextUnformatted($"E:{ViewMatrixHook.Instance.Eye}");
				TextUnformatted($"T:{ViewMatrixHook.Instance.Target}");
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

			//Cameraswindow();
		}

		public void CameraBeginControl()
		{
			correction = QuickAnimation(speed);
			ViewMatrixHook.Instance.DoCamControl += Instance_DoCamControl;
		}

		private static void Cameraswindow()
		{
			if (im.Begin("Cameras"))
			{
				var cameraManager = Offsets.CameraManager;
				var worldCamera = (*(IntPtr*)cameraManager);
				var idleCamera = (*(IntPtr*)(cameraManager + 0x8));
				var menuCamera = (*(IntPtr*)(cameraManager + 0x10));
				var spectatorCamera = (*(IntPtr*)(cameraManager + 0x18));
				CopyButton(cameraManager.ToInt64().ToString("X"), 1);
				CopyButton(worldCamera.ToInt64().ToString("X"), 2);
				CopyButton(idleCamera.ToInt64().ToString("X"), 3);
				CopyButton(menuCamera.ToInt64().ToString("X"), 4);
				CopyButton(spectatorCamera.ToInt64().ToString("X"), 5);

				CopyButton(ViewMatrixHook.Instance.MatrixPtr.ToInt64().ToString("X"), 7);
				CopyButton(((delegate*<IntPtr>)Offsets.GetMatrixSingleton)().ToInt64().ToString("X"), 6);
			}

			End();
		}

		private unsafe void DoF()
		{
			if (Begin("DOF"))
			{
				if (Button("DOF ON"))
				{
					DOF.Instance.DOFStructPtr->unkVector = Vector3.One;
					DOF.Instance.DOFStructPtr->Enabled = 0x40;
				}
				SameLine();
				if (Button("DOF OFF"))
				{
					DOF.Instance.DOFStructPtr->unkVector = Vector3.Zero;
					Task.Delay(100).ContinueWith(task => DOF.Instance.DOFStructPtr->Enabled = 0);
				}


				DragFloat3("UNK", ref DOF.Instance.DOFStructPtr->unkVector, 1f, 0.1f, 1000);
				SliderFloat("NEAR", ref DOF.Instance.Near, 0, DOF.Instance.Mid);
				SliderFloat("MID", ref DOF.Instance.Mid, DOF.Instance.Near, DOF.Instance.Far);
				SliderFloat("FAR", ref DOF.Instance.Far, DOF.Instance.Mid, 100, DOF.Instance.Far.ToString("F3"), ImGuiSliderFlags.NoRoundToFormat | ImGuiSliderFlags.Logarithmic);

				Spacing();
				var changingFocalPlane = SliderFloat("FOCAL PLANE", ref DOF.Instance.Mid, 0.5f, 1000, DOF.Instance.Mid.ToString(), ImGuiSliderFlags.NoRoundToFormat | ImGuiSliderFlags.Logarithmic);
				if (changingFocalPlane | SliderFloat("APERTURE VALUE", ref Config.Instance.Aperture, 0.001f, 1))
				{
					DOF.Instance.Near = DOF.Instance.Mid * Config.Instance.Aperture;
					DOF.Instance.Far = DOF.Instance.Mid / Config.Instance.Aperture;
				}

				if (changingFocalPlane) Config.Instance.DOFAutoFocus = false;

				Checkbox("AUTO FOCUS", ref Config.Instance.DOFAutoFocus);

				if (Config.Instance.DOFAutoFocus)
				{
					if ((_currentEye != null && _currentTarget != null))
					{
						DOF.Instance.Mid = (_currentEye - _currentTarget).Value.Length();
					}
					else
					{
						DOF.Instance.Mid = ViewMatrixHook.Instance.LookAtDirection.Length();
					}
					DOF.Instance.Near = DOF.Instance.Mid * Config.Instance.Aperture;
					DOF.Instance.Far = DOF.Instance.Mid / Config.Instance.Aperture;
				}
			}

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

		private static void DrawVector(Vector3 vector, Vector4 color, float radius = 4)
		{
			if (api.GameGui.WorldToScreen(vector, out var screenPos))
			{
				GetForegroundDrawList(GetMainViewport()).AddCircleFilled(screenPos, radius, ColorConvertFloat4ToU32(color));
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
		private Vector3? _currentEye;
		private Vector3? _currentTarget;

		private unsafe void Instance_DoCamControl(Matrix4x4* matrix, System.Numerics.Vector3* eye, System.Numerics.Vector3* target, System.Numerics.Vector3* unk)
		{
			//var eye1 = GetEaseVector(easingEye);
			//var target1 = GetEaseVector(easingTarget);

			if (_currentTarget != null)
				if (_currentEye != null)
				{
					*eye = _currentEye.Value;
					*target = _currentTarget.Value;
					*unk = Vector3.UnitY;
				}
			//*matrix = Matrix4x4.CreateLookAt(_currentTarget.Value, Vector3.UnitY);
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
}
