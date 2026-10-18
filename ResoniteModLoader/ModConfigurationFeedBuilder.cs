using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections;
using System.Collections.ObjectModel;
using Elements.Data;
using FrooxEngine.UIX;
using ResoniteModLoader.Utility;

namespace ResoniteModLoader;

/// <summary>
/// A utility class that aids in the creation of mod configuration feeds.
/// </summary>
[DataModelType]
public class ModConfigurationFeedBuilder {
	private readonly ModConfiguration _config;
	private readonly Dictionary<ModConfigurationKey, FieldInfo> _keyFields = new();

	private static readonly Dictionary<ModConfiguration, ModConfigurationFeedBuilder> _cachedBuilders = new();

	/// <summary>
	/// A cache of <see cref="ModConfigurationFeedBuilder"/>, indexed by the <see cref="ModConfiguration"/> they belong to.
	/// New builders are automatically added to this cache upon instantiation, so you should try to get a cached builder before creating a new one.
	/// </summary>
	/// <example>
	/// <code>
	/// ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out var builder);
	/// builder ??= new ModConfigurationFeedBuilder(config);
	/// </code>
	/// </example>
	public static ReadOnlyDictionary<ModConfiguration, ModConfigurationFeedBuilder> CachedBuilders =>
		_cachedBuilders.AsReadOnly();

	private static bool HasAutoRegisterAttribute(FieldInfo field) =>
		field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>() is not null;

	private static bool TryGetAutoRegisterAttribute(FieldInfo field,
		[MaybeNullWhen(false)] out AutoRegisterConfigKeyAttribute attribute) {
		attribute = field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>();
		return attribute is not null;
	}

	private static bool HasRangeAttribute(FieldInfo field) => field.GetCustomAttribute<RangeAttribute>() is not null;

	private static bool TryGetRangeAttribute(FieldInfo field, [MaybeNullWhen(false)] out RangeAttribute attribute) {
		attribute = field.GetCustomAttribute<RangeAttribute>();
		return attribute is not null;
	}

	private void AssertChildKey(ModConfigurationKey key) {
		if (!_config.IsKeyDefined(key))
			throw new InvalidOperationException($"Mod key ({key}) is not owned by {_config.Owner.Name}'s config");
	}

	private static void AssertMatchingType<T>(ModConfigurationKey key) {
		if (key.ValueType() != typeof(T))
			throw new InvalidOperationException($"Type of mod key ({key}) does not match field type {typeof(T)}");
	}

	/// <summary>
	/// Instantiates and caches a new builder for a specific <see cref="ModConfiguration"/>.
	/// Check if a cached builder exists in <see cref="CachedBuilders"/> before creating a new one!
	/// </summary>
	/// <param name="config">The mod configuration this builder will generate items for</param>
	public ModConfigurationFeedBuilder(ModConfiguration config) {
		_config = config;
		IEnumerable<FieldInfo> autoConfigKeys =
			config.Owner.GetType().GetDeclaredFields().Where(HasAutoRegisterAttribute);

		foreach (FieldInfo field in autoConfigKeys) {
			ModConfigurationKey? key = (ModConfigurationKey)field.GetValue(field.IsStatic ? null : config.Owner);
			if (key is null) continue; // dunno why this would happen
			_keyFields[key] = field;
		}

		_cachedBuilders[config] = this;

		if (Logger.IsDebugEnabled()) {
			Logger.DebugInternal("--- ModConfigurationFeedBuilder instantiated ---");
			Logger.DebugInternal($"Config owner: {config.Owner.Name}");
			Logger.DebugInternal($"Total keys: {config.ConfigurationItemDefinitions.Count}");
			Logger.DebugInternal($"AutoRegistered keys: {autoConfigKeys.Count()}");
		}
	}

	// these generate methods need to be cleaned up and more strongly typed
	// todo: Make all these methods use generic keys

	/// <summary>
	/// Generates a slider for the defining key if it is a float has a range attribute, otherwise generates a generic value field.
	/// </summary>
	/// <typeparam name="T">The value type of the supplied key</typeparam>
	/// <param name="key">The key to generate the item from</param>
	/// <param name="path"><see cref="DataFeedItem.Path"/></param>
	/// <param name="groupingParameters"><see cref="DataFeedItem.GroupingParameters"/></param>
	/// <returns>A DataFeedSlider if possible, otherwise a DataFeedValueField.</returns>
	/// <seealso cref="GenerateDataFeedItem"/>
	public DataFeedValueField<T> GenerateDataFeedField<T>(ModConfigurationKey key, IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {
		AssertChildKey(key);
		AssertMatchingType<T>(key);
		if (typeof(T).IsAssignableFrom(typeof(float)) && _keyFields.TryGetValue(key, out FieldInfo field) &&
		    TryGetRangeAttribute(field, out RangeAttribute range) && range.Min is T min && range.Max is T max)
			return FeedBuilder.Slider<T>(key.Name, key.Name, key.Description,
				(field) => field.SyncWithModConfiguration(_config, key), min, max, range.TextFormat, path,
				groupingParameters);
		// If range attribute wasn't limited to floats, we could also make ClampedValueField's
		else
			return FeedBuilder.ValueField<T>(key.Name, key.Name, key.Description,
				(field) => field.SyncWithModConfiguration(_config, key), path, groupingParameters);
	}

	private static readonly MethodInfo GenerateDataFeedFieldMethod =
		typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedField))!;

	/// <summary>
	/// Generates an enum field for a specific configuration key.
	/// </summary>
	/// <typeparam name="E">The enum type of the supplied key</typeparam>
	/// <param name="key">The key to generate the item from</param>
	/// <param name="path"><see cref="DataFeedItem.Path"/></param>
	/// <param name="groupingParameters"><see cref="DataFeedItem.GroupingParameters"/></param>
	/// <returns>A physical mango if it is opposite day.</returns>
	/// <seealso cref="GenerateDataFeedItem"/>
	public DataFeedEnum<E> GenerateDataFeedEnum<E>(ModConfigurationKey key, IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) where E : Enum {
		AssertChildKey(key);
		AssertMatchingType<E>(key);
		return FeedBuilder.Enum<E>(key.Name, key.Name, key.Description,
			(field) => field.SyncWithModConfiguration(_config, key), path, groupingParameters);
	}

	private static readonly MethodInfo GenerateDataFeedEnumMethod =
		typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedEnum))!;

	/// <summary>
	/// Generates the appropriate DataFeedItem for any config key type.
	/// </summary>
	/// <param name="key">The key to generate the item from</param>
	/// <param name="path"><see cref="DataFeedItem.Path"/></param>
	/// <param name="groupingParameters"><see cref="DataFeedItem.GroupingParameters"/></param>
	/// <returns>Automatically picks the best item type for the config key type.</returns>
	public DataFeedItem GenerateDataFeedItem(ModConfigurationKey key, IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {
		AssertChildKey(key);
		Type valueType = key.ValueType();
		if (valueType == typeof(dummy))
			// Compatibility with ResoniteModSettings conventions, A lot of mods use dummy keys as labels
			return FeedBuilder.Label(key.Name, key.Description, path, groupingParameters);
		else if (valueType == typeof(bool))
			return FeedBuilder.Toggle(key.Name, key.Name, key.Description,
				(field) => field.SyncWithModConfiguration(_config, key), path, groupingParameters);
		else if (valueType != typeof(string) && valueType != typeof(Uri) &&
		         typeof(IEnumerable).IsAssignableFrom(valueType))
			return FeedBuilder.Category(key.Name, key.Name, key.Description, path, groupingParameters);
		else if (valueType.InheritsFrom(typeof(Enum)))
			return (DataFeedItem)GenerateDataFeedEnumMethod.MakeGenericMethod(key.ValueType())
				.Invoke(this, [key, path, groupingParameters]);
		else
			return (DataFeedItem)GenerateDataFeedFieldMethod.MakeGenericMethod(key.ValueType())
				.Invoke(this, [key, path, groupingParameters]);
	}

	private static readonly MethodInfo[] PrimEditorMethods =
		typeof(PrimitiveMemberEditor).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo PrimEditorEditingStarted =
		PrimEditorMethods.Single(m => m.Name == "EditingStarted");

	private static readonly MethodInfo PrimEditorEditingChanged =
		PrimEditorMethods.Single(m => m.Name == "EditingChanged");

	private static readonly MethodInfo PrimEditorEditingFinished =
		PrimEditorMethods.Single(m => m.Name == "EditingFinished");

	private DataFeedValueField<string> GenerateDataFeedStringEditorInternal<T>(ModConfigurationKey key,
		IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {

		void Setup(IField<string> field) {
			var textTarget = field.Parent as Text;
			var parentSlot = textTarget.Slot;
			var textEditorTarget =
				parentSlot.GetComponentInParents<TextEditor>(component => component.Text.Target == textTarget);
			var valueField = parentSlot.AttachComponent<ValueField<T>>();
			valueField.Value.SyncWithModConfiguration(_config, key, parentSlot);
			var primEditor = parentSlot.AttachComponent<PrimitiveMemberEditor>();
			var textEditorRef = primEditor.GetSyncMember("_textEditor") as SyncRef<TextEditor>;
			var textDriveRef = primEditor.GetSyncMember("_textDrive") as FieldDrive<string>;
			var targetRef = primEditor.GetSyncMember("_target") as RelayRef<IField>;
			textEditorRef.Target = textEditorTarget;
			textDriveRef.Target = field;
			targetRef.Target = valueField.Value;

			textEditorTarget.EditingStarted.Target =
				PrimEditorEditingStarted.CreateDelegate<Action<TextEditor>>(primEditor);
			textEditorTarget.EditingChanged.Target =
				PrimEditorEditingChanged.CreateDelegate<Action<TextEditor>>(primEditor);
			textEditorTarget.EditingFinished.Target =
				PrimEditorEditingFinished.CreateDelegate<Action<TextEditor>>(primEditor);
		}

		return FeedBuilder.ValueField<string>(key.Name, key.Name, key.Description, Setup, path, groupingParameters);
	}

	private static readonly MethodInfo GenerateDataFeedStringEditorInternalMethod =
		typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedStringEditorInternal),
			BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static bool PrimEditorSupportsType(Type type) => type.IsPrimitive || type == typeof(string) ||
	                                                         type == typeof(Uri) || type == typeof(Type) ||
	                                                         type == typeof(decimal);

	public DataFeedValueField<string> GenerateDataFeedStringEditor(ModConfigurationKey key,
		IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {
		AssertChildKey(key);
		Type valueType = key.ValueType();
		if (!PrimEditorSupportsType(valueType))
			throw new InvalidOperationException(
				$"Config key type {valueType.GetNiceName()} can't be used with GenerateDataFeedStringEditor");

		return (DataFeedValueField<string>)GenerateDataFeedStringEditorInternalMethod.MakeGenericMethod(key.ValueType())
			.Invoke(this, [key, path, groupingParameters]);
	}

	private DataFeedAction GenerateDataFeedModalEditorInternal<T>(ModConfigurationKey<T> key,
		IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {
		Type valueType = key.ValueType();

		[SyncMethod(typeof(Action))]
		void Setup(SyncDelegate<Action> field) {
			field.Target = () => {
				var slot = field.FindNearestParent<Slot>();

				var rect = slot.OpenModalOverlay(new float2(0.5f, 0.5f), key.Name);
				var ui = new UIBuilder(rect);
				ui.HorizontalFooter(72f, out var footer, out var content);
				ui.NestInto(footer);
				ui.HorizontalLayout();
				ui.Button("General.Cancel".AsLocaleKey()).LocalPressed += (button, _) =>
					button.Slot.GetComponentInParents<IUIContainer>()?.CloseContainer();
				var save = ui.Button("General.Save".AsLocaleKey());
				// save.LocalPressed += (button, data) => button.Slot.GetComponentInParents<IUIContainer>()?.CloseContainer();
				ui.NestInto(content);

				if (Coder<T>.IsEnginePrimitive) {
					var temp = rect.Slot.AttachComponent<ValueField<T>>();
					temp.Value.Value = _config.GetValue(key)!;
					ui.OverlappingLayout(0f, Alignment.MiddleCenter);
					SyncMemberEditorBuilder.Build(temp.Value, key.Name, temp.GetSyncMemberFieldInfo("Value"), ui);
				}
			};
		}

		return FeedBuilder.Action(key.Name,
			$"{key.Name} = {_config.GetValue(key).ToString() ?? MemberEditor.NULL_STRING}", key.Description, Setup,
			path, groupingParameters);
	}

	private static readonly MethodInfo GenerateDataFeedModalEditorInternalMethod =
		typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedModalEditorInternal),
			BindingFlags.Instance | BindingFlags.NonPublic)!;

	public DataFeedAction GenerateDataFeedModalEditor(ModConfigurationKey key,
		IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null) {
		AssertChildKey(key);

		return (DataFeedAction)GenerateDataFeedModalEditorInternalMethod.MakeGenericMethod(key.ValueType())
			.Invoke(this, [key, path, groupingParameters]);
	}

	public DataFeedItem GenerateDataFeedFallbackEditor(ModConfigurationKey key,
		IReadOnlyList<string>? path = null,
		IReadOnlyList<string>? groupingParameters = null)
		=> PrimEditorSupportsType(key.ValueType())
			? GenerateDataFeedStringEditor(key, path, groupingParameters)
			: GenerateDataFeedModalEditor(key, path, groupingParameters);
}

/// <summary>
/// Extensions that work with <see cref="ModConfigurationFeedBuilder"/>'s
/// </summary>
public static class ModConfigurationFeedBuilderExtensions {
	/// <summary>
	/// Returns a cached <see cref="ModConfigurationFeedBuilder"/>, or creates a new one.
	/// </summary>
	/// <param name="config">The <see cref="ModConfiguration"/> the builder belongs to</param>
	/// <returns>A cached or new builder.</returns>
	public static ModConfigurationFeedBuilder ConfigurationFeedBuilder(this ModConfiguration config) {
		ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out var builder);
		return builder ?? new ModConfigurationFeedBuilder(config);
	}
}
