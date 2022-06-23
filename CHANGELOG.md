## 0.1.42 (11-03-2022):

Add health check mechanism.

## 0.1.40 (11-03-2022):

Format bytes array to base64 string.

## 0.1.39 (09-02-2022):

Wrap `<error>`, `<secret>`, `<cyclic>` into quotes.

## 0.1.38 (27-01-2022):

Use `ToString` on `ISettingsNode` instances with `ConfigurationPrinter`.

## 0.1.37 (27-12-2021):

Use `ToString` method only when `Parse`/`TryParse` is available.

## 0.1.36 (16-12-2021):

Added non-generic `Bind` version to `DefaultSettingsBinder`

## 0.1.35 (06-12-2021):

Added `net6.0` target.

## 0.1.33 (02.12.2021):

Disabled inheritance for BindByAttribute.

## 0.1.32 (12.11.2021):

Improved performance by removing a closure.

## 0.1.31 (26.06.2021):

ErrorCallbackDecorator: introduced a cooldown on exception deduplication (https://github.com/vostok/configuration/issues/43).

## 0.1.30 (23.06.2021):

Fixed bug with `Secret` attribute on classes.
Fixed bug with `BindByAttribute` and composite binders.
Fixed bug with multiple custom generic binders defined for one generic class.
Added support for `OmitConstructorsAttribute`.

## 0.1.29 (27.05.2021):

Change default MaxSourceCacheSize setting value to 500.

## 0.1.28 (07.05.2021):

Implemented https://github.com/vostok/configuration/issues/31

## 0.1.27 (05.05.2021):

Added support for ConcurrentDictionary to DictionaryBinder.

## 0.1.26 (04.05.2021):

Added constraint validator implementation based on user-provided expressions.

## 0.1.24 (26.11.2020):

Search private constructors in ConstructorBinder.

## 0.1.23 (25.11.2020):

Fixed https://github.com/vostok/configuration/issues/32

## 0.1.22 (05.11.2020):

Support parsing base64 string to byte array.

## 0.1.21 (06.10.2020):

* DynamicTypesHelper: handle arrays in custom attributes when implementing interfaces.
* TimeSpanParser, DateTimeParser and DateTimeOffsetParser are public now.

## 0.1.20 (29.09.2020):

* Fixed "Duplicate type name within an assembly" error arising when DynamicTypesHelper tries to implement the same generic interface with different type arguments.

## 0.1.19 (03.06.2020):

* ConfigurationPrinter no longer sorts property names alphabetically.

## 0.1.18 (25.05.2020):

* Added `InitialIndent` print setting for beautiful logging.

## 0.1.17 (05.05.2020):

* Fixed https://github.com/vostok/configuration/issues/21

## 0.1.16 (01.05.2020):

* ConfigurationProvider: added HasSourceFor and TrySetupSourceFor methods.

## 0.1.15 (01.05.2020):

* Default binder is now capable to bind classes with a single one-argument constructor.

## 0.1.14 (13.01.202):

* ClassStructBinder now tolerates aliases with duplicate names (comparison is case-insensitive).

## 0.1.13 (10.01.2020):

* Added support for aliases that allow to provide alternative keys for field and properties instead of their names in the model.

## 0.1.12 (14.12.2019):

* ConfigurationProvider: SetupSourceFor no longer fails even after Get or Observe if called with same source (by reference).

## 0.1.11 (14.12.2019):

* Added SecretBinder — a binder that treats all models as if they were marked with a SecretAttribute.
* ConfigurationProvider now allows to set up sources for yet unused types even if other types have been already used.

## 0.1.10 (11.12.2019)

* ConfigurationProvider: added a non-generic overload of `SetupSourceFor` method.

## 0.1.9 (09.12.2019):

* ConfigurationProvider: added new `Default` property for library developers to enable reuse of a shared static provider instance.

## 0.1.8 (13-09-2019):

* Fixed SequenceToken indentation.
* ConfigurationPrinter: added support for GUID as dictionary key.

## 0.1.7 (31-08-2019):

* Performance optimizations in binders producing up to 2x speed-up.

## 0.1.6 (19-08-2019):

* Fixed a failure occuring when validating types with nested properties of the same type, such as `DateTime`.

## 0.1.5 (23-05-2019):

* Fixed https://github.com/vostok/configuration/issues/17

## 0.1.4 (27-04-2019):

* Fixed https://github.com/vostok/configuration/issues/13
* Fixed https://github.com/vostok/configuration/issues/14
* `ConfigurationPrinter`: respect SecretAttribute applied to types.
* `ConfigurationPrinter`: allow to print settings instance without censoring secret values.
* `ConfigurationPrinter`: added support for JSON format.
* Added an API to register custom attributes to function like built-in `SecretAttribute` (see `SecurityHelper`).

## 0.1.3 (24-04-2019):

* Fixed https://github.com/vostok/configuration/issues/15

## 0.1.2 (19-03-2019):

* Added support for interfaces as settings models.
* Implemented "hot interfaces" feature: see the new `CreateHot` extension for `IConfigurationProvider`.

## 0.1.1 (15-03-2019):

Fixed possible assembly resolution issues caused by this library.

## 0.1.0 (04-03-2019): 

Initial prerelease.