# Recycler Dark Alchemy Switcheroo
Swaps Recycler and Dark Alchemy's functionality with each other.

Also includes a config option to disable *recyclying* hexes (Enabled by default).

|                                                                      | Pickup               | Description                                                                                                    |
|:---------------------------------------------------------------------------------:|----------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|     ![Recycler](https://riskofrain2.wiki.gg/images/Recycler.png)<br/>Recycler     | Recycle items.     | **Recycles** a targeted Item into two Items of a **lesser tier**. *Recycled items can't be recycled again.*                                             |
| ![Dark Alchemy](https://riskofrain2.wiki.gg/images/Dark_Alchemy.png)<br/>Dark Alchemy | Equivalent exchange. | **Transmute**  an Item or Equipment into a different one of the **same tier**.  |

## Quality Compatibility
<details>
<summary>Changes when Quality is installed...</summary>

Recycler has explicit interactions with quality items, Dark Alchemy gets a clarifying description change.

| | Pickup | Description |
|:-:|---|---|
| ![Recycler (Uncommon)](https://raw.githubusercontent.com/Goorakh/RoR2_ItemQualities/refs/heads/main/ItemQualities/Assets/ItemQualities/Assets/Equipment/Recycle/texRecycleUncommon.png)<br/>Recycler (Uncommon)  | Recycled items can be quality.  | Recycled items can be <sprite name="Quality"> quality. All items above uncommon <sprite name="QualityUncommon"> quality are recycled into uncommon <sprite name="QualityUncommon"> quality. |
| ![Recycler (Rare)](https://raw.githubusercontent.com/Goorakh/RoR2_ItemQualities/refs/heads/main/ItemQualities/Assets/ItemQualities/Assets/Equipment/Recycle/texRecycleRare.png)<br/>Recycler (Uncommon)  | Recycled items can be quality.  | Recycled items can be <sprite name="Quality"> quality. All items above rare <sprite name="QualityRare"> quality are recycled into rare <sprite name="QualityRare"> quality. |
| ![Recycler (Epic)](https://raw.githubusercontent.com/Goorakh/RoR2_ItemQualities/refs/heads/main/ItemQualities/Assets/ItemQualities/Assets/Equipment/Recycle/texRecycleEpic.png)<br/>Recycler (Uncommon)  | Recycled items can be quality.  | Recycled items can be <sprite name="Quality"> quality. All items above epic <sprite name="QualityEpic"> quality are recycled into epic <sprite name="QualityEpic"> quality. |
| ![Recycler (Legendary)](https://raw.githubusercontent.com/Goorakh/RoR2_ItemQualities/refs/heads/main/ItemQualities/Assets/ItemQualities/Assets/Equipment/Recycle/texRecycleLegendary.png)<br/>Recycler (Uncommon)  | Recycled items can be quality.  | Recycled items can keep their <sprite name="Quality"> quality. |
  | ![Dark Alchemy](https://riskofrain2.wiki.gg/images/Dark_Alchemy.png)<br/>Dark Alchemy | Equivalent exchange. | **Transmute**  an Item or Equipment into a different one of the **same tier and quality**.  |

</details>