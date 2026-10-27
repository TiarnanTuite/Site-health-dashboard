# Retro Log

## Sprint 0: Setup (2026-10-27)

**Went well:**

- Solution scaffolded with a clean layered structure; builds with no errors
- First full PR cycle completed (branch, push, PR, merge)
- Branch protection on `main` set up, so the PR workflow is enforced, not just intended
- Wrote ADR-001 and understand the reasoning behind the project dependencies

**Didn't go well:**

- GitHub push failed on first attempt; password auth isn't supported, had to authenticate through the browser
- Created `SHM.Web` in the wrong folder and had to delete and recreate it under `src/`
- Scaffold commit went straight to `main` instead of through a PR
- Docs PR was closed without merging, then the branch was deleted locally. `git branch -d` didn't warn me, because the branch was pushed to GitHub, which counted as "merged"
- Got stuck in Vim during a merge commit

**Change next sprint:**

- Check for the purple "Merged" label before deleting any branch
- Run `git status` before every commit
- Use `-o` with `dotnet new` and double-check the output path before running
- Set VS Code as the Git editor (`git config --global core.editor "code --wait"`)
