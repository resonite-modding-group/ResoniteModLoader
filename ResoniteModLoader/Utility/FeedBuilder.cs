using Elements.Core;
using Elements.Quantity;
using FrooxEngine;

namespace ResoniteModLoader.Utility;

/// <summary>
/// Utility class to easily generate <see cref="DataFeedItem"/>.
/// </summary>
public static class FeedBuilder {
#pragma warning disable CS8625, CS1591, CA1715, CS8601
	public static T Item<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : DataFeedItem, new()
		=> new T().WithBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT AB
	public static DataFeedCategory Category(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedCategory>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT BA
	public static DataFeedCategory Category(string itemKey, LocaleString label, string[] subpath, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Category(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithOverrideSubpath(subpath);

	public static DataFeedGroup Group(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGroup>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGroup Group(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Group(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedResettableGroup ResettableGroup(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedResettableGroup>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedResettableGroup ResettableGroup(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> ResettableGroup(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedResettableGroup ResettableGroup(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, Action<SyncDelegate<Action>> setupResetAction, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> ResettableGroup(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithResetAction(setupResetAction);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedGrid>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedGrid Grid(string itemKey, LocaleString label, IReadOnlyList<DataFeedItem> subitems, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, object customEntity = null)
		=> Grid(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedEntity<E>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEntity<E> Entity<E>(string itemKey, LocaleString label, E entity, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Entity<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithEntity(entity);

	public static DataFeedAction Action(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedAction>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedAction Action(string itemKey, LocaleString label, Action<SyncDelegate<Action>> setupAction, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction);

	public static DataFeedAction Action(string itemKey, LocaleString label, Action<SyncDelegate<Action>> setupAction, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction).WithHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueAction<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, setupValue);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, T value, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, value);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, setupValue).WithHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, Action<SyncDelegate<Action<T>>> setupAction, T value, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, value).WithHighlight(setupHighlight);

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
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup, format);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, T value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue((field) => field.Value = value, format);
	public static DataFeedIndicator<string> StringIndicator(string itemKey, LocaleString label, object value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<string>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue((field) => field.Value = value.ToString(), format);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedToggle>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, Action<IField<bool>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Toggle(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> Item<DataFeedOrderedItem<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, Func<long> orderGetter, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSorting(orderGetter);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, long order, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSorting(order);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueField<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedValueField<T>, T>(setupFormatting);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, Action<IField<T>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedValueField<T>, T>(formatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Item<DataFeedEnum<E>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedEnum<E>, E>(setupFormatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, Action<IField<E>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedEnum<E>, E>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedClampedValueField<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> Item<DataFeedQuantityField<Q, T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSlider<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, Action<IField<T>> value, T min, T max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(formatting);

	// With description

	public static T Item<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : DataFeedItem
		=> Activator.CreateInstance<T>().WithBase(itemKey, path, groupingParameters, label, description, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT CD
	public static DataFeedCategory Category(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedCategory>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT DC
	public static DataFeedCategory Category(string itemKey, LocaleString label, LocaleString description, string[] subpath, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Category(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithOverrideSubpath(subpath);

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
		=> Entity<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithEntity(entity);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedAction>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action>> setupAction, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction);

	public static DataFeedAction Action(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action>> setupAction, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Action(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction).WithHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueAction<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, setupValue);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, T value, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, value);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, setupValue).WithHighlight(setupHighlight);

	public static DataFeedValueAction<T> ValueAction<T>(string itemKey, LocaleString label, LocaleString description, Action<SyncDelegate<Action<T>>> setupAction, T value, Action<IField<bool>> setupHighlight, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueAction<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithAction(setupAction, value).WithHighlight(setupHighlight);

	public static DataFeedSelection Selection(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSelection>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedLabel>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, colorX color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, LocaleString description, color color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<color={color.ToHexString(true)}>{label}</color>", description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedIndicator<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup, format);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, LocaleString description, T value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue((field) => field.Value = value, format);
	public static DataFeedIndicator<string> StringIndicator(string itemKey, LocaleString label, LocaleString description, object value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<string>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue((field) => field.Value = value.ToString(), format);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedToggle>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedToggle Toggle(string itemKey, LocaleString label, LocaleString description, Action<IField<bool>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Toggle(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> Item<DataFeedOrderedItem<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, Func<long> orderGetter, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSorting(orderGetter);

	public static DataFeedOrderedItem<T> OrderedItem<T>(string itemKey, LocaleString label, LocaleString description, long order, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : IComparable<T>
		=> OrderedItem<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSorting(order);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedValueField<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedValueField<T>, T>(setupFormatting);

	public static DataFeedValueField<T> ValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedValueField<T>, T>(formatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Item<DataFeedEnum<E>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedEnum<E>, E>(setupFormatting);

	public static DataFeedEnum<E> Enum<E>(string itemKey, LocaleString label, LocaleString description, Action<IField<E>> setup, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where E : Enum
		=> Enum<E>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetupValue(setup).WithFormatting<DataFeedEnum<E>, E>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedClampedValueField<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(setupFormatting);

	public static DataFeedClampedValueField<T> ClampedValueField<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> ClampedValueField<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedClampedValueField<T>, T>(formatting);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> Item<DataFeedQuantityField<Q, T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedQuantityField<Q, T> QuantityField<Q, T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where Q : unmanaged, IQuantity<Q>
		=> QuantityField<Q, T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithUnitConfiguration(defaultConfig, imperialConfig);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedSlider<T>>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(formatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, Action<IField<string>> setupFormatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(setupFormatting);

	public static DataFeedSlider<T> Slider<T>(string itemKey, LocaleString label, LocaleString description, Action<IField<T>> value, T min, T max, string formatting, Action<IField<T>> setupReferenceValue, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Slider<T>(itemKey, label, description, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).WithSetup(value, min, max).WithSlider(setupReferenceValue).WithFormatting<DataFeedSlider<T>, T>(formatting);
#pragma warning restore CS8625, CS1591, CA1715, CS8601
}

/// <summary>
/// Extends all <see cref="DataFeedItem"/> "Init" methods so they can be called in a chain (methods return original item).
/// </summary>
public static class FeedBuilderExtensions {
#pragma warning disable CS8625, CA1715
	/// <summary>Mapped to <see cref="DataFeedItem.InitBase"/></summary>
	public static I WithBase<I>(this I item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where I : DataFeedItem {
		item.InitBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitBase"/></summary>
	public static I WithBase<I>(this I item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, LocaleString description, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where I : DataFeedItem {
		item.InitBase(itemKey, path, groupingParameters, label, description, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitVisible"/></summary>
	public static I WithVisible<I>(this I item, Action<IField<bool>> setupVisible) where I : DataFeedItem {
		item.InitVisible(setupVisible);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitEnabled"/></summary>
	public static I WithEnabled<I>(this I item, Action<IField<bool>> setupEnabled) where I : DataFeedItem {
		item.InitEnabled(setupEnabled);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitDescription"/></summary>
	public static I WithDescription<I>(this I item, LocaleString description) where I : DataFeedItem {
		item.InitDescription(description);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitSorting"/></summary>
	public static I WithSorting<I>(this I item, long order) where I : DataFeedItem {
		item.InitSorting(order);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitSorting"/></summary>
	public static I WithSorting<I>(this I item, Func<long> orderGetter) where I : DataFeedItem {
		item.InitSorting(orderGetter);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedCategory.SetOverrideSubpath"/></summary>
	public static DataFeedCategory WithOverrideSubpath(this DataFeedCategory item, params string[] subpath) {
		item.SetOverrideSubpath(subpath);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedEntity.InitEntity"/></summary>
	public static DataFeedEntity<E> WithEntity<E>(this DataFeedEntity<E> item, E entity) {
		item.InitEntity(entity);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedItem.InitSetupValue"/></summary>
	public static I WithSetupValue<I, T>(this I item, Action<IField<T>> setup) where I : DataFeedValueElement<T> {
		item.InitSetupValue(setup);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedValueElement.InitFormatting"/></summary>
	public static I WithFormatting<I, T>(this I item, Action<IField<string>> setupFormatting) where I : DataFeedValueElement<T> {
		item.InitFormatting(setupFormatting);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedValueElement.InitFormatting"/></summary>
	public static I WithFormatting<I, T>(this I item, string formatting) where I : DataFeedValueElement<T> {
		item.InitFormatting(formatting);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedOrderedItem.InitSetup"/></summary>
	public static DataFeedOrderedItem<T> WithSetup<T>(this DataFeedOrderedItem<T> item, Action<IField<T>> orderValue, Action<IField<bool>> setupIsFirst, Action<IField<bool>> setupIsLast, Action<SyncDelegate<Action>> setupMoveUp, Action<SyncDelegate<Action>> setupMoveDown, Action<SyncDelegate<Action>> setupMakeFirst, Action<SyncDelegate<Action>> setupMakeLast, LocaleString moveUpLabel = default, LocaleString moveDownLabel = default, LocaleString makeFirstLabel = default, LocaleString makeLastLabel = default) where T : IComparable<T> {
		item.InitSetup(orderValue, setupIsFirst, setupIsLast, setupMoveUp, setupMoveDown, setupMakeFirst, setupMakeLast, moveUpLabel, moveDownLabel, makeFirstLabel, makeLastLabel);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedClampedValueField.InitSetup"/></summary>
	public static I WithSetup<I, T>(this I item, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max) where I : DataFeedClampedValueField<T> {
		item.InitSetup(value, min, max);
		return item;
	}
	/// <summary>Mapped to <see cref="DataFeedClampedValueField.InitSetup"/></summary>
	public static I WithSetup<I, T>(this I item, Action<IField<T>> value, T min, T max) where I : DataFeedClampedValueField<T> {
		item.InitSetup(value, min, max);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedQuantityField.InitUnitConfiguration"/></summary>
	public static DataFeedQuantityField<Q, T> WithUnitConfiguration<Q, T>(this DataFeedQuantityField<Q, T> item, UnitConfiguration defaultConfig, UnitConfiguration imperialConfig = null) where Q : unmanaged, IQuantity<Q> {
		item.InitUnitConfiguration(defaultConfig, imperialConfig);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedSlider.InitSlider"/></summary>
	public static DataFeedSlider<T> WithSlider<T>(this DataFeedSlider<T> item, Action<IField<T>> setupReferenceValue) {
		item.InitSlider(setupReferenceValue);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedAction.InitAction"/></summary>
	public static DataFeedAction WithAction(this DataFeedAction item, Action<SyncDelegate<Action>> setupAction) {
		item.InitAction(setupAction);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedAction.InitHighlight"/></summary>
	public static DataFeedAction WithHighlight(this DataFeedAction item, Action<IField<bool>> setupHighlight) {
		item.InitHighlight(setupHighlight);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedValueAction.InitAction"/></summary>
	public static DataFeedValueAction<T> WithAction<T>(this DataFeedValueAction<T> item, Action<SyncDelegate<Action<T>>> setupAction, Action<IField<T>> setupValue) {
		item.InitAction(setupAction, setupValue);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedValueAction.InitAction"/></summary>
	public static DataFeedValueAction<T> WithAction<T>(this DataFeedValueAction<T> item, Action<SyncDelegate<Action<T>>> setupAction, T value) {
		item.InitAction(setupAction, value);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedValueAction.InitHighlight"/></summary>
	public static DataFeedValueAction<T> WithHighlight<T>(this DataFeedValueAction<T> item, Action<IField<bool>> setupHighlight) {
		item.InitHighlight(setupHighlight);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedIndicator.InitSetupValue"/></summary>
	public static DataFeedIndicator<T> WithSetupValue<T>(this DataFeedIndicator<T> item, Action<IField<T>> setup, string format = null) {
		item.InitSetupValue(setup, format);
		return item;
	}

	/// <summary>Mapped to <see cref="DataFeedResettableGroup.InitResetAction"/></summary>
	public static DataFeedResettableGroup WithResetAction(this DataFeedResettableGroup item, Action<SyncDelegate<Action>> setupResetAction) {
		item.InitResetAction(setupResetAction);
		return item;
	}

	private static readonly PropertyInfo SubItemsSetter = typeof(DataFeedItem).GetProperty(nameof(DataFeedItem.SubItems))!;

	/// <summary>Appends sub-items to a <see cref="DataFeedItem"/></summary>
	public static I AddSubitems<I>(this I item, IReadOnlyList<DataFeedItem> subitems) where I : DataFeedItem {
		if (item.SubItems is null)
			SubItemsSetter.SetValue(item, subitems, null);
		else
			SubItemsSetter.SetValue(item, item.SubItems.Concat(subitems).ToList().AsReadOnly(), null);
		return item;
	}

	/// <summary>Appends sub-items to a <see cref="DataFeedItem"/></summary>
	public static I AddSubitems<I>(this I item, params DataFeedItem[] subitems) where I : DataFeedItem => item.AddSubitems(subitems.AsReadOnly());

	/// <summary>Replaces all sub-items of a <see cref="DataFeedItem"/></summary>
	public static I ReplaceSubitems<I>(this I item, IReadOnlyList<DataFeedItem> subitems) where I : DataFeedItem {
		SubItemsSetter.SetValue(item, subitems, null);
		return item;
	}

	/// <summary>Replaces all sub-items of a <see cref="DataFeedItem"/></summary>
	public static I ReplaceSubitems<I>(this I item, params DataFeedItem[] subitems) where I : DataFeedItem => item.ReplaceSubitems(subitems.AsReadOnly());

	/// <summary>Removes all sub-items from a <see cref="DataFeedItem"/></summary>
	public static I ClearSubitems<I>(this I item) where I : DataFeedItem {
		SubItemsSetter.SetValue(item, null, null);
		return item;
	}
#pragma warning restore CS8625, CA1715
}
