# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## Unreleased

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

## Fixed
