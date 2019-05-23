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