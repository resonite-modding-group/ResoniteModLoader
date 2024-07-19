using Elements.Core;
using Elements.Quantity;
using FrooxEngine;

namespace ResoniteModLoader;

public static class FeedBuilder {
	public static T Item<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : DataFeedItem
		=> Activator.CreateInstance<T>().ChainInitBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT AB
	public static DataFeedCategory Category(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedCategory>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT BA
	public static DataFeedCategory Category(string itemKey, LocaleString label, string[] subpath, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Category(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainSetOverrideSubpath(subpath);

	public static DataFeedGroup Group(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGroup>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGroup Group(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Group(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGrid>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Grid(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedEntity<E>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, E entity, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Entity<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitEntity(entity);

	public static DataFeedAction Action(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedAction>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedAction Action(string itemKey, LocaleString label, Action<SyncDelegate<Action>> setupAction, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction);

	public static DataFeedAction Action(string itemKey, LocaleString label, Action<SyncDelegate<Action>> setupAction, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction).ChainInitHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueAction<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, setupValue);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, T value, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, value);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, setupValue).ChainInitHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, T value, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, value).ChainInitHighlight(setupHighlight);

	public static DataFeedSelection Selection(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSelection>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedLabel>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, colorX color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, color color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedIndicator<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, Action<IField<T>> setup, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup, format);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, T value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value, format);
	public static DataFeedIndicator<string> StringIndicator(string itemKey, LocaleString label, object value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<string>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value.ToString(), format);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedToggle>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, Action<IField<bool>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Toggle(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> Item<DataFeedOrderedItem<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, Func<long> orderGetter, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSorting(orderGetter);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, long order, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSorting(order);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueField<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedValueField<T>, T>(setupFormatting);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedValueField<T>, T>(formatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Item<DataFeedEnum<E>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedEnum<E>, E>(setupFormatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedEnum<E>, E>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedClampedValueField<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> Item<DataFeedQuantityField<Q, T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSlider<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	// With description

	public static T Item<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : DataFeedItem
		=> Activator.CreateInstance<T>().ChainInitBase(itemKey, path, groupingParameters, label, description, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT AB
	public static DataFeedCategory Category(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedCategory>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT BA
	public static DataFeedCategory Category(string itemKey, LocaleString label, LocaleString description, string[] subpath, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Category(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainSetOverrideSubpath(subpath);

	public static DataFeedGroup Group(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGroup>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGroup Group(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Group(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGrid>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Grid(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedEntity<E>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, LocaleString description, E entity, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Entity<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitEntity(entity);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedAction>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action>> setupAction, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action>> setupAction, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction).ChainInitHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueAction<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, setupValue);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, T value, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, value);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, setupValue).ChainInitHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, T value, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitAction(setupAction, value).ChainInitHighlight(setupHighlight);

	public static DataFeedSelection Selection(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSelection>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedLabel>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, colorX color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, color color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedIndicator<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup, format);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, T value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value, format);
	public static DataFeedIndicator<string> StringIndicator(string itemKey, LocaleString label, LocaleString description, object value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<string>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value.ToString(), format);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedToggle>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, LocaleString description, Action<IField<bool>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Toggle(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> Item<DataFeedOrderedItem<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, Func<long> orderGetter, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSorting(orderGetter);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, long order, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSorting(order);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueField<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedValueField<T>, T>(setupFormatting);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedValueField<T>, T>(formatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Item<DataFeedEnum<E>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedEnum<E>, E>(setupFormatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup).ChainInitFormatting<DataFeedEnum<E>, E>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedClampedValueField<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> Item<DataFeedQuantityField<Q, T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSlider<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetup(value, min, max).ChainInitSlider(setupReferenceValue).ChainInitFormatting<DataFeedSlider<T>, T>(formatting);
}

public static class DataFeedItemChaining {
	public static I ChainInitBase<I>(this I item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where I : DataFeedItem {
		item.InitBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	public static I ChainInitBase<I>(this I item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, LocaleString description, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where I : DataFeedItem {
		item.InitBase(itemKey, path, groupingParameters, label, description, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	public static I ChainInitVisible<I>(this I item, Action<IField<bool>> setupVisible) where I : DataFeedItem {
		item.InitVisible(setupVisible);
		return item;
	}

	public static I ChainInitEnabled<I>(this I item, Action<IField<bool>> setupEnabled) where I : DataFeedItem {
		item.InitEnabled(setupEnabled);
		return item;
	}

	public static I ChainInitDescription<I>(this I item, LocaleString description) where I : DataFeedItem {
		item.InitDescription(description);
		return item;
	}

	public static I ChainInitSorting<I>(this I item, long order) where I : DataFeedItem {
		item.InitSorting(order);
		return item;
	}

	public static I ChainInitSorting<I>(this I item, Func<long> orderGetter) where I : DataFeedItem {
		item.InitSorting(orderGetter);
		return item;
	}

	public static DataFeedCategory ChainSetOverrideSubpath(this DataFeedCategory item, params string[] subpath) {
		item.SetOverrideSubpath(subpath);
		return item;
	}

	public static DataFeedEntity<E> ChainInitEntity<E>(this DataFeedEntity<E> item, E entity) {
		item.InitEntity(entity);
		return item;
	}

	public static I ChainInitSetupValue<I, T>(this I item, Action<IField<T>> setup) where I : DataFeedValueElement<T> {
		item.InitSetupValue(setup);
		return item;
	}

	public static I ChainInitFormatting<I, T>(this I item, Action<IField<string>> setupFormatting) where I : DataFeedValueElement<T> {
		item.InitFormatting(setupFormatting);
		return item;
	}

	public static I ChainInitFormatting<I, T>(this I item, string formatting) where I : DataFeedValueElement<T> {
		item.InitFormatting(formatting);
		return item;
	}

	public static DataFeedOrderedItem<T> ChainInitSetup<T>(this DataFeedOrderedItem<T> item, Action<IField<T>> orderValue, Action<IField<bool>> setupIsFirst, Action<IField<bool>> setupIsLast, Action<SyncDelegate<Action>> setupMoveUp, Action<SyncDelegate<Action>> setupMoveDown, Action<SyncDelegate<Action>> setupMakeFirst, Action<SyncDelegate<Action>> setupMakeLast, LocaleString moveUpLabel = default, LocaleString moveDownLabel = default, LocaleString makeFirstLabel = default, LocaleString makeLastLabel = default) where T : IComparable<T> {
		item.InitSetup(orderValue, setupIsFirst, setupIsLast, setupMoveUp, setupMoveDown, setupMakeFirst, setupMakeLast, moveUpLabel, moveDownLabel, makeFirstLabel, makeLastLabel);
		return item;
	}

	public static I ChainInitSetup<I, T>(this I item, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max) where I : DataFeedClampedValueField<T> {
		item.InitSetup(value, min, max);
		return item;
	}
	public static I ChainInitSetup<I, T>(this I item, Action<IField<T>> value, T min, T max) where I : DataFeedClampedValueField<T> {
		item.InitSetup(value, min, max);
		return item;
	}

	public static DataFeedQuantityField<Q, T> ChainInitUnitConfiguration<Q, T>(this DataFeedQuantityField<Q, T> item, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null) where Q : unmanaged, IQuantity<Q> {
		item.InitUnitConfiguration(defaultConfig, imperialConfig);
		return item;
	}

	public static DataFeedSlider<T> ChainInitSlider<T>(this DataFeedSlider<T> item, Action<IField<T>> setupReferenceValue) {
		item.InitSlider(setupReferenceValue);
		return item;
	}

	public static DataFeedAction ChainInitAction(this DataFeedAction item, Action<SyncDelegate<Action>> setupAction) {
		item.InitAction(setupAction);
		return item;
	}

	public static DataFeedAction ChainInitHighlight(this DataFeedAction item, Action<IField<bool>> setupHighlight) {
		item.InitHighlight(setupHighlight);
		return item;
	}

	public static DataFeedValueAction<T> ChainInitAction<T>(this DataFeedValueAction<T> item, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue) {
		item.InitAction(setupAction, setupValue);
		return item;
	}

	public static DataFeedValueAction<T> ChainInitAction<T>(this DataFeedValueAction<T> item, Action<SyncDelegate<Action<T>>> setupAction, T value) {
		item.InitAction(setupAction, value);
		return item;
	}

	public static DataFeedValueAction<T> ChainInitHighlight<T>(this DataFeedValueAction<T> item, Action<IField<bool>> setupHighlight) {
		item.InitHighlight(setupHighlight);
		return item;
	}

	public static DataFeedIndicator<T> ChainInitSetupValue<T>(this DataFeedIndicator<T> item, Action<IField<T>> setup, string format = null) {
		item.InitSetupValue(setup, format);
		return item;
	}
}
