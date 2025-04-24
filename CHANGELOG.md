# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## Unreleased

### Added

- **Label Renderer:** Expression supports formatting for `number` and `date` types.
  `[FIELD:0.00]` or `[FIELD:yyyy-MM-dd]` will format the field value. 

- **Layer Query Definintion:** supports `Order By`. 
  You can now define the order of the features during the rendering by using the `orderBy` property in the layer properties dialog (`Filter`)

## Fixed