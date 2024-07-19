using Elements.Core;
using Elements.Quantity;
using FrooxEngine;

namespace ResoniteModLoader;

public sealed class FeedBuilder {
	public static T Item<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) where T : DataFeedItem {
		T item = Activator.CreateInstance<T>();
		item.InitBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	// CONFLICT AA
	public static DataFeedCategory Category(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedCategory>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	// CONFLICT AB
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
		=> Label(itemKey, $"<c={color.ToHexString(true)}>{label}</c>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedLabel Label(string itemKey, LocaleString label, color color, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Label(itemKey, $"<c={color.ToHexString(true)}>{label}</c>", path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Item<DataFeedIndicator<T>>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, Action<IField<T>> setup, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue(setup, format);

	public static DataFeedIndicator<T> Indicator<T>(string itemKey, LocaleString label, T value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<T>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value, format);
	public static DataFeedIndicator<string> StringIndicator(string itemKey, LocaleString label, object value, string format = null, IReadOnlyList<string> path = null, IReadOnlyList<string> groupingParameters = null, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null)
		=> Indicator<string>(itemKey, label, path, groupingParameters, icon, setupVisible, setupEnabled, subitems, customEntity).ChainInitSetupValue((field) => field.Value = value.ToString(), format);

}

public static class DataFeedItemChaining {
	public static DataFeedItem ChainInitBase(this DataFeedItem item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) {
		item.InitBase(itemKey, path, groupingParameters, label, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	public static DataFeedItem ChainInitBase(this DataFeedItem item, string itemKey, IReadOnlyList<string> path, IReadOnlyList<string> groupingParameters, LocaleString label, LocaleString description, Uri icon = null, Action<IField<bool>> setupVisible = null, Action<IField<bool>> setupEnabled = null, IReadOnlyList<DataFeedItem> subitems = null, object customEntity = null) {
		item.InitBase(itemKey, path, groupingParameters, label, description, icon, setupVisible, setupEnabled, subitems, customEntity);
		return item;
	}

	public static DataFeedItem ChainInitVisible(this DataFeedItem item, Action<IField<bool>> setupVisible) {
		item.InitVisible(setupVisible);
		return item;
	}

	public static DataFeedItem ChainInitEnabled(this DataFeedItem item, Action<IField<bool>> setupEnabled) {
		item.InitEnabled(setupEnabled);
		return item;
	}

	public static DataFeedItem ChainInitDescription(this DataFeedItem item, LocaleString description) {
		item.InitDescription(description);
		return item;
	}

	public static DataFeedItem ChainInitSorting(this DataFeedItem item, long order) {
		item.InitSorting(order);
		return item;
	}

	public static DataFeedItem ChainInitSorting(this DataFeedItem item, Func<long> orderGetter) {
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

	public static DataFeedValueElement<T> ChainInitSetupValue<T>(this DataFeedValueElement<T> item, Action<IField<T>> setup) {
		item.InitSetupValue(setup);
		return item;
	}

	public static DataFeedValueElement<T> ChainInitFormatting<T>(this DataFeedValueElement<T> item, Action<IField<string>> setupFormatting) {
		item.InitFormatting(setupFormatting);
		return item;
	}

	public static DataFeedValueElement<T> ChainInitFormatting<T>(this DataFeedValueElement<T> item, string formatting) {
		item.InitFormatting(formatting);
		return item;
	}

	public static DataFeedOrderedItem<T> ChainInitSetup<T>(this DataFeedOrderedItem<T> item, Action<IField<T>> orderValue, Action<IField<bool>> setupIsFirst, Action<IField<bool>> setupIsLast, Action<SyncDelegate<Action>> setupMoveUp, Action<SyncDelegate<Action>> setupMoveDown, Action<SyncDelegate<Action>> setupMakeFirst, Action<SyncDelegate<Action>> setupMakeLast, LocaleString moveUpLabel = default, LocaleString moveDownLabel = default, LocaleString makeFirstLabel = default, LocaleString makeLastLabel = default) where T : IComparable<T> {
		item.InitSetup(orderValue, setupIsFirst, setupIsLast, setupMoveUp, setupMoveDown, setupMakeFirst, setupMakeLast, moveUpLabel, moveDownLabel, makeFirstLabel, makeLastLabel);
		return item;
	}

	public static DataFeedClampedValueField<T> ChainInitSetup<T>(this DataFeedClampedValueField<T> item, Action<IField<T>> value, Action<IField<T>> min, Action<IField<T>> max) {
		item.InitSetup(value, min, max);
		return item;
	}
	public static DataFeedClampedValueField<T> ChainInitSetup<T>(this DataFeedClampedValueField<T> item, Action<IField<T>> value, T min, T max) {
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
