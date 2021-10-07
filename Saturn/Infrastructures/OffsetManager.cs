using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Logging;

namespace Saturn.Infrastructures
{
	public static class OffsetManager
	{
		public static void Setup(SigScanner scanner)
		{
			var props = typeof(Offsets).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(i => (prop: i, Attribute: i.GetCustomAttribute<SigAttribute>())).Where(i => i.Attribute != null);

			List<Exception> exceptions = new List<Exception>(100);
			foreach ((PropertyInfo prop, SigAttribute sigAttribute) in props)
			{
				try
				{
					var sig = sigAttribute.SigString;
					sig = string.Join(' ', sig.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
						.Select(i => i == "?" ? "??" : i));

					IntPtr address;
					switch (sigAttribute)
					{
						case StaticAddressAttribute:
							address = scanner.GetStaticAddressFromSig(sig);
							address += sigAttribute.Offset;
							break;
						case FunctionAttribute:
							{
								address = scanner.ScanText(sig);
								address += sigAttribute.Offset;
								SetupDelegateForFuncAddress(prop, address, out var associateDelegate);
								break;
							}
						case OffsetAttribute:
							{
								address = scanner.ScanText(sig);
								address += sigAttribute.Offset;
								var structure = Marshal.PtrToStructure(address, prop.PropertyType);
								prop.SetValue(null, structure);
								PluginLog.Information($"[{nameof(OffsetManager)}][{prop.Name}] {prop.PropertyType.FullName} {structure}");
								continue;
							}
						default:
							throw new ArgumentOutOfRangeException();
					}

					prop.SetValue(null, address);
					PluginLog.Information($"[{nameof(OffsetManager)}][{prop?.Name}] {address.ToInt64():X}");
				}
				catch (Exception e)
				{
					PluginLog.Error(e, $"[{nameof(OffsetManager)}][{prop?.Name}] no sig found : {sigAttribute?.SigString}");
					exceptions.Add(e);
				}
			}

			if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		public static void CleanUpHooks()
		{
			typeof(Offsets).GetFields(BindingFlags.Static | BindingFlags.Public)
				.Where(i => i.FieldType.GetInterfaces().Any(j => j.FullName == "System.IDisposable")).ToList()
				.ForEach(i =>
				{
					try
					{
						PluginLog.Debug($"Disposing {i}");
						((IDisposable)i.GetValue(null))?.Dispose();
					}
					catch (Exception e)
					{
						PluginLog.Error(e,"error when disposing hook");
					}
				});

			typeof(Offsets).GetProperties(BindingFlags.Static | BindingFlags.Public)
				.Where(i => i.PropertyType.GetInterfaces().Any(j => j.FullName == "System.IDisposable")).ToList()
				.ForEach(i =>
				{
					try
					{
						PluginLog.Debug($"Disposing {i}");
						((IDisposable)i.GetValue(null))?.Dispose();
					}
					catch (Exception e)
					{
						PluginLog.Error(e, "error when disposing hook");
					}
				});
		}

		private static void SetupDelegateForFuncAddress(PropertyInfo prop, IntPtr address, out MemberInfo[] associateDelegate)
		{
			associateDelegate = typeof(Offsets).GetMember(prop.Name + "Delegate",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (associateDelegate.Any())
			{
				var memberInfo = associateDelegate.FirstOrDefault();
				MethodInfo value;
				if (memberInfo is PropertyInfo property)
				{
					property.SetValue(null,
						Marshal.GetDelegateForFunctionPointer(address, property.PropertyType));
					value = ((Delegate)property.GetValue(null)).Method;
				}
				else if (memberInfo is FieldInfo field)
				{
					field.SetValue(null, Marshal.GetDelegateForFunctionPointer(address, field.FieldType));
					value = ((Delegate)field.GetValue(null)).Method;
				}
				else
					throw new InvalidOperationException();

				PluginLog.Information(
					$"[{nameof(OffsetManager)}][{memberInfo.Name}] {value.ReturnType.Name} ({string.Join(", ", value.GetParameters().Select(i => $"{i.ParameterType.Name} {i.Name}"))})");
			}
		}


		internal abstract class SigAttribute : Attribute
		{
			protected SigAttribute(string sigString, int offset = 0)
			{
				this.SigString = sigString;
				Offset = offset;
			}

			public readonly string SigString;
			public readonly int Offset;
		}

		[AttributeUsage(AttributeTargets.Property)]
		internal sealed class StaticAddressAttribute : SigAttribute
		{
			public StaticAddressAttribute(string sigString, int offset = 0) : base(sigString, offset) { }
		}

		[AttributeUsage(AttributeTargets.Property)]
		internal class FunctionAttribute : SigAttribute
		{
			public FunctionAttribute(string sigString, int offset = 0) : base(sigString, offset) { }
		}

		[AttributeUsage(AttributeTargets.Property)]
		internal class HookFunctionAttribute : SigAttribute
		{
			public HookFunctionAttribute(string sigString, int offset = 0) : base(sigString, offset) { }
		}

		[AttributeUsage(AttributeTargets.Property)]
		internal sealed class OffsetAttribute : SigAttribute
		{
			public OffsetAttribute(string sigString, int offset) : base(sigString, offset) { }
		}
	}
}
