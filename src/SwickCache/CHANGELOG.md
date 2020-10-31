# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## v0.3.1
### Fixed
- Handle exceptions while deserialization cached entry

## v0.3.0
### Changed
- Make `ICacheSerializer` non-generic

## v0.2.2
### Fixed
- Separate default key builder items by `*`

## v0.2.1
### Fixed
- Handle `null` responses without failing
- Handle `ValueTask<>` responses