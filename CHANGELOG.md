# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## Unreleased
## Added
## Fixed

## 7.25.3601
## Added

- ``mapserver.config``: Added support for ``CriticalErrorLevel``
  [Online Documentation](https://docs.gviewonline.com/en/setup/config-server.html)

## Fixed

- Bug: Publish services from gView.Carto to gView.Server failed (400: Badrequest)

## 7.25.3301
## Added

- Introduced additional System-Rotation-Types: (geographic - 0 => north, arithmetic - 0 => east)

## 7.25.3201

## Fixed

- Manage Map Datasets Dialog Width: [Issue #40](https://github.com/jugstalt/gview-gis/issues/40)]

## 7.25.1901

### Added

```
**************************************************************
*                                                            *
*                    !! Upgrade to NET 9 !!                  *
*                                                            *
**************************************************************
```

-   Installation of dotnet-9.x-hosting-bundle required on windows servers.

-   **Label Renderer:** Expression supports formatting for `number` and `date` types.
    `[FIELD:0.00]` or `[FIELD:yyyy-MM-dd]` will format the field value.
    [Online Documentation](https://docs.gviewonline.com/en/webapps/carto/labeling.html#simple-text-renderer)

-   **Layer Query Definintion:** supports `Order By`.
    You can now define the order of the features during the rendering by using the `orderBy` property in the layer properties dialog (`Filter`)
    [Online Documentation](https://docs.gviewonline.com/en/webapps/carto/layersettings.html)

-   **Projection:** Parallel coordinate transformations on the fly when using `proj-engine=ManageProj4Parallel`
    [Online Documentation](https://docs.gviewonline.com/en/setup/config-server.html)

-   **Projection:** Intoduced gridshifts for transforming to WGS84.
    (still in beta)

