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