# Cthangover: Story — 主游戏

Cthangover 是一款黑色幽默的视觉小说，讲述马劳在糟透的一夜之后在小巷里醒来：失忆、裹着一块红布、身边还有一本他根本不记得买过的旧书。本模组是游戏自身的内容：场景、剧本、角色、物品、任务、世界地图、音乐选择和故事文本。其余一切——对话、背包、战斗、地图界面——都由游戏所组合的引擎模组和玩法模组提供。

从“开始游戏 → Cthangover → 新游戏”进入游戏。

![新游戏的第一幕：马劳裹着红布站在昏暗的小巷里，旁白描述着他的宿醉。序章文本目前仅有俄语。](assets/opening.jpg)

## 游戏内容

- **序章。** 故事从小巷开始，途经墓地，再沿河岸一直走到一栋旧别墅。序章以小说方式呈现：一行行文本、马劳的立绘和各种选择。
- **别墅与小镇。** 序章之后，你通过选项在各个地点之间移动：别墅外、别墅的各个房间（门厅、客厅、厨房、卧室、工作室、地下室）、小镇入口和郊外。
- **生存细节。** 新游戏开始时，背包里就有狼肉和柴火；厨房会统计口粮及其每日消耗，还可以做饭。
- **任务。** 日志记录故事中的任务，从“苏醒”和“老房子”开始。
- **世界地图。** 地图面板显示城区中可以前往的地点。
- **战斗。** 战斗默认采用卡牌模式；可以在游戏设置中切换为指令式战斗。

![正午的别墅外：瀑布旁的磨坊小屋，以及“进屋”“去镇上”“去森林”三个选项。](assets/villa-outside.jpg)

![厨房：左上角的口粮计数器和“离开”“做饭”两个选项。](assets/kitchen.jpg)

![北城区的世界地图，标有别墅和小镇入口的标记。](assets/world-map.jpg)

![镇中的一场卡牌战斗：马劳和一名同伴对阵两只狼。](assets/battle.jpg)

## 语言

序章剧本以俄语写成，尚无翻译，因此无论界面是什么语言，这部分文本都会显示俄语。别墅和小镇场景、任务日志、物品和菜单均已翻译。

## 游戏的组成

游戏就是合集 **Cthangover**。它的各个分组说明了每个模组的分工：

```text
剧情          cthangover_game, cthangover_murakami, cthangover_party, town_pack
2D 引擎       外壳、对话、场景交互、时间与光照、特效、世界地图
角色          角色和技能
任务          日志
物品          背包、烹饪
战斗          回合制平台 + 卡牌战斗（默认核心）
战斗（FF）    指令式战斗核心（可选）
招募          招募被击败的角色（可选）
Live2D        动态立绘（可选）
界面          UI 组件库、菜单、音频
开发者工具（可选）  编辑器、验证器、图集、模组管理器
```

- **Cthangover: Murakami** 增加一条独立剧情线，**Cthangover: Starter Party** 则在开局就把马劳放进队伍。它们属于同一个游戏，但彼此是独立的模组。
- **Town Pack** 提供小镇入口和门厅的图片，**Music Pack** 提供配乐。本模组为小镇入口场景单独添加了一个播放列表。
- 合集将 `battle.core` 设为 `card_battle`。你可以在 **Mods & Collections → 合集 → Cthangover → 编辑结构** 中修改它，也可以调整任意分组的模组；存档不会受影响。

## 给模组作者

内容都放在普通文件夹里：

```filestree
cthangover_game/
├── scenes/          每个地点一个 JSON：名称、背景、环境音、默认剧本
├── scenarios/       prolog/、home/、town/ 以及 start.scenario
├── characters/      玩家、敌人、技能
├── items/           食物、食谱、资源
├── quests/          quests.json
├── interactives/    序章场景中可点击的对象
├── map/map.json     世界地图的区域和标记
├── backgrounds/, avatars/, sounds/, effects/, shaders/
└── locale/          七种语言的名称、任务和剧本翻译
```

新游戏从 `start_scene` 开始，其剧本会发放初始物品并切换到小巷：

```scenario
scene: start_scene
priority: 0
is_one_run: true
---
action item.add id=food/wolf_meat count=10
action item.add id=resource/branches count=10
switch_scene prolog_alley
end
```

想在不改动本模组的情况下修改场景文本，可以在自己的模组里为同一场景添加剧本，或者声明对 `cthangover_game` 的依赖，用同名文件进行覆盖。

## 出问题了怎么办

1. **游戏卡片提示尚未就绪。** 在“开始游戏”中使用“安装缺失项”，或者检查合集结构。
2. **战斗无法开始。** 所选的战斗核心必须包含在游戏中。
3. **故事文本是俄语。** 那是序章（见“语言”一节）。
