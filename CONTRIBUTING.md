
# Contributing to MetaLite

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

The following is a set of guidelines for contributing to MetaLite and its packages, which are hosted in the [JelloXBiootechInc](https://github.com/JelloXBiotechInc) on GitHub. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

#### Table Of Contents

[Code of Conduct](#code-of-conduct)

[I don't want to read this whole thing, I just have a question!!!](#i-dont-want-to-read-this-whole-thing-i-just-have-a-question)

[What should I know before I get started?](#what-should-i-know-before-i-get-started)
  * [MetaLite and Packages](#metalite-and-packages)

[How Can I Contribute?](#how-can-i-contribute)
  * [Reporting Bugs](#reporting-bugs)
  * [Your First Code Contribution](#your-first-code-contribution)
  * [Pull Requests](#pull-requests)

[Styleguides](#styleguides)
  * [Git Commit Messages](#git-commit-messages)

[Additional Notes](#additional-notes)
  * [Issue and Pull Request Labels](#issue-and-pull-request-labels)

## Code of Conduct

This project and everyone participating in it is governed by the [MetaLite Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [changsy@jellox.com](mailto:changsy@jellox.com).

## I don't want to read this whole thing I just have a question!!!

> **Note:** Please don't file an issue to ask a question. You'll get faster results by using the resources below.

We have an official message board with a detailed FAQ and where the community chimes in with helpful advice if you have questions.

* [Github Discussions, the official MetaLite](https://github.com/JelloXBiotechInc/MetaLite/discussions)
* [MetaLite FAQ](https://github.com/JelloXBiotechInc/MetaLite/discussions/categories/q-a)

## What should I know before I get started?

### MetaLite and Packages

MetaLite is a open source project. When you initially consider contributing to MetaLite, you might be unsure about which of those package implements the functionality you want to change or report a bug for. This section should help you with that.

MetaLite is develop on Microsoft .NET framework 4.7.2, and Microsoft Visual Studio Community 2019. For the early stage, we put the package as sub-project in one repository for for our maintain ability. But eventually, we will separate those package to independent repositories.

If the needed packages is not in the repository, you should find is in Nuget package management with the exactly version.

Here's a list of the packages:

 - [OpenSlideNET](https://github.com/yigolden/OpenSlideNET) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/yigolden/OpenSlideNET/blob/master/LICENSE) Interop with the compiled OpenSlide ```.dll```s in C# code. 
 - [OpenSlide](https://github.com/openslide/openslide) [![license](https://img.shields.io/badge/license-lgpl_2.1-blue.svg "lgpl_2_1")](https://github.com/openslide/openslide/blob/main/LICENSE.txt) OpenSlide ```.dll```s compiled for windows OS from C code. You can find pre-compiled [Windows Binaries](https://openslide.org/download/) in OpenSilde official website.
 - [WPFPixelShaderLibrary](https://github.com/Unknown6656/WPFPixelShaderLibrary/tree/master) [![license](https://img.shields.io/badge/license-GPL-blue.svg "GPL")](https://github.com/Unknown6656/WPFPixelShaderLibrary/blob/master/LICENSE) For psuedo staining function. Use [HLSL](https://docs.microsoft.com/zh-tw/windows/win32/direct3dhlsl/dx-graphics-hlsl) to modify UIElement runtime rendering.
 - [ColorPickerWPF](https://github.com/drogoganor/ColorPickerWPF) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/drogoganor/ColorPickerWPF/blob/master/LICENSE) Customized the color picker for MetaLite.
 - [OpenCvSharp](https://github.com/shimat/opencvsharp) [![license](https://img.shields.io/badge/license-Apache_2.0-blue.svg "Apache 2.0")](https://github.com/shimat/opencvsharp/blob/master/LICENSE) [![OpenCvSharp](https://img.shields.io/badge/OpenCvSharp-4.5.3.20210817-purple.svg "Fellow Oak DICOM")](https://github.com/shimat/opencvsharp) Image processing.
 - [Fellow Oak DICOM](https://github.com/fo-dicom/fo-dicom) [![license](https://img.shields.io/badge/license-MS--PL-blue.svg "MS-PL")](https://github.com/fo-dicom/fo-dicom/blob/development/License.txt) [![fo-dicom](https://img.shields.io/badge/Fellow_Oak_DICOM-4.0.8-orange.svg "Fellow Oak DICOM")](https://github.com/fo-dicom/fo-dicom) Wrapping DICOM reading mathod into OpenSlideNET API.
 - [ImageSharp](https://github.com/SixLabors/ImageSharp) [![license](https://img.shields.io/badge/license-Apache_2.0-blue.svg "Apache 2.0")](https://github.com/SixLabors/ImageSharp/blob/master/LICENSE)
 - [PooledGrowableBufferHelper](https://github.com/yigolden/PooledGrowableBufferHelper) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/yigolden/PooledGrowableBufferHelper/blob/master/LICENSE)
 - [LiveCharts](https://github.com/Live-Charts/Live-Charts) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/Live-Charts/Live-Charts/blob/master/LICENSE.TXT) [![license](https://img.shields.io/badge/LiveCharts-0.9.7-00ccff.svg "Live-Charts")](https://github.com/Live-Charts/Live-Charts) For statistic chart.
 - [PdfSharp](https://www.nuget.org/packages/PdfSharp.Xps.SimpleWPFReporting/1.0.0/ReportAbuse) For diagnosis report function.
 - [SimpleWPFReporting](https://github.com/maximcus/SimpleWPFReporting) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/maximcus/SimpleWPFReporting/blob/master/LICENSE.md) For diagnosis report function.
 - [MahApps](https://github.com/MahApps/MahApps.Metro) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/MahApps/MahApps.Metro/blob/develop/LICENSE) [![license](https://img.shields.io/badge/MahApps-2.1.1-blue.svg "MahApps")](https://github.com/MahApps/MahApps.Metro) For UI.
 - [Html Agility Pack](https://github.com/zzzprojects/html-agility-pack) [![license](https://img.shields.io/badge/license-MIT-brightgreen.svg "MIT")](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE) [![license](https://img.shields.io/badge/Html_Agility_Pack-1.11.24-00ccff.svg "Html Agility Pack")](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE) For embeded web browser html manipulate.

## How Can I Contribute?

### Reporting Bugs

This section guides you through submitting a bug report for MetaLite. Following these guidelines helps maintainers and the community understand your report :pencil:, reproduce the behavior :computer: :computer:, and find related reports :mag_right:.

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

### Your First Code Contribution

Unsure where to begin contributing to MetaLite? You can start by looking through these `beginner` and `help-wanted` issues:

* [Beginner issues][beginner] - issues which should only require a few lines of code, and a test or two.
* [Help wanted issues][help-wanted] - issues which should be a bit more involved than `beginner` issues.

Both issue lists are sorted by total number of comments. While not perfect, number of comments is a reasonable proxy for impact a given change will have.

### Pull Requests

The process described here has several goals:

- Maintain MetaLite's quality
- Fix problems that are important to users
- Engage the community in working toward the best possible MetaLite
- Enable a sustainable system for MetaLite's maintainers to review contributions

Please follow these steps to have your contribution considered by the maintainers:

1. Follow all instructions in [the template](PULL_REQUEST_TEMPLATE.md)
2. Follow the [styleguides](#styleguides)
3. After you submit your pull request, verify that all [status checks](https://help.github.com/articles/about-status-checks/) are passing <details><summary>What if the status checks are failing?</summary>If a status check is failing, and you believe that the failure is unrelated to your change, please leave a comment on the pull request explaining why you believe the failure is unrelated. A maintainer will re-run the status check for you. If we conclude that the failure was a false positive, then we will open an issue to track that problem with our status check suite.</details>

While the prerequisites above must be satisfied prior to having your pull request reviewed, the reviewer(s) may ask you to complete additional design work, tests, or other changes before your pull request can be ultimately accepted.

## Styleguides

### Git Commit Messages

* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line
* When only changing documentation, include `[ci skip]` in the commit title
* Consider starting the commit message with an applicable emoji:
    * :art: `:art:` when improving the format/structure of the code
    * :racehorse: `:racehorse:` when improving performance
    * :non-potable_water: `:non-potable_water:` when plugging memory leaks
    * :memo: `:memo:` when writing docs
    * :penguin: `:penguin:` when fixing something on Linux
    * :apple: `:apple:` when fixing something on macOS
    * :checkered_flag: `:checkered_flag:` when fixing something on Windows
    * :bug: `:bug:` when fixing a bug
    * :fire: `:fire:` when removing code or files
    * :green_heart: `:green_heart:` when fixing the CI build
    * :white_check_mark: `:white_check_mark:` when adding tests
    * :lock: `:lock:` when dealing with security
    * :arrow_up: `:arrow_up:` when upgrading dependencies
    * :arrow_down: `:arrow_down:` when downgrading dependencies
    * :shirt: `:shirt:` when removing linter warnings

## Additional Notes

### Issue and Pull Request Labels

This section lists the labels we use to help us track and manage issues and pull requests. Most labels are used across all MetaLite repositories, but some are specific to `JelloXBiotechInc/MetaLite`.

[GitHub search](https://help.github.com/articles/searching-issues/) makes it easy to use labels for finding groups of issues or pull requests you're interested in. For example, you might be interested in [open issues across `JelloXBiotechInc/MetaLite` and all JelloXBiotechInc-owned packages which are labeled as bugs, but still need to be reliably reproduced](https://github.com/search?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Abug+label%3Aneeds-reproduction) or perhaps [open pull requests in `JelloXBiotechInc/MetaLite` which haven't been reviewed yet](https://github.com/search?utf8=%E2%9C%93&q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+comments%3A0). To help you find issues and pull requests, each label is listed with search links for finding open items with that label in `JelloXBiotechInc/MetaLite` only and also across all MetaLite repositories. We  encourage you to read about [other search filters](https://help.github.com/articles/searching-issues/) which will help you write more focused queries.

The labels are loosely grouped by their purpose, but it's not required that every issue has a label from every group or that an issue can't have more than one label from the same group.

Please open an issue on `JelloXBiotechInc/MetaLite` if you have suggestions for new labels, and if you notice some labels are missing on some repositories, then please open an issue on that repository.

#### Type of Issue and Issue State

| Label name | `JelloXBiotechInc/MetaLite` :mag_right: | `JelloXBiotechInc`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `enhancement` | [search][search-MetaLite-repo-label-enhancement] | [search][search-JelloXBiotechInc-org-label-enhancement] | Feature requests. |
| `bug` | [search][search-MetaLite-repo-label-bug] | [search][search-JelloXBiotechInc-org-label-bug] | Confirmed bugs or reports that are very likely to be bugs. |
| `question` | [search][search-MetaLite-repo-label-question] | [search][search-JelloXBiotechInc-org-label-question] | Questions more than bug reports or feature requests (e.g. how do I do X). |
| `feedback` | [search][search-MetaLite-repo-label-feedback] | [search][search-JelloXBiotechInc-org-label-feedback] | General feedback more than bug reports or feature requests. |
| `help-wanted` | [search][search-MetaLite-repo-label-help-wanted] | [search][search-JelloXBiotechInc-org-label-help-wanted] | The MetaLite core team would appreciate help from the community in resolving these issues. |
| `beginner` | [search][search-MetaLite-repo-label-beginner] | [search][search-JelloXBiotechInc-org-label-beginner] | Less complex issues which would be good first issues to work on for users who want to contribute to MetaLite. |
| `more-information-needed` | [search][search-MetaLite-repo-label-more-information-needed] | [search][search-JelloXBiotechInc-org-label-more-information-needed] | More information needs to be collected about these problems or feature requests (e.g. steps to reproduce). |
| `needs-reproduction` | [search][search-MetaLite-repo-label-needs-reproduction] | [search][search-JelloXBiotechInc-org-label-needs-reproduction] | Likely bugs, but haven't been reliably reproduced. |
| `blocked` | [search][search-MetaLite-repo-label-blocked] | [search][search-JelloXBiotechInc-org-label-blocked] | Issues blocked on other issues. |
| `duplicate` | [search][search-MetaLite-repo-label-duplicate] | [search][search-JelloXBiotechInc-org-label-duplicate] | Issues which are duplicates of other issues, i.e. they have been reported before. |
| `wontfix` | [search][search-MetaLite-repo-label-wontfix] | [search][search-JelloXBiotechInc-org-label-wontfix] | The MetaLite core team has decided not to fix these issues for now, either because they're working as intended or for some other reason. |
| `invalid` | [search][search-MetaLite-repo-label-invalid] | [search][search-JelloXBiotechInc-org-label-invalid] | Issues which aren't valid (e.g. user errors). |
| `package-idea` | [search][search-MetaLite-repo-label-package-idea] | [search][search-JelloXBiotechInc-org-label-package-idea] | Feature request which might be good candidates for new packages, instead of extending MetaLite or core MetaLite packages. |

#### Topic Categories

| Label name | `JelloXBiotechInc/MetaLite` :mag_right: | `JelloXBiotechInc`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `windows` | [search][search-MetaLite-repo-label-windows] | [search][search-JelloXBiotechInc-org-label-windows] | Related to MetaLite running on Windows. |
| `documentation` | [search][search-MetaLite-repo-label-documentation] | [search][search-JelloXBiotechInc-org-label-documentation] | Related to any type of documentation. |
| `performance` | [search][search-MetaLite-repo-label-performance] | [search][search-JelloXBiotechInc-org-label-performance] | Related to performance. |
| `security` | [search][search-MetaLite-repo-label-security] | [search][search-JelloXBiotechInc-org-label-security] | Related to security. |
| `ui` | [search][search-MetaLite-repo-label-ui] | [search][search-JelloXBiotechInc-org-label-ui] | Related to visual design. |
| `api` | [search][search-MetaLite-repo-label-api] | [search][search-JelloXBiotechInc-org-label-api] | Related to MetaLite's public APIs. |
| `uncaught-exception` | [search][search-MetaLite-repo-label-uncaught-exception] | [search][search-JelloXBiotechInc-org-label-uncaught-exception] | Issues about uncaught exceptions, normally created from the Notifications package. |
| `crash` | [search][search-MetaLite-repo-label-crash] | [search][search-JelloXBiotechInc-org-label-crash] | Reports of MetaLite completely crashing. |
| `auto-indent` | [search][search-MetaLite-repo-label-auto-indent] | [search][search-JelloXBiotechInc-org-label-auto-indent] | Related to auto-indenting text. |
| `encoding` | [search][search-MetaLite-repo-label-encoding] | [search][search-JelloXBiotechInc-org-label-encoding] | Related to character encoding. |
| `network` | [search][search-MetaLite-repo-label-network] | [search][search-JelloXBiotechInc-org-label-network] | Related to network problems or working with remote files (e.g. on network drives). |
| `git` | [search][search-MetaLite-repo-label-git] | [search][search-JelloXBiotechInc-org-label-git] | Related to Git functionality (e.g. problems with gitignore files or with showing the correct file status). |

#### `JelloXBiotechInc/MetaLite` Topic Categories

| Label name | `JelloXBiotechInc/MetaLite` :mag_right: | `JelloXBiotechInc`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `editor-rendering` | [search][search-MetaLite-repo-label-editor-rendering] | [search][search-JelloXBiotechInc-org-label-editor-rendering] | Related to language-independent aspects of rendering text (e.g. scrolling, soft wrap, and font rendering). |
| `build-error` | [search][search-MetaLite-repo-label-build-error] | [search][search-JelloXBiotechInc-org-label-build-error] | Related to problems with building MetaLite from source. |
| `error-from-pathwatcher` | [search][search-MetaLite-repo-label-error-from-pathwatcher] | [search][search-JelloXBiotechInc-org-label-error-from-pathwatcher] | Related to errors thrown by the pathwatcher library. |
| `error-from-save` | [search][search-MetaLite-repo-label-error-from-save] | [search][search-JelloXBiotechInc-org-label-error-from-save] | Related to errors thrown when saving files. |
| `error-from-open` | [search][search-MetaLite-repo-label-error-from-open] | [search][search-JelloXBiotechInc-org-label-error-from-open] | Related to errors thrown when opening files. |
| `deprecation-help` | [search][search-MetaLite-repo-label-deprecation-help] | [search][search-JelloXBiotechInc-org-label-deprecation-help] | Issues for helping package authors remove usage of deprecated APIs in packages. |

#### Pull Request Labels

| Label name | `JelloXBiotechInc/MetaLite` :mag_right: | `JelloXBiotechInc`‑org :mag_right: | Description
| --- | --- | --- | --- |
| `work-in-progress` | [search][search-MetaLite-repo-label-work-in-progress] | [search][search-JelloXBiotechInc-org-label-work-in-progress] | Pull requests which are still being worked on, more changes will follow. |
| `needs-review` | [search][search-MetaLite-repo-label-needs-review] | [search][search-JelloXBiotechInc-org-label-needs-review] | Pull requests which need code review, and approval from maintainers or MetaLite core team. |
| `under-review` | [search][search-MetaLite-repo-label-under-review] | [search][search-JelloXBiotechInc-org-label-under-review] | Pull requests being reviewed by maintainers or MetaLite core team. |
| `requires-changes` | [search][search-MetaLite-repo-label-requires-changes] | [search][search-JelloXBiotechInc-org-label-requires-changes] | Pull requests which need to be updated based on review comments and then reviewed again. |
| `needs-testing` | [search][search-MetaLite-repo-label-needs-testing] | [search][search-JelloXBiotechInc-org-label-needs-testing] | Pull requests which need manual testing. |

[search-MetaLite-repo-label-enhancement]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aenhancement
[search-JelloXBiotechInc-org-label-enhancement]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aenhancement
[search-MetaLite-repo-label-bug]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Abug
[search-JelloXBiotechInc-org-label-bug]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Abug
[search-MetaLite-repo-label-question]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aquestion
[search-JelloXBiotechInc-org-label-question]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aquestion
[search-MetaLite-repo-label-feedback]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Afeedback
[search-JelloXBiotechInc-org-label-feedback]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Afeedback
[search-MetaLite-repo-label-help-wanted]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Ahelp-wanted
[search-JelloXBiotechInc-org-label-help-wanted]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Ahelp-wanted
[search-MetaLite-repo-label-beginner]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Abeginner
[search-JelloXBiotechInc-org-label-beginner]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Abeginner
[search-MetaLite-repo-label-more-information-needed]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Amore-information-needed
[search-JelloXBiotechInc-org-label-more-information-needed]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Amore-information-needed
[search-MetaLite-repo-label-needs-reproduction]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aneeds-reproduction
[search-JelloXBiotechInc-org-label-needs-reproduction]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aneeds-reproduction
[search-MetaLite-repo-label-triage-help-needed]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Atriage-help-needed
[search-JelloXBiotechInc-org-label-triage-help-needed]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Atriage-help-needed
[search-MetaLite-repo-label-windows]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Awindows
[search-JelloXBiotechInc-org-label-windows]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Awindows
[search-MetaLite-repo-label-linux]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Alinux
[search-JelloXBiotechInc-org-label-linux]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Alinux
[search-MetaLite-repo-label-mac]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Amac
[search-JelloXBiotechInc-org-label-mac]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Amac
[search-MetaLite-repo-label-documentation]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Adocumentation
[search-JelloXBiotechInc-org-label-documentation]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Adocumentation
[search-MetaLite-repo-label-performance]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aperformance
[search-JelloXBiotechInc-org-label-performance]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aperformance
[search-MetaLite-repo-label-security]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Asecurity
[search-JelloXBiotechInc-org-label-security]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Asecurity
[search-MetaLite-repo-label-ui]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aui
[search-JelloXBiotechInc-org-label-ui]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aui
[search-MetaLite-repo-label-api]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aapi
[search-JelloXBiotechInc-org-label-api]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aapi
[search-MetaLite-repo-label-crash]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Acrash
[search-JelloXBiotechInc-org-label-crash]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Acrash
[search-MetaLite-repo-label-auto-indent]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aauto-indent
[search-JelloXBiotechInc-org-label-auto-indent]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aauto-indent
[search-MetaLite-repo-label-encoding]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aencoding
[search-JelloXBiotechInc-org-label-encoding]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aencoding
[search-MetaLite-repo-label-network]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Anetwork
[search-JelloXBiotechInc-org-label-network]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Anetwork
[search-MetaLite-repo-label-uncaught-exception]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Auncaught-exception
[search-JelloXBiotechInc-org-label-uncaught-exception]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Auncaught-exception
[search-MetaLite-repo-label-git]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Agit
[search-JelloXBiotechInc-org-label-git]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Agit
[search-MetaLite-repo-label-blocked]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Ablocked
[search-JelloXBiotechInc-org-label-blocked]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Ablocked
[search-MetaLite-repo-label-duplicate]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aduplicate
[search-JelloXBiotechInc-org-label-duplicate]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aduplicate
[search-MetaLite-repo-label-wontfix]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Awontfix
[search-JelloXBiotechInc-org-label-wontfix]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Awontfix
[search-MetaLite-repo-label-invalid]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Ainvalid
[search-JelloXBiotechInc-org-label-invalid]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Ainvalid
[search-MetaLite-repo-label-package-idea]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Apackage-idea
[search-JelloXBiotechInc-org-label-package-idea]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Apackage-idea
[search-MetaLite-repo-label-wrong-repo]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Awrong-repo
[search-JelloXBiotechInc-org-label-wrong-repo]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Awrong-repo
[search-MetaLite-repo-label-editor-rendering]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aeditor-rendering
[search-JelloXBiotechInc-org-label-editor-rendering]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aeditor-rendering
[search-MetaLite-repo-label-build-error]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Abuild-error
[search-JelloXBiotechInc-org-label-build-error]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Abuild-error
[search-MetaLite-repo-label-error-from-pathwatcher]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aerror-from-pathwatcher
[search-JelloXBiotechInc-org-label-error-from-pathwatcher]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aerror-from-pathwatcher
[search-MetaLite-repo-label-error-from-save]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aerror-from-save
[search-JelloXBiotechInc-org-label-error-from-save]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aerror-from-save
[search-MetaLite-repo-label-error-from-open]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aerror-from-open
[search-JelloXBiotechInc-org-label-error-from-open]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aerror-from-open
[search-MetaLite-repo-label-installer]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Ainstaller
[search-JelloXBiotechInc-org-label-installer]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Ainstaller
[search-MetaLite-repo-label-auto-updater]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aauto-updater
[search-JelloXBiotechInc-org-label-auto-updater]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aauto-updater
[search-MetaLite-repo-label-deprecation-help]: https://github.com/search?q=is%3Aopen+is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+label%3Adeprecation-help
[search-JelloXBiotechInc-org-label-deprecation-help]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Adeprecation-help
[search-MetaLite-repo-label-electron]: https://github.com/search?q=is%3Aissue+repo%3AJelloXBiotechInc%2FMetaLite+is%3Aopen+label%3Aelectron
[search-JelloXBiotechInc-org-label-electron]: https://github.com/search?q=is%3Aopen+is%3Aissue+user%3AJelloXBiotechInc+label%3Aelectron
[search-MetaLite-repo-label-work-in-progress]: https://github.com/search?q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+label%3Awork-in-progress
[search-JelloXBiotechInc-org-label-work-in-progress]: https://github.com/search?q=is%3Aopen+is%3Apr+user%3AJelloXBiotechInc+label%3Awork-in-progress
[search-MetaLite-repo-label-needs-review]: https://github.com/search?q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aneeds-review
[search-JelloXBiotechInc-org-label-needs-review]: https://github.com/search?q=is%3Aopen+is%3Apr+user%3AJelloXBiotechInc+label%3Aneeds-review
[search-MetaLite-repo-label-under-review]: https://github.com/search?q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aunder-review
[search-JelloXBiotechInc-org-label-under-review]: https://github.com/search?q=is%3Aopen+is%3Apr+user%3AJelloXBiotechInc+label%3Aunder-review
[search-MetaLite-repo-label-requires-changes]: https://github.com/search?q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+label%3Arequires-changes
[search-JelloXBiotechInc-org-label-requires-changes]: https://github.com/search?q=is%3Aopen+is%3Apr+user%3AJelloXBiotechInc+label%3Arequires-changes
[search-MetaLite-repo-label-needs-testing]: https://github.com/search?q=is%3Aopen+is%3Apr+repo%3AJelloXBiotechInc%2FMetaLite+label%3Aneeds-testing
[search-JelloXBiotechInc-org-label-needs-testing]: https://github.com/search?q=is%3Aopen+is%3Apr+user%3AJelloXBiotechInc+label%3Aneeds-testing

[beginner]:https://github.com/search?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue+label%3Abeginner+label%3Ahelp-wanted+user%3AJelloXBiotechInc+sort%3Acomments-desc
[help-wanted]:https://github.com/search?q=is%3Aopen+is%3Aissue+label%3Ahelp-wanted+user%3AJelloXBiotechInc+sort%3Acomments-desc+-label%3Abeginner
