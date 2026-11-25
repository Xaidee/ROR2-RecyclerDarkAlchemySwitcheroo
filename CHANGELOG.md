# 1.1.0
 - Rebalance behaviour:
   + **Recycled items can't be recycled again**: Reimplemented addition and check for recycle pickup tag which is also applied to the additional Item created.
   + **Transmuted items can now be infinitely transmuted, limited by the number of pages itself**: Remove addition and check for recycle pickup tag.
   + **Both** Recycler and Dark Alchemy utilise the <code>ObjectiveRelated</code> and <code>AlchemyBlacklist</code> Item Tag in determining whether a pickup is valid to use on, and can transform Ethereal items.
 - Quality compatibility:
   + Quality Recyclers keep the quality of an item in its recycled output, limited by the Recycler's own quality.
   + Dark Alchemy's description clarifies the quality will also be kept (No actual changes to the item's mechanics).
 - Remove GreenShell from the game.
# 1.0.0
 - Initial release.