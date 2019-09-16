# Changelog

## 2.5.1371 - 17 September 2019
* Introduced changelog ¯\\\_(ツ)_/¯
* Implemented: support of `ParamsArrayAttribute` (**`params`** keyword)
* Fixed: object property named `async` cause a syntax error
* Fixed: `__proto__` is null after trying to set to it primitive value
* Fixed: incorrect `StackOverflowError` in multithreading scenarios
* API of resolving of modules fully reworked. Added new interface `IModuleResolver` and halper base class `CachedModuleResolverBase`

