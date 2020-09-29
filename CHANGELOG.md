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