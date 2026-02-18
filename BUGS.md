# Bugs Found

1. LRS/CRS grid ordering placed current region off-center.
   Status: Fixed (reordered LRS region list to row-major with center).

2. LRS bottom-right cell sometimes blank due to null counts (nebula/unknown).
   Status: Fixed (default null counts to 0; mark nebula scan as Unknown).

3. Prompt flow crashes or misroutes on empty/invalid navigation input.
   Status: Fixed (safe parsing + reprompt for impulse/warp prompts).

4. `wrp` command showed subcommands but they were not wired to prompt flow.
   Status: Fixed (wrp now enters prompt-driven warp flow).
