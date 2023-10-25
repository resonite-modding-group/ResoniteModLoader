namespace ResoniteModLoader;

/// <summary>
/// Represents an untyped mod configuration key.
/// </summary>
public abstract class ModConfigurationKey {
	internal ModConfigurationKey(string name, string? description, bool internalAccessOnly) {
		Name = name ?? throw new ArgumentNullException("Configuration key name must not be null");
		Description = description;
		InternalAccessOnly = internalAccessOnly;
	}

	/// <summary>
	/// Gets the mod-unique name of this config item. Must be present.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the human-readable description of this config item. Should be specified by the defining mod.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Gets whether only the owning mod should have access to this config item.
	/// </summary>
	public bool InternalAccessOnly { get; private set; }

	/// <summary>
	/// Delegate for handling configuration changes.
	/// </summary>
	/// <param name="configKey">The key of the <see cref="ModConfigurationKey"/> that changed.</param>
	/// <param name="newValue">The new value of the <see cref="ModConfigurationKey"/>.</param>

	public delegate void OnChangedHandler(object? newValue);

	/// <summary>
	/// Called if this <see cref="ModConfigurationKey"/> changed. 
	/// </summary>
	public event OnChangedHandler? OnChanged;

	/// <summary>
	/// Get the <see cref="Type"/> of this key's value.
	/// </summary>
	/// <returns>The <see cref="Type"/> of this key's value.</returns>
	public abstract Type ValueType();

	/// <summary>
	/// Checks if a value is valid for this configuration item.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns><c>true</c> if the value is valid.</returns>
	public abstract bool Validate(object? value);

	/// <summary>
	/// Tries to compute the default value for this key, if a default provider was set.
	/// </summary>
	/// <param name="defaultValue">The computed default value if the return value is <c>true</c>. Otherwise <c>default</c>.</param>
	/// <returns><c>true</c> if the default value was successfully computed.</returns>
	public abstract bool TryComputeDefault(out object? defaultValue);

	/// <summary>
	/// We only care about key name for non-defining keys.<br/>
	/// For defining keys all of the other properties (default, validator, etc.) also matter.
	/// </summary>
	/// <param name="obj">The other object to compare against.</param>
	/// <returns><c>true</c> if the other object is equal to this.</returns>
	public override bool Equals(object obj) {
		return obj is ModConfigurationKey key &&
			   Name == key.Name;
	}

	/// <inheritdoc/>
	public override int GetHashCode() {
		return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
	}

	private object? Value;
	internal bool HasValue;

	/// <summary>
	/// Each configuration item has exactly ONE defining key, and that is the key defined by the mod.
	/// Duplicate keys can be created (they only need to share the same Name) and they'll still work
	/// for reading configs.
	/// <para/>
	/// This is a non-null self-reference for the defining key itself as soon as the definition is done initializing.
	/// </summary>
	internal ModConfigurationKey? DefiningKey;

	internal bool TryGetValue(out object? value) {
		if (HasValue) {
			value = Value;
			return true;
		} else {
			value = null;
			return false;
		}
	}

	internal void Set(object? value) {
		Value = value;
		HasValue = true;
		try {
			OnChanged?.SafeInvoke(value);
		} catch (Exception e) {
			Logger.ErrorInternal($"An OnChanged event subscriber for {Name} threw an exception:\n{e}");
		}
	}

	internal bool Unset() {
		bool hadValue = HasValue;
		HasValue = false;
		try {
			OnChanged?.SafeInvoke();
		} catch (Exception e) {
			Logger.ErrorInternal($"An OnChanged event subscriber for {Name} threw an exception:\n{e}");
		}
		return hadValue;
	}
}

/// <summary>
/// Represents a typed mod configuration key.
/// </summary>
/// <typeparam name="T">The type of this key's value.</typeparam>
public class ModConfigurationKey<T> : ModConfigurationKey {
	/// <summary>
	/// Creates a new instance of the <see cref="ModConfigurationKey{T}"/> class with the given parameters.
	/// </summary>
	/// <param name="name">The mod-unique name of this config item.</param>
	/// <param name="description">The human-readable description of this config item.</param>
	/// <param name="computeDefault">The function that computes a default value for this key. Otherwise <c>default(<typeparamref name="T"/>)</c> will be used.</param>
	/// <param name="internalAccessOnly">If <c>true</c>, only the owning mod should have access to this config item.</param>
	/// <param name="valueValidator">The function that checks if the given value is valid for this configuration item. Otherwise everything will be accepted.</param>
	public ModConfigurationKey(string name, string? description = null, Func<T>? computeDefault = null, bool internalAccessOnly = false, Predicate<T?>? valueValidator = null) : base(name, description, internalAccessOnly) {
		ComputeDefault = computeDefault;
		IsValueValid = valueValidator;
	}

	private readonly Func<T>? ComputeDefault;
	private readonly Predicate<T?>? IsValueValid;

	/// <inheritdoc/>
	public override Type ValueType() => typeof(T);

	/// <inheritdoc/>
	public override bool Validate(object? value) {
		if (value is T typedValue) {
			// value is of the correct type
			return ValidateTyped(typedValue);
		} else if (value == null) {
			if (Util.CanBeNull(ValueType())) {
				// null is valid for T
				return ValidateTyped((T?)value);
			} else {
				// null is not valid for T
				return false;
			}
		} else {
			// value is of the wrong type
			return false;
		}
	}

	/// <summary>
	/// Checks if a value is valid for this configuration item.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns><c>true</c> if the value is valid.</returns>
	public bool ValidateTyped(T? value) {
		if (IsValueValid == null) {
			return true;
		} else {
			return IsValueValid(value);
		}
	}

	/// <inheritdoc/>
	public override bool TryComputeDefault(out object? defaultValue) {
		if (TryComputeDefaultTyped(out T? defaultTypedValue)) {
			defaultValue = defaultTypedValue;
			return true;
		} else {
			defaultValue = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to compute the default value for this key, if a default provider was set.
	/// </summary>
	/// <param name="defaultValue">The computed default value if the return value is <c>true</c>. Otherwise <c>default(T)</c>.</param>
	/// <returns><c>true</c> if the default value was successfully computed.</returns>
	public bool TryComputeDefaultTyped(out T? defaultValue) {
		if (ComputeDefault == null) {
			defaultValue = default;
			return false;
		} else {
			defaultValue = ComputeDefault();
			return true;
		}
	}
}
