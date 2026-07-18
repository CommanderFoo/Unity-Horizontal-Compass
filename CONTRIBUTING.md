# Contributing to Unity Horizontal Compass

Thanks for your interest in improving this project. Contributions of all sizes are welcome: bug reports, feature ideas, documentation fixes, and pull requests.

## Getting Started

1. **Fork** the repository and clone your fork.
2. **Open the project** in Unity. It requires **Unity 6.5 or newer** (it uses the Panel Renderer component; see [Tested With](README.md#tested-with) in the README for verified versions).
3. The compass lives under `Assets/Horizontal Compass/`. The demo lives in `Assets/Scenes/Example.unity`.

## Making Changes

1. Create a branch for your work: `git checkout -b my-feature`.
2. Make your change.
3. **Test it** in the `Example.unity` scene:
   - Enter Play mode and rotate the camera to confirm the compass tracks heading.
   - Add a marker via the Inspector (or `add_marker` in script) and confirm it positions and shows distance.
   - Use **Editor Preview Heading** on the `Compass_Controller` to verify edit-mode behaviour without entering Play mode.
4. Note which Unity version(s) you tested in. This helps keep the compatibility table accurate.

## Code Style

To keep the codebase consistent, please match the existing style:

- `snake_case` for variables, fields, methods, and parameters.
- `Title_Snake_Case` for classes and structs (e.g. `Compass_Controller`).
- `SCREAMING_SNAKE_CASE` for constants.
- K&R braces (opening brace on the same line).
- Explicit types (avoid `var`).
- Tabs for indentation.
- Method parameters on a single line.

When in doubt, look at `Compass_Controller.cs` and follow the surrounding code.

## Submitting a Pull Request

1. Push your branch and open a PR against `master`.
2. Fill out the PR template.
3. Describe what you changed and which Unity version you tested in.
4. Keep PRs focused. One logical change per PR is easier to review and merge.

## Good First Contributions

New here? These are welcoming, self-contained tasks. Check the [issues](../../issues) labelled `good first issue`, or pick one of these:

- Add a version row to the **Tested With** table in the README after testing on a Unity version not yet listed.
- Configurable distance units (metres / kilometres / imperial) in `format_distance`.
- Distance-based marker fade (hide or fade markers beyond a configurable range).

## Reporting Bugs & Requesting Features

Use the issue templates. They prompt for the details needed (Unity version, repro steps, expected vs. actual).

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
