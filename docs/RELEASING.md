# Releasing Isekai Hero

Releases are intentionally started by hand from GitHub Actions. A merge to
`main` validates the metadata but never publishes a public build. This gives a
solo developer one deliberate release button, while GitHub builds and packages
the mod entirely in the cloud.

## One-time setup

Cloud builds use the
[Book.StS2.RefLib](https://www.nuget.org/packages/Book.StS2.RefLib) package. It
contains code-stripped reference assemblies published with Mega Crit's
permission, giving the compiler the type signatures it needs without copying
the game into GitHub Actions. Local builds continue using your installed game
assemblies automatically.

1. Add the GitHub Actions repository variable `NEXUSMODS_FILE_ID`. This is the
   ID of the existing Nexus file that receives new versions. On the public mod
   page, open **Files > API Info**, or use the edit menu on **Manage Files**.
   The mod page must already have one manually uploaded file.
2. Create a personal Nexus Mods API key at
   <https://www.nexusmods.com/settings/api-keys> and save it as the repository
   secret `NEXUSMODS_API_KEY`.

Do not put the Nexus API key, game DLLs, or local paths in the repository.

## Each release

1. From the repository root, prepare the next version:

   ```text
   python scripts/release.py prepare v0.6.0-alpha
   ```

   This synchronizes the version in `IsekaiHero.json`, `README.md`, and the
   checked-in Nexus description, then creates a changelog section when needed.
2. Replace the generated changelog `TODO` with player-facing notes. Review all
   counts and feature claims in `docs/nexus-mods-description.txt`, then paste
   that file into the Nexus mod page.
3. Commit the release metadata and merge or push it to `main`. The **CI**
   workflow will compile the mod in GitHub's cloud and reject mismatched
   versions, missing notes, placeholders, and packaging failures.
4. Open **Actions > Release > Run workflow**, choose `main`, enter the exact
   version, and check the Nexus-description confirmation. Leave the Nexus
   upload enabled for a normal public release.

The release job runs `dotnet build`, packages the generated PCK, creates and
verifies the three-file installable zip, creates the matching tag and GitHub
release, and uploads the same zip to Nexus Mods. The changelog section becomes
both the GitHub release body and the Nexus file-version description. Existing
Nexus file versions are archived automatically.

The official Nexus upload API currently updates an existing file version, not
the full mod-page description. The confirmation checkbox is therefore an
intentional final guardrail for the checked-in BBCode description.

For a GitHub-only recovery or test, uncheck **Upload the package to Nexus
Mods**. Re-running the same version is safe only while the tag still points to
the same commit; do not re-run the Nexus upload after it has succeeded.
